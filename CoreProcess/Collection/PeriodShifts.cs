using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Structure;
using System.Collections.ObjectModel;
using System.Collections;

namespace CoreProcess.Collection
{
    public class PeriodShifts : ICollection
    {
        private object syncRoot = null;
        private int employeeid = 0;
        private Shift[] shifts = null;
        private int monthid = 0, yearid = 0, daysinmonth = 0, lastdaycheckin = 0;

        public ReadOnlyCollection<Shift> Values { get { return Array.AsReadOnly(shifts); } }
        public int DaysCount { get { return daysinmonth; } }
        public int EmployeeID { get { return employeeid; } }
        public int Month { get { return monthid; } }
        public int Year { get { return yearid; } }
        public Shift[] InnerValues { get { return shifts; } }

        public PeriodShifts(int _employeeid)
        {
            syncRoot = new object();
            employeeid = _employeeid;
        }
        public PeriodShifts(int _employeeid, int _monthid, int _yearid, int _daysinmonth)
        {
            syncRoot = new object();
            employeeid = _employeeid;
            monthid = _monthid;
            yearid = _yearid;
            daysinmonth = _daysinmonth;
            shifts = new Shift[daysinmonth];
            for (int x = 0; x < shifts.Length; x++)
            {
                shifts[x] = new Shift(x + 1, monthid);
            }
        }

        public void SetData(int _monthid, int _yearid, int _daysinmonth)
        {
            lock (syncRoot)
            {
                if (shifts == null)
                {
                    monthid = _monthid;
                    yearid = _yearid;
                    daysinmonth = _daysinmonth;
                    shifts = new Shift[daysinmonth];
                    for (int x = 0; x < shifts.Length; x++)
                    {
                        shifts[x] = new Shift(x + 1, monthid);
                    }
                }
            }
        }
        public void CheckIn(int dayid, int hourid, int minuteid)
        {
            lock (syncRoot)
            {
                if (lastdaycheckin == 0)
                {
                    lastdaycheckin = dayid;
                    shifts[dayid - 1].CheckIn(hourid, minuteid);
                }
            }
        }
        public bool CheckOut(int hourid, int minuteid)
        {
            lock (syncRoot)
            {
                if (lastdaycheckin > 0)
                {
                    shifts[lastdaycheckin - 1].CheckOut(hourid, minuteid);
                    lastdaycheckin = 0;
                    return true;
                }
                return false;
            }
        }
        public void CheckOut(int dayid, int hourid, int minuteid)
        {
            lock (syncRoot)
            {
                lastdaycheckin = 0;
                shifts[dayid - 1].CheckOut(hourid, minuteid);
            }
        }

        ~PeriodShifts()
        {
            syncRoot = null;
            employeeid = 0;
            shifts = null;
            monthid = 0; yearid = 0; daysinmonth = 0; lastdaycheckin = 0;
        }

        public void CopyTo(Array array, int index)
        {
            lock (syncRoot)
            {
                shifts.CopyTo(array, index);
            }
        }

        public int Count
        {
            get { return shifts.Length; }
        }

        public bool IsSynchronized
        {
            get { return syncRoot != null; }
        }

        public object SyncRoot
        {
            get { return syncRoot; }
        }

        public IEnumerator GetEnumerator()
        {
            return shifts.GetEnumerator();
        }
    }
}
