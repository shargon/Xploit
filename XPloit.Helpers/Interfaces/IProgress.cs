namespace XPloit.Helpers.Interfaces
{
    public interface IProgress
    {
        // Progress

        bool IsInProgress { get; }
        void WriteProgress(double value);
        void EndProgress();
        void StartProgress(double max);
    }
}