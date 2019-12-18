using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct DepartmentReport
    {
        public static DepartmentReport Empty = default(DepartmentReport);

        public long Time;
        public int ID;
        public int OwnerID;
        public int EmployeeID;
        public string Address;
        public byte Flag;
        public byte ReportFlag;
        public int AwaitingCheques;

        public static DepartmentReport Create(int _ID, int _OwnerID, int _EmployeeID, string _Address,
            byte _Flag, byte _ReportFlag, int _AwaitingCheques)
        {
            return new DepartmentReport()
            {
                ID = _ID,
                OwnerID = _OwnerID,
                EmployeeID = _EmployeeID,
                Address = _Address,
                Flag = _Flag,
                ReportFlag = _ReportFlag,
                AwaitingCheques = _AwaitingCheques
            };
        }
    }
}
