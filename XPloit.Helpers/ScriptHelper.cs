using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace XPloit.Helpers
{
    public class ScriptHelper
    {
        public class ScriptOptions
        {
            /// <summary>
            /// Include files
            /// </summary>
            public string[] IncludeFiles { get; set; }
            /// <summary>
            /// Extra Usings
            /// </summary>
            public string[] includeUsings { get; set; }
            /// <summary>
            /// Inherited 
            /// </summary>
            public Type[] Inherited { get; set; }
        }

        /// <summary>
        /// Default Core options
        /// </summary>
        public static ScriptOptions DefaultCoreOptions
        {
            get
            {
                return new ScriptOptions()
                {
                    // Core file
                    IncludeFiles = new string[] { Assembly.GetExecutingAssembly().Location },
                    // Using core
                    includeUsings = new string[] { "XPloit.Core.Helpers", "XPloit.Core.Extensions" }
                };
            }
        }

        Type _TypeAsm;
        Assembly _Asm;
        static Dictionary<string, ScriptHelper> _AsmLoaded = new Dictionary<string, ScriptHelper>();

        /// <summary>
        /// Type Asm
        /// </summary>
        public Type Type { get { return _TypeAsm; } }

        /// <summary>
        /// Create a Script from File
        /// </summary>
        /// <param name="fileName">Script file</param>
        /// <param name="options">Options</param>
        public static ScriptHelper CreateFromFile(string fileName, ScriptOptions options)
        {
            return CreateFromString(File.ReadAllText(fileName), options);
        }
        /// <summary>
        /// Create a Script from String
        /// </summary>
        /// <param name="codeOrHash">C# Code or hash</param>
        /// <param name="options">Options</param>
        public static ScriptHelper CreateFromString(string codeOrHash, ScriptOptions options)
        {
            if (string.IsNullOrEmpty(codeOrHash)) return null;

            ScriptHelper ret = null;
            if (codeOrHash.Length == 64)
            {
                // Hash
                if (_AsmLoaded.TryGetValue(codeOrHash, out ret)) return ret;
            }

            string hash = HashHelper.HashHex(HashHelper.EHashType.Sha256, Encoding.UTF8, codeOrHash);
            if (_AsmLoaded.TryGetValue(hash, out ret)) return ret;

            List<string> asms = new List<string>(new string[]
            {
                "system.dll",
                "system.xml.dll",
                "system.data.dll",
                "system.web.dll",
                "system.windows.forms.dll",
                "system.drawing.dll" ,
            });

            // Append files
            if (options != null && options.IncludeFiles != null)
                foreach (string su in options.IncludeFiles)
                    if (!asms.Contains(su)) asms.Add(su);

            List<string> usings = new List<string>(new string[]
            {
               "System",
                "System.Data",
                "System.Collections.Generic",
                "System.Drawing.Imaging",
                "System.IO",
                "System.Web",
                "System.Net",
                "System.Net.NetworkInformation",
                "System.IO.Ports",
                "System.Windows.Forms",
                "System.Drawing",
                "System.Text",
                "System.Xml",
                "System.Drawing.Printing",
                "System.Data.OleDb",
                "System.Data.Odbc",
                "System.Text.RegularExpressions",
                "System.ComponentModel",
                "System.Threading"
            });

            // Append usings
            if (options != null && options.includeUsings != null)
                foreach (string su in options.includeUsings)
                    if (!usings.Contains(su)) usings.Add(su);

            string addUsing = "";
            foreach (string su in usings)
                addUsing += "using " + su + ";";

            string herencia = "";
            if (options != null && options.Inherited != null)
            {
                foreach (Type t in options.Inherited)
                {
                    if (t == null) continue;

                    if (herencia != "") herencia += ",";
                    else herencia += ":";

                    herencia += t.FullName.Replace("+", ".");
                }
            }

            codeOrHash = addUsing + " namespace DummyNamespace{public class DummyClass" + herencia + "{public DummyClass(){}" + codeOrHash + "\n}}";

            Assembly asm = Compile(codeOrHash, asms.ToArray());
            if (asm == null) return null;

            ret = new ScriptHelper(asm);
            _AsmLoaded.Add(hash, ret);
            return ret;
        }

        ScriptHelper(Assembly asm)
        {
            _Asm = asm;

            //  Search default exported class
            foreach (Type typ in _Asm.GetExportedTypes())
            {
                if (!typ.Attributes.HasFlag(TypeAttributes.Public)) continue;

                if (_TypeAsm != null)
                    throw (new Exception("Multiple public Types exported (Require only one)"));
                _TypeAsm = typ;
            }

            if (_TypeAsm == null)
                throw (new Exception("No public Type exported"));
        }

        /// <summary>
        /// Create Instance
        /// </summary>
        public dynamic CreateNewInstance()
        {
            if (_Asm == null) return null;
            return Activator.CreateInstance(_TypeAsm);
        }
        /// <summary>
        /// Create Instance
        /// </summary>
        public T CreateNewInstance<T>()
        {
            if (_Asm == null) return default(T);
            return (T)Activator.CreateInstance(_TypeAsm);
        }

        #region Compile
        static Assembly Compile(string code, string[] refAsms) { return Compile(code, refAsms, null); }
        static Assembly Compile(string code, string[] refAsms, string fileDest)
        {
            CompilerParameters compilerparams = new CompilerParameters();
            if (fileDest == null)
            {
                compilerparams.GenerateExecutable = false;
                compilerparams.GenerateInMemory = true;
            }
            else
            {
                compilerparams.GenerateExecutable = true;
                compilerparams.GenerateInMemory = false;
                compilerparams.OutputAssembly = fileDest;
            }
            if (refAsms != null) foreach (string rf in refAsms) compilerparams.ReferencedAssemblies.Add(rf);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(compilerparams, code);
            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compilation errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat(" > Line {0},{1}\t: {2}\n", error.Line, error.Column, error.ErrorText);
                }
                throw new Exception(errors.ToString());
            }
            return results.CompiledAssembly;
        }
        #endregion
    }
}