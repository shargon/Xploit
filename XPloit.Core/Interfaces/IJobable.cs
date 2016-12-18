using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace XPloit.Core.Interfaces
{
    public class IJobable : IDisposable
    {
        Process _Process;
        Thread _Thread;
        IDisposable _Object;
        bool _IsDisposed = false;
        PropertyInfo _IsDisposedProperty;

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
                    (_Thread != null && !_Thread.IsAlive) ||
                    (_Object != null && (bool)_IsDisposedProperty.GetValue(_Object) == true);
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
        internal IJobable(IDisposable obj, string isDisposedProperty)
        {
            _Object = obj;
            _IsDisposedProperty = obj.GetType().GetProperty(isDisposedProperty);
            if (_IsDisposedProperty == null)
                throw new Exception("Property not found: '" + isDisposedProperty + "'");
        }

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

            if (_Object != null)
            {
                try { _Object.Dispose(); } catch { }
                _Object = null;
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