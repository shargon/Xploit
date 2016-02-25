using System;
using System.Diagnostics;
using System.Threading;

namespace XPloit.Core.Interfaces
{
    public class IJobable : IDisposable
    {
        Process _Process;
        Thread _Thread;
        bool _IsDisposed = false;

        /// <summary>
        /// Is Disposed
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return _IsDisposed || 
                    (_Process != null && _Process.HasExited) ||
                    (_Thread != null && !_Thread.IsAlive);
            }
        }

        public virtual void OnDispose() { }

        internal IJobable() { }
        internal IJobable(Process pr) { _Process = pr; }
        internal IJobable(Thread th) { _Thread = th; }

        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            if (_IsDisposed) return;

            _IsDisposed = true;

            if (_Process != null)
            {
                try { _Process.Kill(); } catch { }
                _Process.Dispose();
                _Process = null;
            }

            if (_Thread != null)
            {
                try { _Thread.Abort(); } catch { }
                _Thread = null;
            }

            OnDispose();
        }

        //public static explicit operator IJobable(Process pr) { return new IJobable(pr); }
        public static implicit operator IJobable(Process pr) { return new IJobable(pr); }
        public static implicit operator IJobable(Thread th) { return new IJobable(th); }
    }
}