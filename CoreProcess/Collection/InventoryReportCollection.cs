using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Structure;
using System.Collections;
using System.Collections.ObjectModel;

namespace CoreProcess.Collection
{
    public class InventoryReportCollection : ICollection//, IAppDomainSetup
    {
        private Dictionary<RawCountFlag, List<CountReport>> items = null;
        private object syncRoot = null;
        private long time = 0;
        private int ownerid = 0;

        public int Count { get { return CountReports.Count(); } }
        public List<CountReport> CountReports { get { return items.Values.SelectMany(x => x).ToList(); } }
        public List<int> CountReportsIDs { get { return CountReports.Select(x => x.ID).ToList(); } }
        public ReadOnlyCollection<CountReport> Reports { get { return Array.AsReadOnly(items.Values.SelectMany(x => x).ToArray()); } }
        public ReadOnlyCollection<string> RecordedDates { get { return Array.AsReadOnly(items.Values.SelectMany(x => x).Select(x => x.ForDate).ToArray()); } }

        public InventoryReportCollection(int _ownerid)
        {
            this.syncRoot = new object();
            this.ownerid = _ownerid;
            items = new Dictionary<RawCountFlag, List<CountReport>>();
            items.Add(RawCountFlag.Daily, new List<CountReport>());
            items.Add(RawCountFlag.Weekly, new List<CountReport>());
            items.Add(RawCountFlag.Monthly, new List<CountReport>());
        }

        public void UpdateTime(long _time)
        {
            this.time = _time;
        }
        public void AddReport(CountReport _report)
        {
            lock (this.syncRoot)
            {
                List<CountReport> reports = null;
                if (this.items.TryGetValue(_report.CountFlag, out reports))
                {
                    reports.Add(_report);
                }
            }
        }
        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
                items.Values.SelectMany(x => x).ToArray().CopyTo(array, index);
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
            lock (this.syncRoot)
            {
                return items.Values.SelectMany(x => x).GetEnumerator();
            }
        }

        ~InventoryReportCollection()
        {
            this.time = 0;
            this.items = null;
            this.syncRoot = null;
        }
    }
}
