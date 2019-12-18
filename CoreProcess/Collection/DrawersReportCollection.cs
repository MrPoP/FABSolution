using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using CoreProcess.Structure;
using System.Collections.ObjectModel;

namespace CoreProcess.Collection
{
    public class DrawersReportCollection : ICollection
    {
        private Dictionary<long, DrawerReport> items = null;
        private long time = 0;
        private object syncRoot = null;
        private int ownerid = 0, gmanagerid = 0;

        public ReadOnlyCollection<DrawerReport> ProtectedReports { get { return Array.AsReadOnly(this.items.Values.ToArray()); } }
        public DrawerReport[] Reports { get { return this.items.Values.ToArray(); } }
        public List<int> DrawersReportsIDs { get { return Reports.Select(x => x.ID).ToList(); } }

        public DrawersReportCollection(int _ownerid, int _gmanagerid)
        {
            this.syncRoot = new object();
            this.ownerid = _ownerid;
            this.gmanagerid = _gmanagerid;
            this.items = new Dictionary<long, DrawerReport>();
            this.time = 0;
        }

        public IEnumerable<DrawerReport> GetReports(Func<DrawerReport, bool> predicate)
        {
            foreach (DrawerReport report in Reports.Where(predicate))
            {
                if (predicate(report))
                    yield return report;
            }
        }
        /// <summary>
        /// get elements of the collection not more than capacity you wrote
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>elements of the collection not more than capacity you wrote</returns>
        public IEnumerable<DrawerReport> GetReports(Func<DrawerReport, int, bool> predicate)
        {
            int counter = 0;
            foreach (DrawerReport report in Reports.Where(predicate))
            {
                if (predicate(report, counter))
                {
                    yield return report;
                    counter++;
                }
            }
        }
        public bool AddReport(DrawerReport _report)
        {
            lock (this.syncRoot)
            {
                if (_report.OwnerID == this.ownerid)
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
        public void UpdateTime(long _time)
        {
            this.time = _time;
        }
        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
                this.Reports.CopyTo(array, index);
            }
        }
        public int Count
        {
            get { return this.items.Count; }
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
            lock (this.syncRoot)
            {
                return this.Reports.GetEnumerator();
            }
        }

        ~DrawersReportCollection()
        {
            this.syncRoot = null;
            this.ownerid = 0;
            this.gmanagerid = 0;
            this.items = null;
            this.time = 0;
        }
    }
}
