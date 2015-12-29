using System;
using System.Collections.Generic;
using System.Text;

namespace XPloit.Core.Command
{
    public class CommandTable : IEnumerable<CommandTableRow>
    {
        public String Separator { get; set; }

        List<CommandTableRow> rows = new List<CommandTableRow>();
        List<int> colLength = new List<int>();

        public int GetLength(int index) { return colLength[index]; }
        public CommandTable() { Separator = "  "; }
        public CommandTable(String separator) : this() { Separator = separator; }

        public CommandTableRow AddRow(params object[] cols)
        {
            CommandTableRow row = new CommandTableRow(this);
            int ix = 0;
            foreach (object o in cols)
            {
                string str = o == null ? "" : o.ToString().Trim();

                row.Add(new CommandTableCol(ix, row) { Value = str });
                ix++;

                if (colLength.Count >= row.Count)
                {
                    int curLength = colLength[row.Count - 1];
                    if (str.Length > curLength) colLength[row.Count - 1] = str.Length;
                }
                else
                {
                    colLength.Add(str.Length);
                }
            }
            rows.Add(row);
            return row;
        }

        public string Output()
        {
            StringBuilder sb = new StringBuilder();
            foreach (CommandTableRow row in rows)
                row.Output(sb);

            return sb.ToString();
        }

        #region IEnumerable Members
        public IEnumerator<CommandTableRow> GetEnumerator() { return rows.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return rows.GetEnumerator(); }
        #endregion
    }
}