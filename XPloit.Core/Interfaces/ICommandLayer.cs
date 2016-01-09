using System;
using System.Collections.Generic;
using XPloit.Core.Command.Interfaces;

namespace XPloit.Core.Interfaces
{
    public delegate void PromptDelegate(ICommandLayer sender);

    public interface ICommandLayer
    {
        // Input / Output

        void Clear();
        void Write(string line);
        void WriteLine(string line);
        string ReadLine(PromptDelegate prompt, IAutoCompleteSource source);
        string ReadPassword(PromptDelegate prompt);

        // Helpers

        void WriteError(string error);
        void WriteInfo(string info);
        void WriteInfo(string info, string colorText, ConsoleColor color);

        // Progress

        bool IsInProgress { get; }
        void WriteProgress(double value);
        void EndProgress();
        void StartProgress(double max);

        // Sound


        void Beep();

        // Colors

        void SetForeColor(ConsoleColor color);
        void SetBackgroundColor(ConsoleColor color);

        void AddInput(string input);
        void AddInput(IEnumerable<string> enumerable);
    }
}