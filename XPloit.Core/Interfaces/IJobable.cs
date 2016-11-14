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

        public event EventHandler OnDisposed;

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
        /// <summary>
        /// Tag
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// Dispose tag
        /// </summary>
        public bool DisposeTag { get; set; }
        public virtual void OnDispose() { }

        internal IJobable() { DisposeTag = true; }
        internal IJobable(Process pr) : this() { _Process = pr; }
        internal IJobable(Thread th) : this() { _Thread = th; }

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

            OnDisposed?.Invoke(this, EventArgs.Empty);
            if (DisposeTag && Tag != null && Tag is IDisposable)
            {
                ((IDisposable)Tag).Dispose();
                Tag = null;
            }
        }

        //public static explicit operator IJobable(Process pr) { return new IJobable(pr); }
        public static implicit operator IJobable(Process pr) { return new IJobable(pr); }
        public static implicit operator IJobable(Thread th) { return new IJobable(th); }
    }
}