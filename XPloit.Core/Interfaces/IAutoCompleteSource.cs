using System;
using System.Collections.Generic;

namespace XPloit.Core.Interfaces
{
    public interface IAutoCompleteSource
    {
        IEnumerable<string> GetCommand();
        IEnumerable<string> GetArgument(string command, int argumentNumber);
        StringComparison ComparisonMethod { get; }
    }
}