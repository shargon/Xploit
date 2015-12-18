
namespace XPloit.Core.Interfaces
{
    public class IListener
    {
        /// <summary>
        /// Is Started
        /// </summary>
        public virtual bool IsStarted { get { return false; } }
        /// <summary>
        /// Start listener
        /// </summary>
        public virtual bool Start() { return false; }
        /// <summary>
        /// Stop listener
        /// </summary>
        public virtual bool Stop() { return false; }
    }
}