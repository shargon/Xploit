﻿using System;
using XPloit.Core.Collections;
using XPloit.Core.Interfaces;
using XPloit.Res;

namespace XPloit.Core
{
    public class Job
    {
        string _FullPathModule;
        IJobable _Object;
        uint _Id;

        /// <summary>
        /// Object
        /// </summary>
        public IJobable Object { get { return _Object; } }
        /// <summary>
        /// FullPathModule
        /// </summary>
        public string FullPathModule { get { return _FullPathModule; } }
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
        public bool IsRunning { get { return _Object != null && !_Object.IsDisposed; } }

        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="module">Module</param>
        /// <param name="obj">Object for dispose</param>
        public static Job Create(IModule module, IJobable obj)
        {
            if (module == null || obj == null) return null;

            Job j = new Job(obj, module.FullPath);
            // Append to global list
            JobCollection.Current.Add(j);

            module.WriteInfo(Lang.Get("Job_Created"), j.Id.ToString(), ConsoleColor.Green);
            return j;
        }
        /// <summary>
        /// Create a new job
        /// </summary>
        /// <param name="module">Module</param>
        /// <param name="obj">Object for dispose</param>
        /// <param name="isDisposedProperty">IsDisposed property</param>
        public static Job Create(IModule module, IDisposable obj, string isDisposedProperty)
        {
            if (module == null || obj == null) return null;

            return Create(module, new IJobable(obj, isDisposedProperty));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="fullPathModule">FullPath module</param>
        Job(IJobable obj, string fullPathModule)
        {
            _Object = obj;
            _FullPathModule = fullPathModule;
        }
        /// <summary>
        /// Kill the job
        /// </summary>
        /// <returns></returns>
        public bool Kill()
        {
            if (IsRunning)
            {
                if (_Object != null && !_Object.IsDisposed)
                    _Object.Dispose();
                return true;
            }

            if (_Object != null && !_Object.IsDisposed)
                _Object.Dispose();

            return true;
        }
    }
}