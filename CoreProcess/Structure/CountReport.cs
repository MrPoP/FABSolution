using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CoreProcess.Structure
{
    public class CountReport
    {
        private RawCountFlag flag = RawCountFlag.Daily;
        private int ownerid = 0;
        private int id = 0, managerid = 0, dayofmonth = 0, monthofyear = 0, yearid = 0;
        private long time = 0;
        private Dictionary<string, CountReportItem> items = null;

        public DateTime FullDate { get { return DateTime.FromBinary(time); } }
        public RawCountFlag CountFlag { get { return this.flag; } }
        public int OwnerID { get { return ownerid; } }
        public int ID { get { return id; } }
        public int ManagerID { get { return managerid; } }
        public string ForDate { get { return string.Format("{0}/{1}/{2}", dayofmonth, monthofyear, yearid); } }
        public CountReportItem[] Items { get { return items.Values.ToArray(); } }
        //public ReadOnlyCollection<RawItem> CountResult { get { return Array.AsReadOnly<RawItem>(items.Values.ToList().ConvertAll<RawItem>(CountReportItem.Converter).ToArray()); } }
        public ReadOnlyCollection<RawItem> CountResult { get { return Array.AsReadOnly(items.Values.Cast<RawItem>().ToArray()); } }

        public CountReport(int _managerid, int _dayofmonth, int _monthofyear, int _yearid)
        {
            id = new Random().Next(-1, int.MaxValue);
            managerid = _managerid;
            dayofmonth = _dayofmonth;
            monthofyear = _monthofyear;
            yearid = _yearid;
        }

        public void SetFlag(RawCountFlag _flag)
        {
            flag = _flag;
        }
        public void AddItems(params RawItem[] _inventoryitems)
        {
            foreach (RawItem item in _inventoryitems)
            {
                items.Add(item.Name, CountReportItem.ReverseConverter(item));
            }
        }
        public void SetActual(string name, double stock, out List<Exception> thrownexception)
        {
            thrownexception = new List<Exception>();
            lock (this.items)
            {
                if (this.items.ContainsKey(name))
                {
                    this.items[name].SetActual(stock);
                }
                else
                {
                    thrownexception.Add(new Exception(string.Format("StockItemNotExistedException: Couldn't find item {0}.", name)));
                    return;
                }
            }
        }

        ~CountReport()
        {
            flag = RawCountFlag.Daily;
            ownerid = 0;
            id = 0;
            managerid = 0;
            items = null;
            time = 0;
        }
    }
    public class CountReportItem
    {
        public static readonly Converter<CountReportItem, RawItem> Converter = (obj) => (RawItem)obj;
        public static readonly Converter<RawItem, CountReportItem> ReverseConverter = (obj) => (CountReportItem)obj;
        private RawCountIndicatorFlag averageflag = RawCountIndicatorFlag.None;
        private double average = 0.0;
        private double auditcount = 0.0;
        private string name = string.Empty;
        private Units unit = Units.None;
        private double actualcount = 0.0;
        private RawCountFlag flag = RawCountFlag.Daily;

        public RawCountIndicatorFlag AverageFlag { get { return this.averageflag; } }
        public double Average { get { return average; } }
        public double AuditCount { get { return Math.Round(auditcount, 3); } }
        public double ActualCount { get { return Math.Round(actualcount, 3); } }
        public Units Unit { get { return this.unit; } }
        public string Name { get { return this.name; } }
        public RawCountFlag CountFlag { get { return this.flag; } }

        public CountReportItem(RawItem rawitem)
        {
            name = rawitem.Name;
            auditcount = rawitem.Stock;
            unit = rawitem.Unit;
            flag = rawitem.CountFlag;
        }

        public void SetActual(double _actualcount)
        {
            if (auditcount > actualcount)
            {
                averageflag = RawCountIndicatorFlag.Shortage;
                average = auditcount - actualcount;
            }
            else if (auditcount < actualcount)
            {
                averageflag = RawCountIndicatorFlag.Growing;
                average = actualcount - auditcount;
            }
            else
            {
                averageflag = RawCountIndicatorFlag.None;
                average = 0.0;
            }
            actualcount = _actualcount;
        }

        ~CountReportItem()
        {
            averageflag = RawCountIndicatorFlag.None;
            average = 0.0;
            auditcount = 0.0;
            name = string.Empty;
            unit = Units.None;
            flag = RawCountFlag.Daily;
        }

        public static implicit operator RawItem(CountReportItem item)
        {
            return new RawItem(item.name, item.unit, item.actualcount, item.flag);
        }
        public static implicit operator CountReportItem(RawItem item)
        {
            return new CountReportItem(item);
        }
    }
}
