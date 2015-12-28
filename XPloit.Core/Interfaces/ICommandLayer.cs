using System;
using System.Collections.Generic;

namespace XPloit.Core.Interfaces
{
    public delegate void PromptDelegate(ICommandLayer sender);

    public interface ICommandLayer
    {
        // Input / Output

        void Clear();
        void Write(string line);
        void WriteLine(string line);
        string ReadLine(PromptDelegate prompt);
        
        // Colors

        void SetForeColor(ConsoleColor color);
        void SetBackgroundColor(ConsoleColor color);

        void AddInput(string input);
        void AddInput(IEnumerable<string> enumerable);
    }
}