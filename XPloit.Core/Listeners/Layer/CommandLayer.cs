using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using XPloit.Core.Command.Interfaces;
using XPloit.Core.Helpers;
using XPloit.Core.Interfaces;
using XPloit.Core.Listeners.IO;
using XPloit.Res;

namespace XPloit.Core.Listeners.Layer
{
    public class CommandLayer : IDisposable
    {
        StreamWriter _Log;
        IIOCommandLayer _IO;

        const byte MaxHistoryLength = 10;

        byte _HistoryIndex = 0;
        List<string> _History = new List<string>();
        List<string> _ManualInput = new List<string>();

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

        ConsoleCursor _Position = null;
        bool _AllowOutPut = true, _InsertMode = true;
        Thread _CancelableThread = null;
        ConsoleColor _LastFore, _LastBack;
        ConsoleColor _PromptColor = ConsoleColor.Green;
        ConsoleColor _InputColor = ConsoleColor.White;

        int _LastPercent = -1;
        double _ProgressVal = 0, _ProgressMax = 0;
        bool _ReSendProgress = false;

        /// <summary>
        /// Set Allow output
        /// </summary>
        public bool AllowOutPut
        {
            get { return _AllowOutPut; }
            private set { _AllowOutPut = value; }
        }
        /// <summary>
        /// Return Insert Mode
        /// </summary>
        public bool InsertMode { get { return _InsertMode; } }
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
            if (!_AllowOutPut) return;

            if (_ReSendProgress)
            {
                _ReSendProgress = false;
                _LastPercent = -1;
                WriteStart("%", ConsoleColor.Yellow);
                _Position = _IO.GetCursorPosition();
            }

            _ProgressVal = value;

            if (value > _ProgressMax) value = _ProgressMax;
            double percent = _ProgressMax == 0 ? 0 : (value * 100.0) / _ProgressMax;

            int lp = (int)(percent * 10);
            if (lp == _LastPercent)
                return;
            _LastPercent = lp;

            _Position.Flush(_IO);
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
            if (!_AllowOutPut) return;

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
            //else error = error.Trim();

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
            //else info = info.Trim();

            WriteStart("*", ConsoleColor.Cyan);
            Write(info);

            if (!string.IsNullOrEmpty(colorText))
            {
                Write((info.EndsWith(".") ? "." : " ") + "... [");
                SetForeColor(color);
                Write(colorText);
                SetForeColor(ConsoleColor.Gray);
                WriteLine("]");
            }
        }
        public void Clear()
        {
            if (!_AllowOutPut) return;
            _IO.Clear();
        }
        /// <summary>
        /// Write a char
        /// </summary>
        /// <param name="c">Char</param>
        public void Write(char c)
        {
            if (!_AllowOutPut) return;
            _IO.Write(c.ToString());
        }
        /// <summary>
        /// Beep
        /// </summary>
        public void Beep()
        {
            if (!_AllowOutPut) return;
            _IO.Beep();
        }
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="line">Line</param>
        public void Write(string line)
        {
            if (!_AllowOutPut) return;
            if (line != null) _IO.Write(line);
        }
        /// <summary>
        /// Write
        /// </summary>
        /// <param name="line">Line</param>
        public void WriteLine(string line)
        {
            if (!_AllowOutPut) return;
            if (line == null) line = "";
            _IO.Write(line + Environment.NewLine);
        }
        public void SetBackgroundColor(ConsoleColor value)
        {
            if (!_AllowOutPut) return;
            if (_LastBack == value) return;

            _LastBack = value;
            _IO.SetBackgroundColor(value);
        }
        public void SetForeColor(ConsoleColor value)
        {
            if (!_AllowOutPut) return;
            if (_LastFore == value) return;

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
                AddInput(s);
        }
        /// <summary>
        /// Puts a single line of input on top of the stack.
        /// </summary>
        public void AddInput(string source)
        {
            if (!string.IsNullOrEmpty(source)) _ManualInput.Add(source);
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
            int index = 0;
            _IO.SetCursorMode(_InsertMode ? ConsoleCursor.ECursorMode.Visible : ConsoleCursor.ECursorMode.Small);
            for (;;)
            {
                string input;
                if (_ManualInput.Count >= 1)
                {
                    input = _ManualInput[0];
                    _ManualInput.RemoveAt(0);
                }
                else input = null;

                if (input == null)
                {
                    if (prompt != null) prompt(this);

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

                                    ConsoleCursor point = _IO.GetCursorPosition();

                                    // Ver que hacer según el numero de encuentros
                                    string toWrite = "";
                                    switch (ls.Count)
                                    {
                                        // Add space
                                        case 0: { toWrite = " "; break; }
                                        case 1:
                                            {
                                                // Add input
                                                toWrite = ls[0];
                                                toWrite = toWrite.Substring(word.Length) + " ";
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
                                                    toWrite = fInput;
                                                }
                                                break;
                                            }
                                    }

                                    // Go to end of line
                                    int mas = input.Length - index;
                                    input = input + toWrite;
                                    index = input.Length;

                                    point.MoveRight(mas);
                                    point.Flush(_IO);

                                    if (ls.Count > 1)
                                    {
                                        // Auto complete
                                        WriteLine("");
                                        if (ls.Count > 50)
                                        {
                                            // Check show results
                                            WriteLine(Lang.Get("Show_All_Results", ls.Count.ToString()));
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
                                    }
                                    else
                                    {
                                        Write(toWrite);
                                    }
                                    break;
                                }

                            #region NOT USED
                            case ConsoleKey.Clear:
                            case ConsoleKey.Help:
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

                            case ConsoleKey.Enter: { WriteLine(""); break; }
                            case ConsoleKey.Insert:
                                {
                                    // Tongle insert mode
                                    _InsertMode = !_InsertMode;
                                    _IO.SetCursorMode(_InsertMode ? ConsoleCursor.ECursorMode.Visible : ConsoleCursor.ECursorMode.Small);
                                    break;
                                }
                            case ConsoleKey.Home:
                                {
                                    if (index <= 0) break;

                                    ConsoleCursor point = _IO.GetCursorPosition();
                                    point.MoveLeft(index);
                                    point.Flush(_IO);

                                    index = 0;
                                    break;
                                }
                            case ConsoleKey.End:
                                {
                                    if (input == null || index >= input.Length) break;

                                    ConsoleCursor point = _IO.GetCursorPosition();
                                    point.MoveRight(input.Length - index);
                                    point.Flush(_IO);

                                    index = input.Length;
                                    break;
                                }
                            case ConsoleKey.Delete:
                                {
                                    if (input == null || index >= input.Length) break;

                                    if (index + 1 == input.Length)
                                    {
                                        input = input.Substring(0, input.Length - 1);

                                        Write(" ");

                                        ConsoleCursor point = _IO.GetCursorPosition();
                                        point.MoveLeft(1);
                                        point.Flush(_IO);
                                    }
                                    else
                                    {
                                        input = input.Remove(index, 1);

                                        ConsoleCursor point = _IO.GetCursorPosition();
                                        Write(input.Substring(index) + " ");
                                        point.Flush(_IO);
                                    }
                                    break;
                                }
                            case ConsoleKey.Backspace:
                                {
                                    if (input == null || index <= 0) break;

                                    ConsoleCursor point = _IO.GetCursorPosition();

                                    if (index == input.Length)
                                    {
                                        input = input.Substring(0, input.Length - 1);

                                        point.MoveLeft(1);
                                        point.Flush(_IO);
                                        Write(" ");
                                        point.Flush(_IO);
                                    }
                                    else
                                    {
                                        input = input.Remove(index - 1, 1);

                                        point.MoveLeft(1);
                                        Write("".PadLeft(input.Length - index + 2));
                                        point.Flush(_IO);
                                        Write(input.Substring(index - 1));
                                        point.Flush(_IO);
                                    }

                                    index--;
                                    break;
                                }
                            case ConsoleKey.LeftArrow:
                                {
                                    if (input == null || index <= 0) break;

                                    index--;
                                    ConsoleCursor point = _IO.GetCursorPosition();
                                    point.MoveLeft(1);
                                    point.Flush(_IO);
                                    break;
                                }
                            case ConsoleKey.RightArrow:
                                {
                                    if (input == null || index >= input.Length) break;

                                    index++;
                                    ConsoleCursor point = _IO.GetCursorPosition();
                                    point.MoveRight(1);
                                    point.Flush(_IO);
                                    break;
                                }

                            case ConsoleKey.PageUp:
                            case ConsoleKey.UpArrow:
                            case ConsoleKey.DownArrow:
                            case ConsoleKey.PageDown:
                                {
                                    if (_History.Count <= 0) break;

                                    string next = GetHistory(myKey.Key == ConsoleKey.PageUp || myKey.Key == ConsoleKey.UpArrow);
                                    ConsoleCursor point = _IO.GetCursorPosition();

                                    if (index > 0)
                                    {
                                        point.MoveLeft(index);
                                        point.Flush(_IO);
                                    }

                                    Write(next);

                                    if (next.Length < index)
                                    {
                                        // Undo write move
                                        point.MoveRight(next.Length);

                                        int append = index - next.Length;
                                        Write("".PadLeft(append, ' '));

                                        // Restore save point
                                        point.Flush(_IO);
                                    }

                                    input = next;
                                    index = next.Length;
                                    break;
                                }
                            default:
                                {
                                    if (myKey.KeyChar == '\0') break;

                                    if (input == null) input = "";

                                    if (input.Length == index)
                                    {
                                        input = input + myKey.KeyChar;
                                        Write(isPassword ? "*" : myKey.KeyChar.ToString());
                                    }
                                    else
                                    {
                                        ConsoleCursor point = _IO.GetCursorPosition();

                                        if (_InsertMode)
                                        {
                                            int antes = input.Length;
                                            input = input.Insert(index, myKey.KeyChar.ToString());

                                            Write(isPassword ? "".PadLeft(antes, '*') : input.Substring(index));

                                            point.Flush(_IO);
                                            point.MoveRight(1);
                                            point.Flush(_IO);
                                        }
                                        else
                                        {
                                            char[] array = input.ToCharArray();
                                            array[index] = myKey.KeyChar;
                                            input = new string(array);

                                            Write(isPassword ? "*" : myKey.KeyChar.ToString());
                                        }
                                    }

                                    index++;
                                    break;
                                }
                        }

                        continue;
                    }
                    while (myKey.Key != ConsoleKey.Enter);

                    SetForeColor(ConsoleColor.Gray);
                }

                SetForeColor(ConsoleColor.Gray);
                _IO.SetCursorMode(ConsoleCursor.ECursorMode.Hidden);

                if (!string.IsNullOrWhiteSpace(input))
                {
                    WriteLog(input);
                    return input;
                }
            }
        }
        /// <summary>
        /// Get next or previous history command
        /// </summary>
        /// <param name="next">True for next, False for previous</param>
        string GetHistory(bool next)
        {
            string cad;

            if (next)
            {
                cad = _History[_HistoryIndex];

                if (_HistoryIndex == 0) _HistoryIndex = (byte)(_History.Count - 1);
                else _HistoryIndex--;
            }
            else
            {
                if (_HistoryIndex < _History.Count - 1) _HistoryIndex++;
                else _HistoryIndex = 0;

                cad = _History[_HistoryIndex];
            }

            return cad;
        }
        /// <summary>
        /// Write log for the user input
        /// </summary>
        /// <param name="input">Input</param>
        void WriteLog(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            // Save to file
            if (_Log != null)
            {
                if (input.ToLowerInvariant().Trim() == "record stop") return;

                _Log.WriteLine(input);
                _Log.Flush();
            }

            // Append to history
            if (_History.Count >= MaxHistoryLength) _History.RemoveAt(0);
            _History.Add(input);
            _HistoryIndex = (byte)(_History.Count - 1);
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