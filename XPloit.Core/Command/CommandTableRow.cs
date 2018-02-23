using System;
using System.Collections.Generic;
using System.Text;

namespace XPloit.Core.Command
{
    public class CommandTableRow : List<CommandTableCol>
    {
        /// <summary>
        /// Parent
        /// </summary>
        public CommandTable Parent { get { return _Parent; } }

        CommandTable _Parent = null;
        public CommandTableRow(CommandTable Owner)
        {
            if (Owner == null) throw new ArgumentException("Owner");
            _Parent = Owner;
        }
        public String Output()
        {
            StringBuilder sb = new StringBuilder();
            Output(sb);
            return sb.ToString();
        }
        public void Output(StringBuilder sb)
        {
            string separator = _Parent.Separator;
            foreach (CommandTableCol col in this)
            {
                if (col.Index != 0) sb.Append(separator);
                sb.Append(col.GetFormatedValue());
            }

            sb.AppendLine();
        }

        /// <summary>
        /// Copy values from this row to a array with the character '─'
        /// </summary>
        public string[] MakeSeparator() { return MakeSeparator('─'); }
        /// <summary>
        /// Copy values from this row to a array with the specified character
        /// </summary>
        /// <param name="ch">Char</param>
        public string[] MakeSeparator(char ch)
        {
            string[] ret = new string[Count];
            for (int x = 0, m = ret.Length; x < m; x++)
                ret[x] = "".PadLeft(this[x].Value.Length, ch);

            return ret;
        }
    }
}