using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;

namespace Auxiliary.Local
{
    public class ProcessMemoryDump : Module
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Do a memory dump for the selected Process"; } }
        #endregion

        #region Properties
        [ConfigurableProperty(Required = true, Description = "Process ID")]
        public int? PID { get; set; }
        [ConfigurableProperty(Required = true, Description = "File where create the memory dump")]
        public FileInfo FileDump { get; set; }

        [ConfigurableProperty(Description = "Search a String in dump")]
        public string SearchString { get; set; }
        [ConfigurableProperty(Description = "Search mode for string comparison")]
        public StringComparison SearchMode { get; set; }
        [ConfigurableProperty(Description = "Make this iterations")]
        public int Iterations { get; set; }
        [ConfigurableProperty(Description = "Sleep between iterations")]
        public TimeSpan IterationSleep { get; set; }

        [ConfigurableProperty(Description = "Output data Length")]
        public int OutputDataLength { get; set; }
        [ConfigurableProperty(Description = "Output data Seek before search match")]
        public int OutputDataBefore { get; set; }
        #endregion

        public ProcessMemoryDump()
        {
            Iterations = 1;
            SearchMode = StringComparison.InvariantCultureIgnoreCase;
            IterationSleep = TimeSpan.FromMilliseconds(1000);
            OutputDataLength = 400;
            OutputDataBefore = 200;
        }

        public override ECheck Check()
        {
            Process pr = Process.GetProcessById(PID.Value);
            if (pr == null)
            {
                WriteInfo("Process not found");
                return ECheck.Error;
            }
            pr.Dispose();
            return ECheck.Ok;
        }

        [IsJobable]
        public override bool Run()
        {
            WriteInfo("Search process ...");

            Process pr = Process.GetProcessById(PID.Value);
            if (pr == null)
            {
                WriteInfo("Process not found");
                return false;
            }
            pr.Dispose();

            for (int x = 0, m = Iterations; x < m; x++)
            {
                WriteInfo("Making dump number", x.ToString().PadLeft(5, '0'), ConsoleColor.DarkGray);
                long size = ProcessHelper.ProcessMemoryDump(PID.Value, FileDump.FullName);
                WriteInfo("Done", StringHelper.Convert2KbWithBytes(size), size > 0 ? ConsoleColor.Green : ConsoleColor.Red);

                if (size > 0)
                {
                    // Search
                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        string ap = File.ReadAllText(FileDump.FullName, Encoding.ASCII);
                        int ix = ap.IndexOf(SearchString, 0, SearchMode);
                        if (ix >= 0)
                        {
                            WriteInfo("Search found! ", ix.ToString(), ConsoleColor.Green);

                            ap = ap.Remove(0, Math.Max(0, ix - OutputDataBefore));
                            ap = ap.Substring(0, OutputDataLength).Replace("\0", "");

                            WriteInfo(ap);
                            break;
                        }
                        else WriteInfo("Search without any result");
                    }
                }

                if (x + 1 < m)
                {
                    // Not last
                    Thread.Sleep(IterationSleep);
                }
            }
            return true;
        }
    }
}
