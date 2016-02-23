using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using XPloit.Core.Helpers;
using XPloit.Core.Listeners.Layer;

namespace XPloit.Core.Command
{
    public class CommandTable : IEnumerable<CommandTableRow>
    {
        public String Separator { get; set; }

        List<CommandTableRow> rows = new List<CommandTableRow>();
        List<int> colLength = new List<int>();

        public int Count { get { return rows.Count; } }
        public int GetLength(int index) { return colLength[index]; }
        public CommandTable() { Separator = "  "; }
        public CommandTable(String separator) : this() { Separator = separator; }
        public CommandTable(DataTable dt)
        {
            int cols = dt.Columns.Count, x = 0;
            string[] row = new string[cols];

            foreach (DataColumn dc in dt.Columns)
            {
                row[x] = dc.ColumnName;
                x++;
            }

            AddRow(row);
            AddSeparator(cols, '-');

            foreach (DataRow dr in dt.Rows)
            {
                x = 0;
                foreach (string c in row)
                {
                    row[x] = ConvertHelper.ToString(dr[x]);
                    x++;
                }

                AddRow(row);
            }
        }

        public CommandTableRow AddSeparator(int cols, char ch)
        {
            CommandTableRow row = new CommandTableRow(this);

            for (int x = 0; x < cols; x++)
            {
                row.Add(new CommandTableCol(x, row) { ReplicatedChar = ch });
            }

            rows.Add(row);
            return row;
        }
        public CommandTableRow AddRow(params string[] cols)
        {
            return AddRow(-1, cols);
        }
        public CommandTableRow AddRow(int ommitFieldInLength, params string[] cols)
        {
            CommandTableRow row = new CommandTableRow(this);
            int ix = 0;
            foreach (string o in cols)
            {
                string str = o == null ? "" : o.ToString();//.Trim();
                int sl = str.Length;

                row.Add(new CommandTableCol(ix, row) { Value = str });
                if (ix == ommitFieldInLength) sl = 0;
                ix++;

                if (colLength.Count >= row.Count)
                {
                    int curLength = colLength[row.Count - 1];
                    if (sl > curLength) colLength[row.Count - 1] = sl;
                }
                else
                {
                    colLength.Add(sl);
                }
            }
            rows.Add(row);
            return row;
        }

        public IEnumerable<CommandTableRow> AddSplitRow(int ommitFieldInLength, params string[] cols)
        {
            int maxLines = 1;
            List<string[]> lines = new List<string[]>();
            foreach (string o in cols)
            {
                string[] step = o.Replace("\r", "").Trim(new char[] { '\n', '\r' }).Split(new char[] { '\r', '\n' }, StringSplitOptions.None);
                maxLines = Math.Max(step.Length, maxLines);
                lines.Add(step);
            }

            for (int x = 0; x < maxLines; x++)
            {
                CommandTableRow row = new CommandTableRow(this);
                int ix = 0;
                foreach (string[] o in lines)
                {
                    string str = o == null || o.Length <= x ? "" : o[x].ToString();//.Trim();
                    int sl = str.Length;

                    row.Add(new CommandTableCol(ix, row) { Value = str });
                    if (ix == ommitFieldInLength) sl = 0;
                    ix++;

                    if (colLength.Count >= row.Count)
                    {
                        int curLength = colLength[row.Count - 1];
                        if (sl > curLength) colLength[row.Count - 1] = sl;
                    }
                    else
                    {
                        colLength.Add(sl);
                    }
                }
                rows.Add(row);

                yield return row;
            }
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

        public void OutputColored(CommandLayer io)
        {
            string separator = Separator;
            foreach (CommandTableRow row in this)
            {
                foreach (CommandTableCol col in row)
                {
                    io.SetForeColor(col.ForeColor);

                    if (col.Index != 0) io.Write(separator);
                    io.Write(col.GetFormatedValue());
                }
                io.WriteLine("");
            }

        }
    }
}