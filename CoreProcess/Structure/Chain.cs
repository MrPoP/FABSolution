using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Collection;

namespace CoreProcess.Structure
{
    public class Chain
    {
        private int id = 0, chainmanagerid = 0;
        private Dictionary<int, Area> items = null;
        private Dictionary<int, Dictionary<long, AreaReport>> reports = null;
        private ContactDetails contactdetails = ContactDetails.Default;
        private CustomerCollection customersreports = null;

        public int ID { get { return this.id; } }
        public int ChainManagerID { get { return this.chainmanagerid; } }
        public int AreasCount { get { return this.items.Count; } }
        public List<int> AreasIDs { get { return this.items.Keys.ToList(); } }
        public int AreasReportsCount { get { return this.reports.Values.Count; } }
        public List<long> AreasReportsTimes { get { return this.reports.Values.SelectMany(x => x.Keys).ToList(); } }
        private List<AreaReport> AreasReports { get { return this.reports.Values.SelectMany(x => x.Values).ToList(); } }
        public ContactDetails ContactDetails { get { return this.contactdetails; } }
        public CustomerCollection Customersreports { get { return this.customersreports; } }

        public Chain(int _id, int _chainmanagerid)
        {
            this.id = _id;
            this.chainmanagerid = _chainmanagerid;
            this.items = new Dictionary<int, Area>();
            this.reports = new Dictionary<int, Dictionary<long, AreaReport>>();
            this.customersreports = new CustomerCollection(this.id);
        }

        public void SetContact(ContactDetails _contactdetails)
        {
            this.contactdetails = _contactdetails;
        }
        public bool GetReport(int restaurantid, long time, out AreaReport Report)
        {
            Dictionary<long, AreaReport> values = null;
            Report = AreaReport.Empty;
            if (this.reports.TryGetValue(restaurantid, out values))
            {
                if (values.TryGetValue(time, out Report))
                {
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
        public bool GetReports(out List<AreaReport> Reports, int restaurantid = 0, long time = 0)
        {
            Dictionary<long, AreaReport> values = null;
            Reports = null;
            if (restaurantid != 0 && time == 0)
            {
                if (this.reports.TryGetValue(restaurantid, out values))
                {
                    Reports = values.Select(x => x.Value).ToList();
                }
            }
            if (restaurantid == 0 && time != 0)
            {
                Reports = this.AreasReports.Where(p => DateTime.FromBinary(p.Time).Month == DateTime.FromBinary(time).Month).ToList();
            }
            if (restaurantid != 0 && time != 0)
            {
                if (this.reports.TryGetValue(restaurantid, out values))
                {
                    Reports = values.Select(x => x.Value).Where(p => DateTime.FromBinary(p.Time).Month == DateTime.FromBinary(time).Month).ToList();
                }
            }
            return Reports != null;
        }

        ~Chain()
        {
            this.id = 0;
            this.chainmanagerid = 0;
            this.items = null;
            this.reports = null;
            this.customersreports = null;
        }

        public static implicit operator ChainReport(Chain chain)
        {
            return ChainReport.Create(chain.id, chain.chainmanagerid, chain.items.Count, chain.AreasIDs,
                chain.contactdetails, chain.AreasReportsTimes, chain.Customersreports.CustomersReports);
        }
    }
}
