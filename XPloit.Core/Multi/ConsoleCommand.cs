using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
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

        class Frame
        {
            public IEnumerator<string> E;
            public Frame(IEnumerable<string> source) { E = source.GetEnumerator(); }
        }

        readonly Stack<Frame> _Frames = new Stack<Frame>();

        ConsoleColor _LastFore, _LastBack;
        ConsoleColor _PromptColor = ConsoleColor.Green;
        ConsoleColor _InputColor = ConsoleColor.White;

        public string PromptCharacter { get; set; }

        /// <summary>
        /// Use autocomplete
        /// </summary>
        public bool AllowAutocomplete { get; set; }

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
            if (_LastBack == value) return;
            _LastBack = value;
            Console.BackgroundColor = value;
        }
        public void SetForeColor(ConsoleColor value)
        {
            if (_LastFore == value) return;
            _LastFore = value;
            Console.ForegroundColor = value;
        }

        string GetNextFrameInput()
        {
            while (_Frames.Any())
            {
                Frame f = _Frames.Peek();
                if (!f.E.MoveNext())
                {
                    _Frames.Pop();
                    continue;
                }
                return f.E.Current;
            }
            return null;
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

            _Frames.Push(new Frame(source));
        }
        /// <summary>
        /// Puts a single line of input on top of the stack.
        /// </summary>
        public void AddInput(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            AddInput(new string[] { source });
        }

        void OnPrompt(ICommandLayer sender)
        {
            sender.SetForeColor(_PromptColor);
            sender.Write(PromptCharacter);
            sender.SetForeColor(_InputColor);
        }

        public string ReadLine(PromptDelegate prompt) { return ReadLine(prompt, false); }
        public string ReadLine(PromptDelegate prompt, bool isPassword)
        {
            Console.CursorVisible = true;

            string input = GetNextFrameInput();

            if (string.IsNullOrEmpty(input))
            {
                if (prompt != null) prompt(this);

                if (!AllowAutocomplete)
                {
                    input = Console.ReadLine();
                }
                else
                {
                    //string write = "";
                    //CommandTabState state = null;

                    //ConsoleKeyInfo cki;

                    //do
                    //{
                    //    cki = Console.ReadKey(true);
                    //    char l = cki.KeyChar;

                    //    switch (cki.Key)
                    //    {
                    //        case ConsoleKey.Tab:
                    //            {
                    //                string get = null;

                    //                if (!isPassword)
                    //                {
                    //                    if (_AutoCompleteSource != null)
                    //                    {
                    //                        bool erase = true;
                    //                        if (state == null)
                    //                        {
                    //                            state = new CommandTabState(write, _AutoCompleteSource);
                    //                            erase = false;
                    //                        }

                    //                        int llast = state.Last == null ? 0 : state.Last.Length;

                    //                        get = state.Next();

                    //                        if (get != null)
                    //                        {
                    //                            int lget = get.Length;
                    //                            int lcmd = state.Cmd.Length;

                    //                            if (lget < lcmd)
                    //                            {
                    //                                Delete(ref write, lcmd);

                    //                                Write(get);
                    //                                write += get;
                    //                            }
                    //                            else
                    //                            {
                    //                                if (erase) Delete(ref write, llast - lcmd);

                    //                                // Print rest
                    //                                Write(get.Substring(lcmd));
                    //                                write += get.Substring(lcmd);

                    //                                if (state.MultipleChoise) break;
                    //                            }
                    //                        }

                    //                        if (state.Posibilities != null && state.StartWithQuotes && !write.EndsWith("\""))
                    //                        {
                    //                            l = '"';
                    //                            write += l.ToString();

                    //                            if (isPassword && l != ' ') l = '*';
                    //                            Write(l);
                    //                        }
                    //                    }

                    //                    l = ' ';
                    //                    goto default;
                    //                }

                    //                break;
                    //            }
                    //        case ConsoleKey.Backspace:
                    //            {
                    //                state = null;
                    //                Delete(ref write, 1);
                    //                break;
                    //            }
                    //        case ConsoleKey.Enter:
                    //            {
                    //                state = null;
                    //                Write(Environment.NewLine);
                    //                break;
                    //            }
                    //        default:
                    //            {
                    //                if (l == '\0') continue;

                    //                state = null;
                    //                write += l.ToString();

                    //                if (isPassword && l != ' ') l = '*';
                    //                Write(l);
                    //                break;
                    //            }

                    //    }

                    //} while (cki.Key != ConsoleKey.Enter);

                }
                SetForeColor(ConsoleColor.Gray);
            }

            Console.CursorVisible = false;
            return input;
        }
    }
}