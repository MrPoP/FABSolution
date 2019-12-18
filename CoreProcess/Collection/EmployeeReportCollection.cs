using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using CoreProcess.Structure;

namespace CoreProcess.Collection
{
    public class EmployeeReportCollection : ICollection
    {
        private Dictionary<long, EmployeeReport> items = null;
        private object syncRoot = null;
        private long time = 0;
        private int ownerid = 0;

        public List<EmployeeReport> Employees { get { return this.items.Values.ToList(); } }
        public List<int> EmployeesReportsIDs { get { return Employees.Select(x => x.ID).ToList(); } }
        private EmployeeReport[] EmployeesArr { get { return this.items.Values.ToArray(); } }

        public EmployeeReportCollection(int _ownerid)
        {
            this.syncRoot = new object();
            this.ownerid = _ownerid;
            this.items = new Dictionary<long, EmployeeReport>();
        }

        public IEnumerable<EmployeeReport> GetEmployer(Func<long, bool> predicate = null, int _month = 0, int _year = 0)
        {
            if (_month == 0 && _year == 0 && predicate != null)
            {
                foreach (EmployeeReport report in Employees)
                {
                    if (predicate(report.Time))
                        yield return report;
                }
                yield break;
            }
            else if (_month != 0 && _year == 0 && predicate != null)
            {
                foreach (EmployeeReport report in Employees.Where(p => p.MonthID == _month))
                {
                    if (predicate(report.Time))
                        yield return report;
                }
                yield break;
            }
            else if (_month == 0 && _year != 0 && predicate != null)
            {
                foreach (EmployeeReport report in Employees.Where(p => p.YearID == _year))
                {
                    if (predicate(report.Time))
                        yield return report;
                }
                yield break;
            }
            else if (predicate == null)
            {
                yield break;
            }
            else
            {
                foreach (EmployeeReport report in Employees.Where(p => p.MonthID == _month && p.YearID == _year))
                {
                    if (predicate(report.Time))
                        yield return report;
                }
                yield break;
            }
        }
        public bool AddReport(EmployeeReport _report)
        {
            lock (this.syncRoot)
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
        public void UpdateTime(long _time)
        {
            this.time = _time;
        }
        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
                this.EmployeesArr.CopyTo(array, index);
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
                return this.EmployeesArr.GetEnumerator();
            }
        }
    }
}
