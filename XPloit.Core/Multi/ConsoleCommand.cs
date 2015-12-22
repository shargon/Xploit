using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using XPloit.Core.Enums;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Multi
{
    public class ConsoleCommand : ICommandLayer
    {
        Encoding _Codec;
        TextWriter _Out;
        TextReader _In;

        IGetAutocompleteCommand _AutoCompleteSource = null;
        /// <summary>
        /// AutoComplete Source
        /// </summary>
        public IGetAutocompleteCommand AutoCompleteSource { get { return _AutoCompleteSource; } set { _AutoCompleteSource = value; } }

        class CommandTabState
        {
            public string[] Posibilities = null;

            public string Last;
            public string Cmd;
            public string Input;
            public int Index = -1;
            public bool MultipleChoise = false;
            public bool StartWithQuotes = false;

            static string[] GetCommand(ref string cmd, IGetAutocompleteCommand options)
            {
                int ix = cmd.LastIndexOf(' ');
                if (ix != -1)
                {
                    int ix2 = cmd.IndexOf(' ');
                    if (ix2 != -1)
                    {
                        string exe = cmd.Substring(0, ix2);

                        cmd = cmd.Substring(ix + 1);
                        return options.AvailableCommandOptions(exe.Trim('"'));
                    }
                }
                return options.AvailableCommands();
            }
            public CommandTabState(string write, IGetAutocompleteCommand commander)
            {
                Input = write;
                Cmd = write.ToLowerInvariant();
                Posibilities = GetCommand(ref Cmd, commander);

                if (Cmd.StartsWith("\""))
                {
                    Cmd = Cmd.Substring(1);
                    StartWithQuotes = true;
                }

                // Manual
                List<string> av = new List<string>();
                foreach (string a in Posibilities)
                {
                    if (a.StartsWith(Cmd, StringComparison.InvariantCultureIgnoreCase))
                        av.Add(a);
                }

                MultipleChoise = av.Count > 1;

                // Files
                if (!string.IsNullOrEmpty(Cmd))
                {
                    try
                    {
                        string path = Path.GetDirectoryName(Cmd);
                        if (!Directory.Exists(path) && Directory.Exists(Cmd)) path = Cmd;

                        if (Directory.Exists(path))
                        {
                            bool hasDir = false;
                            int hasFiles = 0;
                            string file = Path.GetFileName(Cmd);

                            if (commander.AllowAutocompleteFolders == EAllowAutocompleteCommand.Yes || (commander.AllowAutocompleteFolders == EAllowAutocompleteCommand.OnlyWhenEmpty && av.Count <= 0))
                                foreach (string di in Directory.GetDirectories(path, file + "*"))
                                {
                                    hasDir = true;
                                    av.Add(di + Path.DirectorySeparatorChar);
                                }

                            if (commander.AllowAutocompleteFiles == EAllowAutocompleteCommand.Yes || (commander.AllowAutocompleteFiles == EAllowAutocompleteCommand.OnlyWhenEmpty && av.Count <= 0))
                                foreach (string di in Directory.GetFiles(path, file + "*"))
                                {
                                    hasFiles++;
                                    av.Add(di);
                                }

                            if (hasDir || hasFiles > 1)
                            {
                                // When is directory dont complete
                                MultipleChoise = true;
                            }
                        }
                    }
                    catch { }

                    if (commander.AllowAutocompleteMaths == EAllowAutocompleteCommand.Yes || (commander.AllowAutocompleteMaths == EAllowAutocompleteCommand.OnlyWhenEmpty && av.Count <= 0))
                    {
                        // Trick for calc from string
                        double c = MathHelper.Calc(Cmd);
                        if (!double.IsNaN(c)) av.Add(c.ToString(CultureInfo.InvariantCulture));
                    }
                }

                av.Sort();
                Posibilities = av.Count == 0 ? null : av.ToArray();

                Last = null;
            }
            public string Next()
            {
                if (Posibilities == null)
                {
                    Last = null;
                    return null;
                }

                Index++;
                if (Posibilities.Length <= Index) Index = 0;
                return Last = Posibilities[Index];
            }
        }
        void Delete(ref string write, int count)
        {
            if (write.Length > 0 && count > 0)
            {
                write = write.Substring(0, write.Length - count);

                for (int x = 0; x < count; x++)
                    Write("\b \b");
            }
        }


        public ConsoleCommand()
        {
            _Codec = Console.OutputEncoding;
            _Out = Console.Out;
            _In = Console.In;
        }

        public void Clear() { Console.Clear(); }
        /// <summary>
        /// Write a char
        /// </summary>
        /// <param name="c">Char</param>
        public void Write(char c) { Write(c.ToString(), false); }
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
        public void SetForeColor(ConsoleColor color) { Console.ForegroundColor = color; }
        public void SetBackgroundColor(ConsoleColor color) { Console.BackgroundColor = color; }


        public string ReadLine() { return ReadLine(false); }
        public string ReadLine(bool isPassword)
        {
            Console.CursorVisible = true;

            string write = "";
            CommandTabState state = null;

            ConsoleKeyInfo cki;

            do
            {
                cki = Console.ReadKey(true);
                char l = cki.KeyChar;

                switch (cki.Key)
                {
                    case ConsoleKey.Tab:
                        {
                            string get = null;

                            if (!isPassword)
                            {
                                if (_AutoCompleteSource != null)
                                {
                                    bool erase = true;
                                    if (state == null)
                                    {
                                        state = new CommandTabState(write, _AutoCompleteSource);
                                        erase = false;
                                    }

                                    int llast = state.Last == null ? 0 : state.Last.Length;

                                    get = state.Next();

                                    if (get != null)
                                    {
                                        int lget = get.Length;
                                        int lcmd = state.Cmd.Length;

                                        if (lget < lcmd)
                                        {
                                            Delete(ref write, lcmd);

                                            Write(get);
                                            write += get;
                                        }
                                        else
                                        {
                                            if (erase) Delete(ref write, llast - lcmd);

                                            // Print rest
                                            Write(get.Substring(lcmd));
                                            write += get.Substring(lcmd);

                                            if (state.MultipleChoise) break;
                                        }
                                    }

                                    if (state.Posibilities != null && state.StartWithQuotes && !write.EndsWith("\""))
                                    {
                                        l = '"';
                                        write += l.ToString();

                                        if (isPassword && l != ' ') l = '*';
                                        Write(l);
                                    }
                                }

                                l = ' ';
                                goto default;
                            }

                            break;
                        }
                    case ConsoleKey.Backspace:
                        {
                            state = null;
                            Delete(ref write, 1);
                            break;
                        }
                    case ConsoleKey.Enter:
                        {
                            state = null;
                            Write(Environment.NewLine);
                            break;
                        }
                    default:
                        {
                            if (l == '\0') continue;

                            state = null;
                            write += l.ToString();

                            if (isPassword && l != ' ') l = '*';
                            Write(l);
                            break;
                        }

                }

            } while (cki.Key != ConsoleKey.Enter);


            Console.CursorVisible = false;
            return write;
        }
    }
}