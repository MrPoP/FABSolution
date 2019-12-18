using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct CustomerReport
    {
        public static CustomerReport Empty = default(CustomerReport);

        public int ID;
        public string Name;
        public double TradablePoints;
        public ContactDetails ContactDetails;
        public List<int> Cheques;
        public List<PerformanceReport> Reports;

        public static CustomerReport Create(int _ID, string _Name, double _TradablePoints, ContactDetails _ContactDetails
            , List<int> _Cheques, List<PerformanceReport> _Reports)
        {
            return new CustomerReport()
            {
                ID = _ID,
                Name = _Name,
                TradablePoints = _TradablePoints,
                ContactDetails = _ContactDetails,
                Cheques = _Cheques,
                Reports = _Reports
            };
        }
    }
}
