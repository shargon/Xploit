using System;
using XPloit.Core.Command;

namespace XPloit.Core.Interfaces
{
    public delegate void PromptDelegate(CommandLayer sender);

    public interface IIOCommandLayer
    {
        Action<object, ConsoleCancelEventArgs> CancelKeyPress { get; set; }

        // Input / Output

        void Clear();
        void Write(string input);
        string ReadLine();
        ConsoleKeyInfo ReadKey(bool intercept);

        // Sound

        void Beep();

        // Colors

        void SetForeColor(ConsoleColor color);
        void SetBackgroundColor(ConsoleColor color);

        // Cursor

        void SetCursorPositionX(int value);
        void SetCursorPositionY(int value);
        void SetCursorPosition(int x, int y);
        void SetCursorVisible(bool visible);

        void GetConsoleSize(out int w, out int h);
        void GetCursorPosition(out int x, out int y);
    }
}