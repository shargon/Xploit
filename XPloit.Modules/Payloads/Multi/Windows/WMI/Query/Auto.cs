using Auxiliary.Multi.Windows;
using System;
using System.Management;
using XPloit.Core;
using XPloit.Core.Attributes;
using XPloit.Core.Command;

namespace Payloads.Multi.Windows.WMI.Query
{
    public class Auto : Payload, WMIManager.IWMIPayload
    {
        #region Configure
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Execute a default query in WMI"; } }
        #endregion

        public enum EList
        {
            Process,
            Apps
        }

        #region Properties
        [ConfigurableProperty(Required = true, Description = "Query the List (Process,Apps)")]
        public EList List { get; set; }
        #endregion

        public bool Run(ManagementScope connection)
        {
            string wmiql = "";

            int omit = -1;
            string[] fields = null;
            CommandTable table = new CommandTable();

            switch (List)
            {
                case EList.Process:
                    {
                        WriteInfo("-----------------------------------");
                        WriteInfo("Win32_Process instance");
                        WriteInfo("-----------------------------------");

                        wmiql = "SELECT * FROM Win32_Process";
                        fields = new string[] { "ProcessId", "Name", "CommandLine" };

                        omit = 2;
                        table.AddRow(table.AddRow("ProcessId", "Name", "CommandLine").MakeSeparator());
                        break;
                    }
                case EList.Apps:
                    {

                        WriteInfo("-----------------------------------");
                        WriteInfo("Win32_Product instance");
                        WriteInfo("-----------------------------------");

                        wmiql = "SELECT * FROM Win32_Product";
                        fields = new string[] { "Version", "Name" };

                        omit = 1;
                        table.AddRow(table.AddRow("Version", "Name").MakeSeparator());
                        break;
                    }
            }

            int mx = fields.Length;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connection, new ObjectQuery(wmiql));
            foreach (ManagementObject q in searcher.Get())
            {
                string[] data = new string[mx];
                for (int x = 0; x < mx; x++)
                    try
                    {
                        data[x] = q[fields[x]].ToString();
                    }
                    catch { }

                CommandTableRow row = table.AddRow(omit, data);
                row[0].ForeColor = ConsoleColor.DarkGray;
                row[0].Align = CommandTableCol.EAlign.Right;
                row[omit].Align = CommandTableCol.EAlign.None;
            }

            WriteTable(table);

            return true;
        }
    }
}