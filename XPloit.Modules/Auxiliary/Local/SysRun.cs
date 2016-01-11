using System;
using System.Diagnostics;
using System.Security;
using System.Text;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;

namespace Auxiliary.Local
{
    public class SysRun : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Execute a system command in local machine"; } }
        public override DateTime DisclosureDate { get { return DateTime.MinValue; } }
        public override bool IsLocal { get { return true; } }
        public override Reference[] References
        {
            get
            {
                return new Reference[] 
                { 
                    new Reference(EReferenceType.URL, "https://msdn.microsoft.com/es-es/library/system.diagnostics.processstartinfo(v=vs.110).aspx") ,
                    new Reference(EReferenceType.URL,"http://referencesource.microsoft.com/#System/services/monitoring/system/diagnosticts/ProcessStartInfo.cs")
                };
            }
        }
        #endregion

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
        [ConfigurableProperty(Description = "Return output")]
        public bool ReturnOutput { get; set; }
        [ConfigurableProperty(Description = "Process Window Style")]
        public ProcessWindowStyle ProcessWindowStyle { get; set; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SysRun()
        {
            // Default variables
            Arguments = null;
            FileName = null;
            Password = null;
            UserName = null;
            Domain = null;

            ReturnOutput = false;
            CreateWindow = false;
            UseShellExecute = false;
            Verb = null;
            ProcessWindowStyle = ProcessWindowStyle.Hidden;
        }

        public ProcessStartInfo GetProcessStartInfo()
        {
            ProcessStartInfo pi = new ProcessStartInfo()
            {
                Arguments = this.Arguments,
                CreateNoWindow = !this.CreateWindow,
                Domain = this.Domain,
                FileName = this.FileName,
                UserName = this.UserName,
                UseShellExecute = this.UseShellExecute,
                Verb = this.Verb,
                WindowStyle = (ProcessWindowStyle)this.ProcessWindowStyle,
            };

            if (ReturnOutput)
            {
                pi.RedirectStandardOutput = true;
                pi.RedirectStandardError = true;
            }

            if (!string.IsNullOrEmpty(this.Password))
            {
                pi.Password = new SecureString();
                foreach (char c in this.Password.ToCharArray())
                    pi.Password.AppendChar(c);
            }

            return pi;
        }
        public override bool Run()
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            ProcessStartInfo info = GetProcessStartInfo();
            if (info == null) return false;

            using (Process pr = new Process())
            {
                pr.StartInfo = info;
                pr.EnableRaisingEvents = true;
                pr.OutputDataReceived += (object e, DataReceivedEventArgs outLine) =>
                {
                    output.AppendLine(outLine.Data);
                };
                pr.ErrorDataReceived += (object e, DataReceivedEventArgs outLine) =>
                {
                    error.AppendLine(outLine.Data);
                };

                if (pr.Start())
                {
                    WriteInfo("Executed in pid ", pr.Id.ToString(), ConsoleColor.Green);

                    if (ReturnOutput)
                    {
                        pr.BeginOutputReadLine();
                        pr.BeginErrorReadLine();
                        pr.WaitForExit();

                        if (error.ToString().Trim().Length > 0)
                            WriteError("Error" + Environment.NewLine + error.ToString().Trim());

                        if (output.ToString().Trim().Length > 0)
                            WriteInfo("Output" + Environment.NewLine + output.ToString().Trim());
                    }

                    return true;
                }
                return false;
            }
        }
    }
}