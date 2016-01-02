using System.Collections.Generic;

namespace XPloit.Core.Collections
{
    public class JobCollection : IEnumerable<Job>
    {
        uint _Id = 0;
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

        protected JobCollection() { }

        public int IndexOf(Job item) { return _InternalList.IndexOf(item); }
        public Job this[int index] { get { return _InternalList[index]; } }
        /// <summary>
        /// Add a job to the list
        /// </summary>
        /// <param name="item">Job</param>
        internal void Add(Job item)
        {
            item.Id = _Id;
            _InternalList.Add(item);
            _Id++;
        }
        public bool Contains(Job item) { return _InternalList.Contains(item); }
        public void CopyTo(Job[] array, int arrayIndex) { _InternalList.CopyTo(array, arrayIndex); }
        public int Count { get { return _InternalList.Count; } }
        public IEnumerator<Job> GetEnumerator() { return _InternalList.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _InternalList.GetEnumerator();
        }

        public bool Kill(int jobId)
        {
            Job k = Search(jobId);
            if (k != null && k.Kill())
            {
                _InternalList.Remove(k);
                return true;
            }
            return false;
        }

        public Job Search(int jobId)
        {
            lock (_InternalList)
                foreach (Job j in _InternalList)
                {
                    if (j.Id == jobId) return j;
                }
            return null;
        }
        /// <summary>
        /// Kill all jobs
        /// </summary>
        public void KillAll()
        {
            lock (_InternalList)
            {
                for (int x = Count - 1; x >= 0; x--)
                {
                    _InternalList[x].Kill();
                }

                _InternalList.Clear();
            }
        }
    }
}