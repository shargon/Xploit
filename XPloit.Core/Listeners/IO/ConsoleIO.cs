using System;
using System.Text;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Listeners.IO
{
    public class ConsoleIO : IIOCommandLayer
    {
        public Action<object, ConsoleCancelEventArgs> CancelKeyPress { get; set; }

        public ConsoleIO()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Console.CancelKeyPress += Console_CancelKeyPress;
        }

        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            CancelKeyPress?.Invoke(sender, e);
        }

        public void Beep() { Console.Beep(); }
        public void Clear() { Console.Clear(); }
        public void Write(string input) { Console.Write(input); }
        public ConsoleKeyInfo ReadKey(bool intercept) { return Console.ReadKey(intercept); }
        public string ReadLine() { return Console.ReadLine(); }
        public ConsoleCursor GetCursorPosition() { return ConsoleCursor.CreateFromConsole(); }

        public void SetBackgroundColor(ConsoleColor value) { Console.BackgroundColor = value; }
        public void SetForeColor(ConsoleColor value) { Console.ForegroundColor = value; }
        /// <summary>
        /// Set cursor position
        /// </summary>
        /// <param name="x">X or -1 for not change</param>
        /// <param name="y">Y or -1 for not change</param>
        public void SetCursorPosition(int x, int y)
        {
            if (x >= 0) Console.CursorLeft = x;
            if (y >= 0) Console.CursorTop = y;
        }
        public void SetCursorMode(ConsoleCursor.ECursorMode mode)
        {
            switch (mode)
            {
                case ConsoleCursor.ECursorMode.Hidden:
                    {
                        Console.CursorVisible = false;
                        break;
                    }
                case ConsoleCursor.ECursorMode.Visible:
                    {
                        Console.CursorVisible = true;
                        Console.CursorSize = 100;
                        break;
                    }
                case ConsoleCursor.ECursorMode.Small:
                    {
                        Console.CursorVisible = true;
                        Console.CursorSize = 10;
                        break;
                    }
            }
        }
    }
}