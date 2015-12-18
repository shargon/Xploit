using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using XPloit.Core.Interfaces;
using XPloit.Core.Multi;
using XPloit.Res;

namespace XPloit.Core.Listeners
{
    public class StreamListener : IListener
    {
        const string CommandStart = " > ";

        bool _IsStarted;
        Encoding _Codec;
        TextWriter _Out;
        TextReader _In;
        Color2Stream _ColorConverter;

        public delegate string Color2Stream(ConsoleColor color, bool isForeGround);

        /// <summary>
        /// Constructor
        /// </summary>
        public StreamListener(Encoding codec, TextWriter streamOut, TextReader streamIn, Color2Stream colorConverter)
        {
            _Out = streamOut;
            _In = streamIn;
            _Codec = codec;
            _ColorConverter = colorConverter;
        }
        public override bool IsStarted { get { return _IsStarted; } }

        public override bool Start()
        {
            _IsStarted = true;

            SetForeColor(ConsoleColor.Green);
            Write(Lang.Get("Wellcome"), true);

            //Menu[] menus = new Menu[]
            //{
            //};

            string read;
            do
            {
                CultureInfo c = CultureInfo.CurrentCulture;
                SetForeColor(ConsoleColor.White);
                Write(CommandStart, false);
                SetForeColor(ConsoleColor.White);
                read = ReadLine();
                if (read == null) continue;

                if (read.StartsWith("cd ", StringComparison.InvariantCultureIgnoreCase))
                {
                    read = read.Trim();

                    if (string.Compare("cd ..", read, true) == 0)
                    {
                        // Down one level
                        continue;
                    }
                    else if (string.Compare("cd /", read, true) == 0 || string.Compare("CD \\", read, true) == 0)
                    {
                        // Down to main
                        continue;
                    }
                }

                if (string.Compare("exit", read, true) != 0)
                {
                    SetForeColor(ConsoleColor.Red);
                    Write(Lang.Get("Unknown_Command"), true);
                }
            }
            while (string.Compare("exit", read, true) != 0);

            return _IsStarted;
        }
        /// <summary>
        /// Write into stream
        /// </summary>
        /// <param name="line">Line</param>
        /// <param name="appendNewLine">True for append newLine</param>
        public void Write(string line, bool appendNewLine)
        {
            if (line != null) _Out.Write(line);
            if (appendNewLine) _Out.Write(Environment.NewLine);
        }
        /// <summary>
        /// Read line
        /// </summary>
        public string ReadLine() { return _In.ReadLine(); }
        public bool SetForeColor(ConsoleColor color)
        {
            if (_ColorConverter != null)
            {
                Write(_ColorConverter(color, true), false);
                return true;
            }
            return false;
        }

        public bool SetBackgroundColor(ConsoleColor color)
        {
            if (_ColorConverter != null)
            {
                Write(_ColorConverter(color, false), false);
                return true;
            }
            return false;
        }

        public override bool Stop()
        {
            _IsStarted = false;
            return true;
        }
    }
}