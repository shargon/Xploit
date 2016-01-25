using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using XPloit.Core.Command.Interfaces;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Command
{
    public class CommandLayer : IDisposable
    {
        StreamWriter _Log;
        IIOCommandLayer _IO;

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

        int _ConsoleX = -1, _ConsoleY = -1;
        Thread _CancelableThread = null;
        ConsoleColor _LastFore, _LastBack;
        ConsoleColor _PromptColor = ConsoleColor.Green;
        ConsoleColor _InputColor = ConsoleColor.White;

        int _LastPercent = -1;
        double _ProgressVal = 0, _ProgressMax = 0;
        bool _ReSendProgress = false;

        /// <summary>
        /// Propmt char
        /// </summary>
        public string PromptCharacter { get; set; }
        /// <summary>
        /// Return true if record is active
        /// </summary>
        public bool IsRecording { get { return _Log != null; } }
        /// <summary>
        /// Get or set the current thread
        /// </summary>
        public Thread CancelableThread
        {
            get { return _CancelableThread; }
            set { _CancelableThread = value; }
        }
        /// <summary>
        /// Returns true if are in progress
        /// </summary>
        public bool IsInProgress { get { return _ProgressMax > 0; } }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="io">Input/Output</param>
        public CommandLayer(IIOCommandLayer io)
        {
            _IO = io;

            io.CancelKeyPress += Console_CancelKeyPress;

            PromptCharacter = "> ";
        }

        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (_CancelableThread != null)
            {
                e.Cancel = true;
                Thread th = _CancelableThread;
                _CancelableThread = null;
                th.Abort();
            }
        }
        public void WriteProgress(double value)
        {
            if (!IsInProgress) return;

            if (_ReSendProgress)
            {
                _ReSendProgress = false;
                _LastPercent = -1;
                WriteStart("%", ConsoleColor.Yellow);
                _IO.GetCursorPosition(ref _ConsoleX, ref _ConsoleY);
            }

            _ProgressVal = value;

            if (value > _ProgressMax) value = _ProgressMax;
            double percent = _ProgressMax == 0 ? 0 : (value * 100.0) / _ProgressMax;

            int lp = (int)(percent * 10);
            if (lp == _LastPercent)
                return;
            _LastPercent = lp;

            _IO.SetCursorPosition(_ConsoleX, _ConsoleY);
            int ip = (int)percent / 10;

            ConsoleColor last = _LastFore;
            SetForeColor(last);
            Write("[");

            if (ip > 0)
            {
                if (ip >= 8) SetForeColor(ConsoleColor.Green);
                else if (ip >= 6) SetForeColor(ConsoleColor.DarkGreen);
                else if (ip >= 4) SetForeColor(ConsoleColor.DarkYellow);
                else SetForeColor(ConsoleColor.Red);

                Write("#".PadLeft(ip, '#'));
            }
            if (ip < 10)
            {
                Write(" ".PadLeft(10 - ip, ' '));
            }

            SetForeColor(last);
            Write("] " + percent.ToString("0.0 '%'"));
            SetForeColor(last);
        }
        public void EndProgress()
        {
            if (!IsInProgress) return;

            WriteProgress(_ProgressMax);

            _ReSendProgress = false;
            _LastPercent = -1;
            _ProgressMax = -1;

            WriteLine("");
        }
        public void StartProgress(double max)
        {
            _ReSendProgress = true;
            _ProgressMax = max;

            WriteProgress(0);
        }
        void WriteStart(string ch, ConsoleColor color)
        {
            if (ch != "%" && IsInProgress)
            {
                _ReSendProgress = true;
                WriteLine("");
            }

            SetForeColor(ConsoleColor.Gray);
            Write("[");
            SetForeColor(color);
            Write(ch);
            SetForeColor(ConsoleColor.Gray);
            Write("] ");
        }
        public void WriteError(string error)
        {
            if (string.IsNullOrEmpty(error)) error = "";
            else error = error.Trim();

            WriteStart("!", ConsoleColor.Red);
            SetForeColor(ConsoleColor.Red);
            WriteLine(error.Replace("\n", "\n    "));
        }
        public void WriteInfo(string info)
        {
            if (string.IsNullOrEmpty(info)) info = "";
            else info = info.Trim();

            WriteStart("*", ConsoleColor.Cyan);
            WriteLine(info.Replace("\n", "\n    "));
        }
        public void WriteInfo(string info, string colorText, ConsoleColor color)
        {
            if (string.IsNullOrEmpty(info)) info = "";
            else info = info.Trim();

            WriteStart("*", ConsoleColor.Cyan);
            Write(info);

            if (!string.IsNullOrEmpty(colorText))
            {
                Write(" ... [");
                SetForeColor(color);
                Write(colorText);
                SetForeColor(ConsoleColor.Gray);
                WriteLine("]");
            }
        }
        public void Clear() { _IO.Clear(); }
        /// <summary>
        /// Write a char
        /// </summary>
        /// <param name="c">Char</param>
        public void Write(char c) { _IO.Write(c.ToString()); }
        /// <summary>
        /// Beep
        /// </summary>
        public void Beep() { _IO.Beep(); }
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="line">Line</param>
        public void Write(string line) { if (line != null) _IO.Write(line); }
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="line">Line</param>
        public void WriteLine(string line)
        {
            if (line == null) line = "";
            _IO.Write(line + Environment.NewLine);
        }
        public void SetBackgroundColor(ConsoleColor value)
        {
            if (_LastBack == value)
                return;

            _LastBack = value;
            _IO.SetBackgroundColor(value);
        }
        public void SetForeColor(ConsoleColor value)
        {
            if (_LastFore == value)
                return;

            _LastFore = value;
            _IO.SetForeColor(value);
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
            if (source != null)
                _Frames.Add(source);
        }
        void OnPrompt(CommandLayer sender)
        {
            sender.SetForeColor(_PromptColor);
            sender.Write(PromptCharacter);
            sender.SetForeColor(_InputColor);
        }
        public string InternalReadLine()
        {
            string input = _IO.ReadLine();

            WriteLog(input);
            return input;
        }
        public ConsoleKeyInfo ReadKey(bool intercept) { return _IO.ReadKey(intercept); }
        public string ReadPassword(PromptDelegate prompt) { return ReadLine(prompt, null, true); }
        public string ReadLine(PromptDelegate prompt, IAutoCompleteSource autoComplete) { return ReadLine(prompt, autoComplete, false); }
        /// <summary>
        /// Returns the next available line of input.
        /// </summary>
        /// <param name="prompt">Prompt</param>
        /// <param name="autoComplete">AutoComplete source</param>
        /// <param name="isPassword">True for hide the input</param>
        string ReadLine(PromptDelegate prompt, IAutoCompleteSource autoComplete, bool isPassword)
        {
            _IO.SetCursorVisible(true);
            for (;;)
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

                    if (autoComplete == null && !isPassword) input = InternalReadLine();
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
                                        if (autoComplete == null || isPassword) break;

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

                                        input = input + myKey.KeyChar;
                                        Write(isPassword ? "*" : myKey.KeyChar.ToString());
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
                _IO.SetCursorVisible(false);

                if (!string.IsNullOrWhiteSpace(input))
                {
                    WriteLog(input);
                    return input;
                }
            }
        }

        void WriteLog(string input)
        {
            if (_Log == null || string.IsNullOrEmpty(input)) return;
            if (input.ToLowerInvariant().Trim() == "record stop") return;

            _Log.WriteLine(input);
            _Log.Flush();
        }

        /// <summary>
        /// Start record
        /// </summary>
        /// <param name="file">File</param>
        public void RecordStart(string file)
        {
            RecordStop();
            _Log = new StreamWriter(file, true, Encoding.UTF8);
        }
        /// <summary>
        /// Stop record
        /// </summary>
        public void RecordStop()
        {
            if (_Log != null)
            {
                _Log.Dispose();
                _Log = null;
            }
        }
        /// <summary>
        /// Free Resources
        /// </summary>
        public void Dispose() { RecordStop(); }
    }
}