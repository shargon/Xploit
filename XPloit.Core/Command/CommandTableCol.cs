using System;

namespace XPloit.Core.Command
{
    public class CommandTableCol
    {
        public enum EAlign
        {
            /// <summary>
            /// Left align
            /// </summary>
            Left,
            /// <summary>
            /// Center align
            /// </summary>
            Center,
            /// <summary>
            /// Right align
            /// </summary>
            Right,
            /// <summary>
            /// No intercept
            /// </summary>
            None
        }

        /// <summary>
        /// Align
        /// </summary>
        public EAlign Align { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// ConsoleColor
        /// </summary>
        public ConsoleColor ForeColor { get; set; }

        int _Index;
        CommandTableRow _Parent;
        /// <summary>
        /// Replicated char
        /// </summary>
        public char ReplicatedChar { get; set; }

        /// <summary>
        /// Index
        /// </summary>
        public int Index { get { return _Index; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public CommandTableCol(int index, CommandTableRow parent)
        {
            Align = EAlign.Left;
            _Index = index;
            _Parent = parent;
            ForeColor = ConsoleColor.Gray;
            ReplicatedChar = '\0';
        }

        public string GetFormatedValue()
        {
            string val = Value == null ? "" : Value;

            int l = _Parent.Parent.GetLength(_Index);

            if (ReplicatedChar != '\0')
            {
                val = val.PadLeft(l, ReplicatedChar);
                return val;
            }

            if (val.Length > l)
            {
                if (Align != EAlign.None)
                    val = val.Substring(0, l);
            }
            else
            {
                switch (Align)
                {
                    case CommandTableCol.EAlign.Left: val = val.PadRight(l, ' '); break;
                    case CommandTableCol.EAlign.Right: val = val.PadLeft(l, ' '); break;
                    case CommandTableCol.EAlign.Center:
                        {
                            int l2 = l - val.Length;
                            int li = l2 / 2;
                            int lr = l2 - li;
                            val = "".PadLeft(li, ' ') + val + "".PadRight(lr, ' ');
                            break;
                        }
                }
            }
            return val;
        }
    }
}