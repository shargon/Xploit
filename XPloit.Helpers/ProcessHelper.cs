using XPloit.Helpers.Forensics;
using XPloit.Helpers.Interfaces;

namespace XPloit.Helpers
{
    public class ProcessHelper
    {
        /// <summary>
        /// Make a memory dump for the selected ProcessID
        /// </summary>
        /// <param name="processId">Process Id</param>
        /// <param name="file">File</param>
        /// <returns>Return file size</returns>
        public static long ProcessMemoryDump(int processId, string file)
        {
            IProcessMemoryDump dump = null;

            if (SystemHelper.IsWindows) dump = GetWindowsProcessMemoryDump(processId);
            else
            {
                if (SystemHelper.IsLinux) dump = GetLinuxProcessMemoryDump(processId);
            }

            if (dump == null) return -1;

            using (dump)
            {
                return dump.ProcessMemoryDump(file);
            }
        }

        static IProcessMemoryDump GetWindowsProcessMemoryDump(int processId) { return new WindowsProcessMemoryDump(processId); }
        static IProcessMemoryDump GetLinuxProcessMemoryDump(int processId) { return new LinuxProcessMemoryDump(processId); }
    }
}