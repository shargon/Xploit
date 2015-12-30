using System.Diagnostics;
using System.Security;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Helpers;

namespace XPloit.Modules.Payloads.Multi
{
    public class ProcessStartPayload : Payload
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Execute a raw command"; } }
        public override string Name { get { return "ProcessStart"; } }
        public override string Path { get { return "Multi"; } }

        #region Properties
        [ConfigurableProperty(Description = "Domain")]
        public string Domain { get; set; }
        [ConfigurableProperty(Description = "Password for the User")]
        public string Password { get; set; }
        [ConfigurableProperty(Description = "Username")]
        public string UserName { get; set; }

        [ConfigurableProperty(Required = true, Description = "Path")]
        public string FileName { get; set; }
        [ConfigurableProperty(Description = "Arguments for the execution")]
        public string Arguments { get; set; }
        [ConfigurableProperty(Description = "Create Window?")]
        public bool CreateWindow { get; set; }
        [ConfigurableProperty(Description = "Use Shell Execute?")]
        public bool UseShellExecute { get; set; }
        [ConfigurableProperty(Description = "Windows verb")]
        public string Verb { get; set; }
        [ConfigurableProperty(Description = "Process Window Style")]
        public ProcessWindowStyle ProcessWindowStyle { get; set; }
        #endregion

        public ProcessStartPayload()
        {
            // Default variables
            Arguments = null;
            FileName = null;
            Password = null;
            UserName = null;
            Domain = null;

            CreateWindow = false;
            UseShellExecute = false;
            Verb = null;
            ProcessWindowStyle = ProcessWindowStyle.Hidden;
        }

        public class SerializableProcessStartInfo
        {
            public string Arguments { get; set; }
            public bool CreateNoWindow { get; set; }
            public string Domain { get; set; }
            public string FileName { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public bool UseShellExecute { get; set; }
            public string Verb { get; set; }
            public int WindowStyle { get; set; }

            public ProcessStartInfo ConvertToProcessStartInfo()
            {
                ProcessStartInfo pi = new ProcessStartInfo()
                {
                    Arguments = this.Arguments,
                    CreateNoWindow = this.CreateNoWindow,
                    Domain = this.Domain,
                    FileName = this.FileName,
                    UserName = this.UserName,
                    UseShellExecute = this.UseShellExecute,
                    Verb = this.Verb,
                    WindowStyle = (ProcessWindowStyle)this.WindowStyle,
                };

                if (!string.IsNullOrEmpty(this.Password))
                {
                    pi.Password = new SecureString();
                    foreach (char c in this.Password.ToCharArray())
                        pi.Password.AppendChar(c);
                }

                return pi;
            }
        }

        /// <summary>
        /// Payload Value
        /// </summary>
        public override byte[] Value
        {
            get
            {
                SerializableProcessStartInfo pi = new SerializableProcessStartInfo()
                 {
                     Arguments = this.Arguments,
                     CreateNoWindow = !this.CreateWindow,
                     Domain = this.Domain,
                     FileName = this.FileName,
                     UserName = this.UserName,
                     UseShellExecute = this.UseShellExecute,
                     Verb = this.Verb,
                     WindowStyle = (int)this.ProcessWindowStyle,
                     Password = this.Password
                 };

                return this.Encoding.GetBytes(JsonHelper.Serialize(pi, false, false));
            }
        }
    }
}