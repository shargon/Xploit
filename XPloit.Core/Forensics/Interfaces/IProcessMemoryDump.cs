using System;
using System.Diagnostics;

namespace XPloit.Core.Forensics.Interfaces
{
    public class IProcessMemoryDump : IDisposable
    {
        Process _Process;
        /// <summary>
        /// Process
        /// </summary>
        public Process Process { get { return _Process; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processId">Process Id</param>
        protected IProcessMemoryDump(int processId)
        {
            _Process = Process.GetProcessById(processId);
        }
        /// <summary>
        /// Dump current process memory to File
        /// </summary>
        /// <param name="file">File</param>
        /// <returns>Return File size</returns>
        public virtual long ProcessMemoryDump(string file) { return -1; }
        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            if (_Process != null)
            {
                _Process.Dispose();
                _Process = null;
            }
        }
    }
}