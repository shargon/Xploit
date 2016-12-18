using System;
using System.IO;
using System.Runtime.InteropServices;
using XPloit.Helpers.Interfaces;

namespace XPloit.Helpers.Forensics
{
    public class WindowsProcessMemoryDump : IProcessMemoryDump
    {
        #region APIS
        // REQUIRED CONSTS
        //const int PROCESS_WM_READ = 0x0010;
        //const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int MEM_COMMIT = 0x00001000;
        const int PAGE_READWRITE = 0x04;

        [Flags]
        enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        // REQUIRED STRUCTS
        [StructLayout(LayoutKind.Sequential)]
        struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        enum ProcessorArchitecture
        {
            X86 = 0,
            X64 = 9,
            @Arm = -1,
            Itanium = 6,
            Unknown = 0xFFFF,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_INFO
        {
            public ProcessorArchitecture ProcessorArchitecture; // WORD
            public uint PageSize; // DWORD
            public IntPtr MinimumApplicationAddress; // (long)void*
            public IntPtr MaximumApplicationAddress; // (long)void*
            public IntPtr ActiveProcessorMask; // DWORD*
            public uint NumberOfProcessors; // DWORD (WTF)
            public uint ProcessorType; // DWORD
            public uint AllocationGranularity; // DWORD
            public ushort ProcessorLevel; // WORD
            public ushort ProcessorRevision; // WORD
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processId">Process Id</param>
        public WindowsProcessMemoryDump(int processId) : base(processId) { }

        // http://www.codeproject.com/Articles/670373/Csharp-Read-Write-another-Process-Memory
        public int WriteProcessMemory(IntPtr address, byte[] buffer)
        {
            if (Process == null) return -1;

            IntPtr processHandle = OpenProcess(ProcessAccessFlags.All, false, Process.Id);

            int bytesWritten = 0;
            //byte[] buffer = Encoding.ASCII.GetBytes("tttt\0");
            // '\0' marks the end of string

            // replace 0x0046A3B8 with your address
            WriteProcessMemory(processHandle, address, buffer, new IntPtr(buffer.Length), ref bytesWritten);

            CloseHandle(processHandle);
            return bytesWritten;
        }
        public int ReadProcessMemory(IntPtr address, byte[] buffer, int length)
        {
            if (Process == null) return -1;

            IntPtr processHandle = OpenProcess(ProcessAccessFlags.All, false, Process.Id);

            int bytesWritten = 0;
            ReadProcessMemory(processHandle, address, buffer, new IntPtr(length), ref bytesWritten);

            CloseHandle(processHandle);
            return bytesWritten;
        }

        // http://www.codingvision.net/security/c-how-to-scan-a-process-memory
        public override long ProcessMemoryDump(string file)
        {
            if (Process == null) return -1;

            // getting minimum & maximum address
            SYSTEM_INFO sys_info;
            GetSystemInfo(out sys_info);

            IntPtr proc_min_address = sys_info.MinimumApplicationAddress;
            IntPtr proc_max_address = sys_info.MaximumApplicationAddress;

            // saving the values as long ints so I won't have to do a lot of casts later
            long proc_min_address_l = proc_min_address.ToInt64();
            long proc_max_address_l = proc_max_address.ToInt64();

            // opening the process with desired access level
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead, false, Process.Id);
            //IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_WM_READ, false, process.Id);

            int bytesRead = 0;  // number of bytes read with ReadProcessMemory
            MEMORY_BASIC_INFORMATION mem_basic_info;
            uint size = (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));

            //try
            //{
            long length = 0;
            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                // this will store any information we get from VirtualQueryEx()

                while (proc_min_address_l < proc_max_address_l)
                {
                    int result = VirtualQueryEx(processHandle, proc_min_address, out mem_basic_info, size);
                    long msize = mem_basic_info.RegionSize.ToInt64();

                    // if this memory chunk is accessible
                    if (mem_basic_info.Protect == PAGE_READWRITE && mem_basic_info.State == MEM_COMMIT)
                    {
                        byte[] buffer = new byte[msize];

                        // read everything in the buffer above
                        ReadProcessMemory(processHandle, mem_basic_info.BaseAddress, buffer, mem_basic_info.RegionSize, ref bytesRead);
                        fs.Write(buffer, 0, bytesRead);
                        length += bytesRead;
                    }

                    // move to the next memory chunk
                    proc_min_address_l += msize;
                    proc_min_address = new IntPtr(proc_min_address_l);
                }
            }
            //}
            //catch (Exception e)
            //{
            //}

            CloseHandle(processHandle);
            return length;
        }
    }
}