using Auxiliary.Local.Windows;
using System;
using System.Management;
using XPloit.Core;
using XPloit.Core.Command;
using XPloit.Core.Attributes;
using XPloit.Helpers.Attributes;

namespace Payloads.Local.Windows.WMI.Query
{
    [ModuleInfo(Author = "Fernando Díaz Toledano", Description = "Execute a default query in WMI")]
    public class Manual : Payload, WMIManager.IWMIPayload
    {
        #region Properties
        [ConfigurableProperty(Description = "Query for WMI")]
        public string Sql { get; set; }
        #endregion

        public Manual()
        {
            Sql = "SELECT * FROM Win32_Process";
        }

        public bool Run(ManagementScope connection)
        {
            CommandTable table = new CommandTable();
            table.AddRow(table.AddRow("Name", "Value").MakeSeparator());

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connection, new ObjectQuery(Sql));
            foreach (ManagementObject q in searcher.Get())
            {
                foreach (PropertyData p in q.Properties)
                {
                    try
                    {
                        CommandTableRow row = table.AddRow(1, new string[] { p.Name, p.Value.ToString() });
                        row[0].ForeColor = ConsoleColor.DarkGray;
                        row[1].Align = CommandTableCol.EAlign.None;
                    }
                    catch { }
                }

                table.AddSeparator(2, '-');
            }

            WriteTable(table);
            return true;
        }

        public string GetWmicparams()
        {
            // Windows
            return "\"" + Sql + "\"";
        }

        public bool ProcessOutPut(string output)
        {
            return false;
        }
    }
}