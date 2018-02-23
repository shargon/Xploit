using System;
using XPloit.Core.Listeners.IO;
using XPloit.Core.Listeners.Layer;

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

        void SetCursorPosition(int x, int y);
        void SetCursorMode(ConsoleCursor.ECursorMode mode);

        ConsoleCursor GetCursorPosition();
    }
}