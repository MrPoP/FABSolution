using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct AreaReport
    {
        public static AreaReport Empty = default(AreaReport);

        public long Time;
        public int ID;
        public int ChainID;
        public int AreaManagerID;
        public int RestaurantsCount;
        public ContactDetails ContactDetails;
        public List<int> RestaurantsIDs;
        public int RestaurantsReportsCount;
        public List<long> RestaurantsReportsTimes;

        public static AreaReport Create(int _ID, int _ChainID, int _AreaManagerID, int _RestaurantsCount,
           ContactDetails _ContactDetails, List<int> _RestaurantsIDs, int _RestaurantsReportsCount,
           List<long> _RestaurantsReportsTimes)
        {
            return new AreaReport()
            {
                ID = _ID,
                ChainID = _ChainID,
                AreaManagerID = _AreaManagerID,
                RestaurantsCount = _RestaurantsCount,
                ContactDetails = _ContactDetails,
                RestaurantsIDs = _RestaurantsIDs,
                RestaurantsReportsCount = _RestaurantsReportsCount,
                RestaurantsReportsTimes = _RestaurantsReportsTimes
            };
        }
    }
}
