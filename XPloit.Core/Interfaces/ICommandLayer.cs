using System;

namespace XPloit.Core.Interfaces
{
    public interface ICommandLayer
    {
        // Input / Output

        void Clear();
        void Write(string line, bool appendNewLine);
        string ReadLine();
        
        // Colors

        void SetForeColor(ConsoleColor color);
        void SetBackgroundColor(ConsoleColor color);
    }
}