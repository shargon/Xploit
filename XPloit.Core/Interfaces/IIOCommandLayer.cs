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
        void SetCursorPosition(int x, int y);
        void GetCursorPosition(ref int x,ref  int y);
        void SetCursorVisible(bool visible);
    }
}