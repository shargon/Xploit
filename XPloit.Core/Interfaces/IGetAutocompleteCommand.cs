using XPloit.Core.Enums;

namespace XPloit.Core.Interfaces
{
    public interface IGetAutocompleteCommand
    {
        EAllowAutocompleteCommand AllowAutocompleteFiles { get; }
        EAllowAutocompleteCommand AllowAutocompleteFolders { get; }
        EAllowAutocompleteCommand AllowAutocompleteMaths { get; }

        string[] AvailableCommands();
        string[] AvailableCommandOptions(string command);
    }
}