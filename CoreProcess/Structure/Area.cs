using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public class Area
    {
        private int id = 0, areamanagerid = 0, chainid = 0;
        private Dictionary<int, Restaurant> items = null;
        private Dictionary<int, Dictionary<long, RestaurantReport>> reports = null;
        private ContactDetails contactdetails = ContactDetails.Default;

        public int ID { get { return this.id; } }
        public int ChainID { get { return this.chainid; } }
        public int AreaManagerID { get { return this.areamanagerid; } }
        public int RestaurantsCount { get { return this.items.Count; } }
        public ContactDetails ContactDetails { get { return this.contactdetails; } }
        public List<int> RestaurantsIDs { get { return this.items.Keys.ToList(); } }
        public int RestaurantsReportsCount { get { return this.reports.Values.Count; } }
        public List<long> RestaurantsReportsTimes { get { return this.reports.Values.SelectMany(x => x.Keys).ToList(); } }
        public List<RestaurantReport> RestaurantsReports { get { return this.reports.Values.SelectMany(x => x.Values).ToList(); } }

        public Area(int _id, int _areamanagerid, int _chainid)
        {
            this.id = _id;
            this.areamanagerid = _areamanagerid;
            this.chainid = _chainid;
            this.items = new Dictionary<int, Restaurant>();
            this.reports = new Dictionary<int, Dictionary<long, RestaurantReport>>();
        }

        public void SetContact(ContactDetails _contactdetails)
        {
            this.contactdetails = _contactdetails;
        }
        public bool GetReport(int restaurantid, long time, out RestaurantReport Report)
        {
            Dictionary<long, RestaurantReport> values = null;
            Report = RestaurantReport.Empty;
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
        public bool GetReports(out List<RestaurantReport> Reports, int restaurantid = 0, long time = 0)
        {
            Dictionary<long, RestaurantReport> values = null;
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
                Reports = this.RestaurantsReports.Where(p => DateTime.FromBinary(p.Time).Month == DateTime.FromBinary(time).Month).ToList();
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

        ~Area()
        {
            this.id = 0;
            this.areamanagerid = 0;
            this.chainid = 0;
            this.items = null;
            this.reports = null;
            this.contactdetails = ContactDetails.Default;
        }

        public static implicit operator AreaReport(Area area)
        {
            return AreaReport.Create(area.id, area.chainid, area.areamanagerid, area.RestaurantsCount,
                area.contactdetails, area.RestaurantsIDs, area.RestaurantsReportsCount, area.RestaurantsReportsTimes);
        }
    }
}
