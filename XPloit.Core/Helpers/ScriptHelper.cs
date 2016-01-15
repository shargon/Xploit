using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace XPloit.Core.Helpers
{
    public class ScriptHelper
    {
        Type _TypeAsm;
        Assembly _Asm;
        static Dictionary<string, ScriptHelper> _AsmLoaded = new Dictionary<string, ScriptHelper>();

        /// <summary>
        /// Type Asm
        /// </summary>
        public Type Type { get { return _TypeAsm; } }

        /// <summary>
        /// Sample:
        ///     GetAssembly("public static void MACRO(DB db,DataSet ds) { /*CODE*/ } ");
        /// </summary>
        /// <param name="fileName">Assembly file</param>
        /// <param name="isSource">True if contains source</param>
        public static ScriptHelper Create(string fileName, bool isSource)
        {
            if (isSource)
            {
                return Create(File.ReadAllText(fileName));
            }
            return Create(File.ReadAllBytes(fileName));
        }
        /// <summary>
        /// Sample:
        ///     GetAssembly("public static void MACRO(DB db,DataSet ds) { /*CODE*/ } ");
        /// </summary>
        /// <param name="data">Assembly data</param>
        /// <returns></returns>
        public static ScriptHelper Create(byte[] data)
        {
            string hash = HashHelper.HashHex(HashHelper.EHashType.Sha256, data, 0, data.Length);

            ScriptHelper ret;
            if (_AsmLoaded.TryGetValue(hash, out ret)) return ret;

            Assembly asm = Assembly.Load(data);
            if (asm == null) return null;

            ret = new ScriptHelper(asm);
            _AsmLoaded.Add(hash, ret);
            return ret;
        }
        /// <summary>
        /// Sample:
        ///     GetAssembly("public static void MACRO(DB db,DataSet ds) { /*CODE*/ } ");
        /// </summary>
        /// <param name="codeOrHash">C# Code or hash</param>
        /// <returns></returns>
        public static ScriptHelper Create(string codeOrHash)
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

            string[] sasm = new string[]
            {
                "system.dll",
                "system.xml.dll",
                "system.data.dll",
                "system.web.dll",
                "system.windows.forms.dll",
                "system.drawing.dll" ,
            };

            codeOrHash =
                "using System;" +
                "using System.Data;" +
                "using System.Collections.Generic;" +
                "using System.Drawing.Imaging;" +
                "using System.IO;" +
                "using System.Web;" +
                "using System.Net;" +
                "using System.Net.NetworkInformation;" +
                "using System.IO.Ports;" +
                "using System.Windows.Forms;" +
                "using System.Drawing;" +
                "using System.Text;" +
                "using System.Xml;" +
                "using System.Drawing.Printing;" +
                "using System.Data.OleDb;" +
                "using System.Data.Odbc;" +
                "using System.Text.RegularExpressions;" +
                "using System.ComponentModel;" +
                "using System.Threading;" +

                "namespace DummyNamespace{public class DummyClass{public DummyClass(){}" + codeOrHash + "\n}}";

            Assembly asm = Compile(codeOrHash, sasm);
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