using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XPloit.Core.Command.Interfaces;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Command
{
    public class ConsoleCommand : ICommandLayer
    {
        Encoding _Codec;
        TextWriter _Out;
        TextReader _In;

        List<string> _Frames = new List<string>();

        public void GetCommand(string input, out string word, out string command, out string[] args)
        {
            if (input == null) input = "";
            input = input.TrimStart();

            int ix = input == null ? -1 : input.IndexOf(' ');
            if (ix != -1)
            {
                command = input.Substring(0, ix).Trim();

                args = ArgumentHelper.ArrayFromCommandLine(input.TrimStart().Substring(command.Length));
                // Añadimos un parametro extra para diferenciar que ya ha acabado de escribir el ultimo parametro
                if (input.EndsWith(" ")) Array.Resize(ref args, args.Length + 1);

                ix = input.LastIndexOf(' ');
                word = input.Substring(ix + 1, input.Length - ix - 1);
            }
            else
            {
                // Command
                word = input == null ? "" : input.TrimStart();
                command = "";
                args = null;
            }
        }

        ConsoleColor _LastFore, _LastBack;
        ConsoleColor _PromptColor = ConsoleColor.Green;
        ConsoleColor _InputColor = ConsoleColor.White;

        public string PromptCharacter { get; set; }

        public ConsoleCommand()
        {
            _Codec = Console.OutputEncoding;
            _Out = Console.Out;
            _In = Console.In;

            PromptCharacter = "> ";
        }
        public void Clear() { Console.Clear(); }
        /// <summary>
        /// Write a char
        /// </summary>
        /// <param name="c">Char</param>
        public void Write(char c) { Write(c.ToString()); }
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="line">Line</param>
        public void Write(string line)
        {
            if (line != null) _Out.Write(line);
        }
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="line">Line</param>
        public void WriteLine(string line)
        {
            if (line != null) _Out.Write(line);
            _Out.Write(Environment.NewLine);
        }
        public void SetBackgroundColor(ConsoleColor value)
        {
            if (_LastBack == value)
                return;
            _LastBack = value;
            Console.BackgroundColor = value;
        }
        public void SetForeColor(ConsoleColor value)
        {
            if (_LastFore == value)
                return;
            _LastFore = value;
            Console.ForegroundColor = value;
        }
        /// <summary>
        /// Adds a new input source on top of the input stack.
        ///
        /// This source will be used until it is exhausted, then the previous source will be used in the same manner.
        /// </summary>
        public void AddInput(IEnumerable<string> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            foreach (string s in source)
                _Frames.Add(s);
        }
        /// <summary>
        /// Puts a single line of input on top of the stack.
        /// </summary>
        public void AddInput(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            _Frames.Add(source);
        }
        void OnPrompt(ICommandLayer sender)
        {
            sender.SetForeColor(_PromptColor);
            sender.Write(PromptCharacter);
            sender.SetForeColor(_InputColor);
        }
        public string InternalReadLine() { return Console.ReadLine(); }
        public ConsoleKeyInfo ReadKey(bool intercept) { return Console.ReadKey(intercept); }
        public string ReadPassword(PromptDelegate prompt) { return ReadLine(prompt, null, true); }
        public string ReadLine(PromptDelegate prompt, IAutoCompleteSource autoComplete) { return ReadLine(prompt, autoComplete, false); }
        /// <summary>
        /// Returns the next available line of input.
        /// </summary>
        /// <param name="prompt">
        /// String to prompt, or null.
        /// </param>
        string ReadLine(PromptDelegate prompt, IAutoCompleteSource autoComplete, bool isPassword)
        {
            Console.CursorVisible = true;
            for (; ; )
            {
                string input;
                if (_Frames.Count >= 1)
                {
                    input = _Frames[0];
                    _Frames.RemoveAt(0);
                }
                else input = null;

                if (input == null)
                {
                    if (prompt != null) prompt(this);

                    if (autoComplete == null) input = InternalReadLine();
                    else
                    {
                        ConsoleKeyInfo myKey;
                        do
                        {
                            myKey = ReadKey(true);
                            switch (myKey.Key)
                            {
                                case ConsoleKey.Tab:
                                    {
                                        // Check in list
                                        string command;
                                        string word;
                                        string[] args;
                                        GetCommand(input, out word, out command, out args);

                                        IEnumerable<string> source;
                                        if (string.IsNullOrEmpty(command)) source = autoComplete.GetCommand();
                                        else source = autoComplete.GetArgument(command, args);

                                        if (source == null) break;

                                        List<string> ls = new List<string>();
                                        foreach (string s in source)
                                            if (word != s && s.StartsWith(word, autoComplete.ComparisonMethod))
                                            {
                                                if (!ls.Contains(s)) ls.Add(s);
                                            }

                                        // Ver que hacer según el numero de encuentros
                                        switch (ls.Count)
                                        {
                                            case 0:
                                                {
                                                    // Add space
                                                    Write(" ");
                                                    input = input + ' ';
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    // Add input
                                                    string toWrite = ls[0];
                                                    toWrite = toWrite.Substring(word.Length) + " ";
                                                    Write(toWrite);
                                                    input = input + toWrite;
                                                    break;
                                                }
                                            default:
                                                {
                                                    // Autocompletar el contenido conjunto
                                                    string fInput = word;
                                                    string l = ls[0];
                                                    while (l.Length > fInput.Length)
                                                    {
                                                        string lw = l.Substring(0, fInput.Length + 1);

                                                        bool enTodos = true;
                                                        foreach (string l2 in ls)
                                                        {
                                                            if (l2 == l) continue;
                                                            if (!l2.StartsWith(lw, autoComplete.ComparisonMethod)) { enTodos = false; break; }
                                                        }
                                                        if (!enTodos)
                                                            break;
                                                        fInput = lw;
                                                    }
                                                    if (fInput != word)
                                                    {
                                                        // Relleno
                                                        fInput = fInput.Remove(0, word.Length);
                                                        word += fInput;

                                                        input += fInput;
                                                        Write(fInput);
                                                    }

                                                    WriteLine("");

                                                    if (ls.Count > 50)
                                                    {
                                                        // Check show results
                                                        WriteLine("Show " + ls.Count.ToString() + " results? [Yes/No/Top]");
                                                        string s1 = InternalReadLine().ToUpperInvariant();

                                                        // Top signal?
                                                        if (s1 == "T" || s1 == "TOP")
                                                            ls.RemoveRange(50, ls.Count - 50);
                                                        else
                                                        {
                                                            // No signal?
                                                            if (s1 != "Y" && s1 != "YES")
                                                            {
                                                                if (prompt != null) prompt(this);
                                                                Write(input);
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    ls.Sort();

                                                    foreach (string s in ls)
                                                    {
                                                        SetBackgroundColor(ConsoleColor.Gray);
                                                        SetForeColor(ConsoleColor.Black);
                                                        Write(s.Substring(0, word.Length));
                                                        SetBackgroundColor(ConsoleColor.Black);
                                                        SetForeColor(ConsoleColor.Gray);
                                                        WriteLine(s.Substring(word.Length));
                                                    }

                                                    if (prompt != null) prompt(this);
                                                    Write(input);
                                                    break;
                                                }
                                        }
                                        break;
                                    }

                                #region NOT USED
                                case ConsoleKey.F1:
                                case ConsoleKey.F2:
                                case ConsoleKey.F3:
                                case ConsoleKey.F4:
                                case ConsoleKey.F5:
                                case ConsoleKey.F6:
                                case ConsoleKey.F7:
                                case ConsoleKey.F8:
                                case ConsoleKey.F9:
                                case ConsoleKey.F10:
                                case ConsoleKey.F11:
                                case ConsoleKey.F12:
                                case ConsoleKey.F13:
                                case ConsoleKey.F14:
                                case ConsoleKey.F15:
                                case ConsoleKey.F16:
                                case ConsoleKey.F17:
                                case ConsoleKey.F18:
                                case ConsoleKey.F19:
                                case ConsoleKey.F20:
                                case ConsoleKey.F21:
                                case ConsoleKey.F22:
                                case ConsoleKey.F23:
                                case ConsoleKey.F24:
                                case ConsoleKey.Applications:
                                case ConsoleKey.Attention:
                                case ConsoleKey.BrowserBack:
                                case ConsoleKey.BrowserFavorites:
                                case ConsoleKey.BrowserForward:
                                case ConsoleKey.BrowserHome:
                                case ConsoleKey.BrowserRefresh:
                                case ConsoleKey.BrowserSearch:
                                case ConsoleKey.BrowserStop:
                                case ConsoleKey.Clear:
                                case ConsoleKey.Help:
                                case ConsoleKey.Home:
                                case ConsoleKey.Insert:
                                case ConsoleKey.LaunchApp1:
                                case ConsoleKey.LaunchApp2:
                                case ConsoleKey.LaunchMail:
                                case ConsoleKey.LaunchMediaSelect:
                                case ConsoleKey.MediaNext:
                                case ConsoleKey.MediaPlay:
                                case ConsoleKey.MediaPrevious:
                                case ConsoleKey.MediaStop:
                                case ConsoleKey.NoName:
                                case ConsoleKey.Packet:
                                case ConsoleKey.Pause:
                                case ConsoleKey.Play:
                                case ConsoleKey.Print:
                                case ConsoleKey.PrintScreen:
                                case ConsoleKey.Process:
                                case ConsoleKey.RightWindows:
                                case ConsoleKey.Select:
                                case ConsoleKey.Sleep:
                                case ConsoleKey.VolumeDown:
                                case ConsoleKey.VolumeMute:
                                case ConsoleKey.VolumeUp:
                                case ConsoleKey.Zoom: break;
                                #endregion

                                case ConsoleKey.Enter:
                                    {
                                        WriteLine("");
                                        break;
                                    }
                                case ConsoleKey.Backspace:
                                    {
                                        if (input.Length > 0)
                                        {
                                            input = input.Substring(0, input.Length - 1);
                                            Write("\b \b");
                                        }
                                        break;
                                    }

                                #region ToDO
                                case ConsoleKey.LeftArrow: { break; }
                                case ConsoleKey.RightArrow: { break; }

                                case ConsoleKey.PageUp:
                                case ConsoleKey.UpArrow: { break; }

                                case ConsoleKey.DownArrow:
                                case ConsoleKey.PageDown: { break; }
                                #endregion

                                default:
                                    {
                                        if (myKey.KeyChar == '\0') break;

                                        Write(myKey.KeyChar.ToString());
                                        input = input + myKey.KeyChar;
                                        break;
                                    }
                            }

                            continue;
                        }
                        while (myKey.Key != ConsoleKey.Enter);
                    }

                    SetForeColor(ConsoleColor.Gray);
                }

                SetForeColor(ConsoleColor.Gray);
                Console.CursorVisible = false;
                if (!string.IsNullOrWhiteSpace(input)) return input;
            }
        }
    }
}