using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Structure;
using System.Collections;

namespace CoreProcess.Collection
{
    public class DepartmentReportCollection : ICollection
    {
        private Dictionary<long, DepartmentReport> items = null;
        private long time = 0;
        private int ownerid = 0;
        private object syncRoot = null;

        public List<DepartmentReport> Reports { get { return this.items.Values.ToList(); } }
        public List<long> Keys { get { return this.items.Keys.ToList(); } }
        public int Count { get { return this.items.Count(); } }

        public DepartmentReportCollection(int _ownerid)
        {
            this.syncRoot = new object();
            this.ownerid = _ownerid;
            this.items = new Dictionary<long, DepartmentReport>();
        }

        public IEnumerable<DepartmentReport> GetReports(Func<long, bool> predicate = null, int _id = 0)
        {
            if (_id == 0 && predicate != null)
            {
                foreach (DepartmentReport report in Reports)
                {
                    if (predicate(report.Time))
                        yield return report;
                    continue;
                }
                yield break;
            }
            else if (_id != 0 && predicate == null)
            {
                foreach (DepartmentReport report in Reports.Where(p => p.ID == _id))
                {
                    yield return report;
                }
                yield break;
            }
            else
            {
                foreach (DepartmentReport report in Reports.Where(p => p.ID == _id))
                {
                    if (predicate(report.Time))
                        yield return report;
                    continue;
                }
                yield break;
            }
        }
        public void UpdateTime(long _time)
        {
            this.time = _time;
        }
        public bool AddReport(DepartmentReport _report)
        {
            lock (this.items)
            {
                if (this.items.ContainsKey(_report.Time))
                {
                    this.items.Add(_report.Time, _report);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
                this.Reports.ToArray().CopyTo(array, index);
            }
        }
        public bool IsSynchronized
        {
            get { return this.syncRoot != null; }
        }
        public object SyncRoot
        {
            get { return this.syncRoot; }
        }
        public IEnumerator GetEnumerator()
        {
            return this.Reports.ToArray().GetEnumerator();
        }

        ~DepartmentReportCollection()
        {
            this.ownerid = 0;
            this.time = 0;
            this.items = null;
            this.syncRoot = null;
        }
    }
}
