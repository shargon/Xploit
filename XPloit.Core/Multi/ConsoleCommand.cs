using System;
using System.IO;
using System.Text;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Multi
{
    public class ConsoleCommand : ICommandLayer
    {
        Encoding _Codec;
        TextWriter _Out;
        TextReader _In;

        public event delOnAutoComplete OnAutoComplete = null;

        public delegate void delOnAutoComplete(string search, out string[] availables);

        public ConsoleCommand()
        {
            _Codec = Console.OutputEncoding;
            _Out = Console.Out;
            _In = Console.In;
        }
        public void Clear() { Console.Clear(); }
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="line">Line</param>
        public void Write(string line) { Write(line, false); }
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="appendNewLine">True for append newLine</param>
        public void Write(string line, bool appendNewLine)
        {
            if (line != null) _Out.Write(line);
            if (appendNewLine) _Out.Write(Environment.NewLine);
        }
        public string ReadLine()
        {
            string sInput = "";
            string onSearch = null;
            string[] autoComplete = null;
            int autoIndex = -1;

            for (; ; )
            {
                ConsoleKeyInfo myKey = Console.ReadKey(true);

                if (myKey.Key == ConsoleKey.Enter)
                {
                    Write(Environment.NewLine);
                    return sInput;
                }

                switch (myKey.Key)
                {
                    case ConsoleKey.Tab:
                        {
                            // Check in list 
                            if (autoComplete == null)
                            {
                                if (OnAutoComplete != null)
                                {
                                    onSearch = sInput;
                                    OnAutoComplete(sInput, out autoComplete);
                                    autoIndex = -1;
                                }
                            }

                            // Select autocomplete
                            if (onSearch != null && autoComplete != null && autoComplete.Length > 0)
                            {
                                autoIndex++;
                                if (autoIndex >= autoComplete.Length) autoIndex = 0;

                                string next = autoComplete[autoIndex];
                                for (int x = onSearch.Length, m = next.Length; x < m; x++)
                                    Console.Write(next[x]);

                                sInput = next;
                                //autoComplete = null;
                            }
                            break;
                        }
                    case ConsoleKey.Backspace:
                        {
                            if (sInput != "")
                            {
                                sInput = sInput.Remove(sInput.Length - 1);
                                Console.Write(myKey.KeyChar);
                                Console.Write(' ');
                                Console.Write(myKey.KeyChar);
                                onSearch = null;
                            }
                            break;
                        }
                    default:
                        {
                            Console.Write(myKey.KeyChar);
                            sInput = sInput + myKey.KeyChar;
                            onSearch = null;
                            break;
                        }
                }
                continue;
            }
        }
        public void SetForeColor(ConsoleColor color) { Console.ForegroundColor = color; }
        public void SetBackgroundColor(ConsoleColor color) { Console.BackgroundColor = color; }
    }
}