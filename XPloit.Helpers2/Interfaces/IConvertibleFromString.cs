namespace XPloit.Helpers.Interfaces
{
    public interface IConvertibleFromString
    {
        void LoadFromString(string input);
        string ToString();
    }
}