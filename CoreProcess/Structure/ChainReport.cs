using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct ChainReport
    {
        public static ChainReport Empty = default(ChainReport);

        public int ID;
        public int ChainManagerID;
        public int AreasCount;
        public List<int> AreasIDs;
        public List<long> AreasReportsTimes;
        public ContactDetails ContactDetails;
        public List<CustomerReport> CustomersReports;

        public static ChainReport Create(int _ID, int _ChainManagerID, int _AreasCount, List<int> _AreasIDs,
           ContactDetails _ContactDetails, List<long> _AreasReportsTimes, List<CustomerReport> _CustomersReports)
        {
            return new ChainReport()
            {
                ID = _ID,
                ChainManagerID = _ChainManagerID,
                AreasCount = _AreasCount,
                AreasIDs = _AreasIDs,
                ContactDetails = _ContactDetails,
                AreasReportsTimes = _AreasReportsTimes,
                CustomersReports = _CustomersReports
            };
        }
    }
}
