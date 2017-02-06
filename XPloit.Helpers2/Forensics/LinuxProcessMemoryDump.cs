using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using XPloit.Helpers.Interfaces;

namespace XPloit.Helpers.Forensics
{
    public class LinuxProcessMemoryDump : IProcessMemoryDump
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processId">Process Id</param>
        public LinuxProcessMemoryDump(int processId) : base(processId) { }

        public override long ProcessMemoryDump(string file)
        {
            if (Process == null) return -1;

            // TODO: Test
            //http://stackoverflow.com/questions/12977179/reading-living-process-memory-without-interrupting-it-proc-kcore-is-an-option
            // http://unix.stackexchange.com/questions/6301/how-do-i-read-from-proc-pid-mem-under-linux

            long length = 0;
            using (FileStream fwrite = new FileStream(file, FileMode.Create, FileAccess.Write))
            using (FileStream fmap = File.OpenRead("/proc/" + Process.Id.ToString() + "/maps"))
            using (StreamReader smap = new StreamReader(fmap))
            using (FileStream fmem = File.OpenRead("/proc/" + Process.Id.ToString() + "/mem"))
            {
                //memory_permissions = 'rw' if only_writable else 'r-'
                string line = null;
                string memory_permissions = "r-";

                Regex regex = new Regex("([0-9A-Fa-f]+)-([0-9A-Fa-f]+) ([-r][-w])");
                while ((line = smap.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    Match match = regex.Match(line);
                    if (!match.Success) continue;
                    if (match.Groups[3].Value == memory_permissions)
                    {
                        long start = Int64.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
                        if (start > 0xFFFFFFFFFFFF)
                            continue;

                        long end = Int64.Parse(match.Groups[2].Value, NumberStyles.HexNumber);
                        fmem.Seek(start, SeekOrigin.Begin); //  seek to region start

                        byte[] buffer = new byte[end - start];
                        int read = fmem.Read(buffer, 0, buffer.Length);
                        fwrite.Write(buffer, 0, read);
                        length += read;
                    }
                }
            }

            return length;
        }
    }
}