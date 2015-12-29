using System;
using System.Collections.Generic;

namespace XPloit.Core.Command.Interfaces
{
    public interface IAutoCompleteSource
    {
        IEnumerable<string> GetCommand();
        IEnumerable<string> GetArgument(string command, string[] argument);
        StringComparison ComparisonMethod { get; }
    }
}