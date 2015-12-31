using System;
using XPloit.Core.Collections;
using XPloit.Core.Interfaces;
using XPloit.Res;

namespace XPloit.Core
{
    public class Job
    {
        public class IJobable : IDisposable
        {
            bool _IsDisposed = false;
            /// <summary>
            /// Is Disposed
            /// </summary>
            public bool IsDisposed { get { return _IsDisposed; } }

            public virtual void OnDispose() { }

            /// <summary>
            /// Free resources
            /// </summary>
            public void Dispose()
            {
                if (_IsDisposed) return;

                _IsDisposed = true;
                OnDispose();
            }
        }

        IJobable _Object;
        uint _Id;

        /// <summary>
        /// Object
        /// </summary>
        public IJobable Object { get { return _Object; } }
        /// <summary>
        /// Id
        /// </summary>
        public uint Id
        {
            get { return _Id; }
            internal set { _Id = value; }
        }

        /// <summary>
        /// IsRunning
        /// </summary>
        public bool IsRunning { get { return _Object != null && _Object.IsDisposed; } }

        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="obj">Object for dispose</param>
        public static Job Create(IModule module, IJobable obj)
        {
            Job j = new Job(obj);
            // Append to global list
            JobCollection.Current.Add(j);

            module.WriteInfo(Lang.Get("Job_Created"), j.Id.ToString(), ConsoleColor.Green);
            return j;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Object</param>
        private Job(IJobable obj)
        {
            _Object = obj;
        }
    }
}