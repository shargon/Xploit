using System.Collections.Generic;

namespace XPloit.Core.Collections
{
    public class JobCollection : IEnumerable<Job>
    {
        List<Job> _InternalList = new List<Job>();

        static JobCollection _Current = null;

        /// <summary>
        /// Current Exploits
        /// </summary>
        public static JobCollection Current
        {
            get
            {
                if (_Current == null) _Current = new JobCollection();
                return _Current;
            }
        }

        protected JobCollection()
        {
        }

        public int IndexOf(Job item) { return _InternalList.IndexOf(item); }
        public Job this[int index] { get { return _InternalList[index]; } }
        internal void Add(Job item) { _InternalList.Add(item); }
        public bool Contains(Job item) { return _InternalList.Contains(item); }
        public void CopyTo(Job[] array, int arrayIndex) { _InternalList.CopyTo(array, arrayIndex); }
        public int Count { get { return _InternalList.Count; } }
        public IEnumerator<Job> GetEnumerator() { return _InternalList.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _InternalList.GetEnumerator();
        }
    }
}