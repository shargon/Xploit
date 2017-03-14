using System;
using System.Text;
using XPloit.Core.Interfaces;

namespace XPloit.Core.Listeners.IO
{
    public class ConsoleIO : IIOCommandLayer
    {
        bool _IsInteractive;
        public Action<object, ConsoleCancelEventArgs> CancelKeyPress { get; set; }

        public ConsoleIO()
        {
            _IsInteractive = Environment.UserInteractive;

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            if (!_IsInteractive) return;
            Console.CancelKeyPress += Console_CancelKeyPress;
        }
        void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            CancelKeyPress?.Invoke(sender, e);
        }
        public void Beep()
        {
            if (!_IsInteractive) return;

            Console.Beep();
        }
        public void Clear()
        {
            if (!_IsInteractive) return;

            Console.Clear();
        }
        public void Write(string input)
        {
            if (!_IsInteractive) return;

            Console.Write(input);
        }
        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            if (!_IsInteractive) return new ConsoleKeyInfo('\0', ConsoleKey.NoName, false, false, false);

            return Console.ReadKey(intercept);
        }
        public string ReadLine()
        {
            if (!_IsInteractive) return "";

            return Console.ReadLine();
        }
        public ConsoleCursor GetCursorPosition()
        {
            if (!_IsInteractive) return ConsoleCursor.Empty;

            return ConsoleCursor.CreateFromConsole();
        }
        public void SetBackgroundColor(ConsoleColor value)
        {
            if (!_IsInteractive) return;

            Console.BackgroundColor = value;
        }
        public void SetForeColor(ConsoleColor value)
        {
            if (!_IsInteractive) return;

            Console.ForegroundColor = value;
        }
        /// <summary>
        /// Set cursor position
        /// </summary>
        /// <param name="x">X or -1 for not change</param>
        /// <param name="y">Y or -1 for not change</param>
        public void SetCursorPosition(int x, int y)
        {
            if (!_IsInteractive) return;

            if (x >= 0) Console.CursorLeft = x;
            if (y >= 0) Console.CursorTop = y;
        }
        public void SetCursorMode(ConsoleCursor.ECursorMode mode)
        {
            if (!_IsInteractive) return;

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