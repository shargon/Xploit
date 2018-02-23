using Auxiliary.Local.Windows;
using System;
using System.Management;
using XPloit.Core;
using XPloit.Core.Command;
using XPloit.Helpers.Attributes;
using XPloit.Core.Attributes;

namespace Payloads.Local.Windows.WMI.Query
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Execute a default query in WMI")]
    public class Auto : Payload, WMIManager.IWMIPayload
    {
        public enum EList
        {
            Win32_Process,
            Win32_Product,
            Win32_DiskDrive,
            Win32_MemoryDevice,
            Win32_NetworkAdapterConfiguration,
            Win32_PhysicalMedia,
            Win32_PhysicalMemory,
            Win32_Processor,
        }

        #region Properties
        [ConfigurableProperty(Description = "Query the List (Win32_Process,Win32_Product)")]
        public EList List { get; set; }
        #endregion

        public bool Run(ManagementScope connection)
        {
            string wmiql = "";

            int omit = -1;
            string[] fields = null;
            CommandTableCol.EAlign first = CommandTableCol.EAlign.Left;
            CommandTable table = new CommandTable();

            switch (List)
            {
                case EList.Win32_Processor:
                    {
                        WriteInfo("------------------------");
                        WriteInfo("Win32_Processor instance");
                        WriteInfo("------------------------");

                        wmiql = "SELECT * FROM Win32_Processor";
                        fields = new string[] { "ProcessorId", "Caption", "Name", "Architecture" };

                        omit = -1;
                        table.AddRow(table.AddRow("ProcessorId", "Caption", "Name", "Architecture").MakeSeparator());
                        break;
                    }
                case EList.Win32_PhysicalMemory:
                    {
                        WriteInfo("----------------------------");
                        WriteInfo("Win32_PhysicalMemory instance");
                        WriteInfo("----------------------------");

                        wmiql = "SELECT * FROM Win32_PhysicalMemory";
                        fields = new string[] { "Caption", "PartNumber", "Speed", "Capacity" };

                        omit = -1;
                        table.AddRow(table.AddRow("Caption", "PartNumber", "Speed", "Capacity").MakeSeparator());
                        break;
                    }
                case EList.Win32_PhysicalMedia:
                    {
                        WriteInfo("----------------------------");
                        WriteInfo("Win32_PhysicalMedia instance");
                        WriteInfo("----------------------------");

                        wmiql = "SELECT * FROM Win32_PhysicalMedia";
                        fields = new string[] { "SerialNumber", "Tag" };

                        omit = -1;
                        table.AddRow(table.AddRow("SerialNumber", "Tag").MakeSeparator());
                        break;
                    }
                case EList.Win32_NetworkAdapterConfiguration:
                    {
                        WriteInfo("------------------------------------------");
                        WriteInfo("Win32_NetworkAdapterConfiguration instance");
                        WriteInfo("------------------------------------------");

                        wmiql = "SELECT * FROM Win32_NetworkAdapterConfiguration";
                        fields = new string[] { "Caption", "DHCPEnabled", "IPEnabled" };

                        omit = -1;
                        table.AddRow(table.AddRow("Caption", "DHCPEnabled", "IPEnabled").MakeSeparator());
                        break;
                    }
                case EList.Win32_MemoryDevice:
                    {
                        WriteInfo("---------------------------");
                        WriteInfo("Win32_MemoryDevice instance");
                        WriteInfo("---------------------------");

                        wmiql = "SELECT * FROM Win32_MemoryDevice";
                        fields = new string[] { "Caption", "StartingAddress", "EndingAddress" };

                        omit = -1;
                        table.AddRow(table.AddRow("Caption", "StartingAddress", "EndingAddress").MakeSeparator());
                        break;
                    }
                case EList.Win32_DiskDrive:
                    {
                        WriteInfo("------------------------");
                        WriteInfo("Win32_DiskDrive instance");
                        WriteInfo("------------------------");

                        wmiql = "SELECT * FROM Win32_DiskDrive";
                        fields = new string[] { "Name", "Model", "SerialNumber", "Size" };

                        omit = -1;
                        table.AddRow(table.AddRow("Name", "Model", "SerialNumber", "Size").MakeSeparator());
                        break;
                    }
                case EList.Win32_Process:
                    {
                        WriteInfo("----------------------");
                        WriteInfo("Win32_Process instance");
                        WriteInfo("----------------------");

                        wmiql = "SELECT * FROM Win32_Process";
                        fields = new string[] { "ProcessId", "Name", "CommandLine" };

                        omit = 2;
                        first = CommandTableCol.EAlign.Right;
                        table.AddRow(table.AddRow("ProcessId", "Name", "CommandLine").MakeSeparator());
                        break;
                    }
                case EList.Win32_Product:
                    {

                        WriteInfo("----------------------");
                        WriteInfo("Win32_Product instance");
                        WriteInfo("----------------------");

                        wmiql = "SELECT * FROM Win32_Product";
                        fields = new string[] { "Version", "Name" };

                        omit = 1;
                        first = CommandTableCol.EAlign.Right;
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
                        data[x] = q[fields[x]].ToString().Trim();
                    }
                    catch { }

                CommandTableRow row = table.AddRow(omit, data);
                row[0].ForeColor = ConsoleColor.DarkGray;
                row[0].Align = first;
                if (omit >= 0) row[omit].Align = CommandTableCol.EAlign.None;
            }

            WriteTable(table);

            return true;
        }

        public string GetWmicparams()
        {
            return "\"SELECT * FROM " + List.ToString() + "\"";
        }

        public bool ProcessOutPut(string output)
        {
            return false;
        }
    }
}