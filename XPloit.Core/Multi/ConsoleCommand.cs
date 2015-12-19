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
        /// <param name="appendNewLine">True for append newLine</param>
        public void Write(string line, bool appendNewLine)
        {
            if (line != null) _Out.Write(line);
            if (appendNewLine) _Out.Write(Environment.NewLine);
        }
        public string ReadLine() { return _In.ReadLine(); }
        public void SetForeColor(ConsoleColor color) { Console.ForegroundColor = color; }
        public void SetBackgroundColor(ConsoleColor color) { Console.BackgroundColor = color; }
    }
}