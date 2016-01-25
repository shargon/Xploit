using System;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Command
{
    public class ConsoleIO : IIOCommandLayer
    {
        public Action<object, ConsoleCancelEventArgs> CancelKeyPress { get; set; }

        public ConsoleIO() { Console.CancelKeyPress += Console_CancelKeyPress; }

        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (CancelKeyPress != null)
                CancelKeyPress(sender, e);
        }

        public void Beep() { Console.Beep(); }
        public void Clear() { Console.Clear(); }
        public ConsoleKeyInfo ReadKey(bool intercept) { return Console.ReadKey(intercept); }
        public string ReadLine() { return Console.ReadLine(); }
        public void SetBackgroundColor(ConsoleColor value) { Console.BackgroundColor = value; }
        public void SetCursorPosition(int x, int y)
        {
            Console.CursorLeft = x;
            Console.CursorTop = y;
        }
        public void GetCursorPosition(ref int x, ref int y)
        {
            x = Console.CursorLeft;
            y = Console.CursorTop;
        }
        public void Write(string input) { Console.Write(input); }
        public void SetForeColor(ConsoleColor value) { Console.ForegroundColor = value; }
        public void SetCursorVisible(bool visible) { Console.CursorVisible = visible; }
    }
}