using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct DrawerReport
    {
        public static readonly DrawerReport Empty = Create();

        public int ID;
        public double ActualBot;
        public double StaffMealBot;
        public double CashInBot;
        public double RefundBot;
        public double TotalSalesBot;
        public double NetSales;
        public double BillExpenses;
        public double BeginningTerm;
        public int OwnerID;
        public int EmpolyeeID;
        public byte Flag;
        public int ManagerID;
        public List<int> Cheques;
        public byte Status;
        public List<long> Logs;
        public List<int> BillLogs;
        public long LastOrderTime;
        public long Time;

        public static DrawerReport Create()
        {
            return new DrawerReport()
            {
                ID = 0,
                ActualBot = 0.0,
                StaffMealBot = 0.0,
                CashInBot = 0.0,
                RefundBot = 0.0,
                TotalSalesBot = 0.0,
                NetSales = 0.0,
                BillExpenses = 0.0,
                BeginningTerm = 0.0,
                OwnerID = 0,
                EmpolyeeID = 0,
                Flag = 0,
                ManagerID = 0,
                Cheques = null,
                Status = 0,
                Logs = null,
                BillLogs = null,
                LastOrderTime = 0,
                Time = 0
            };
        }
        public static DrawerReport Create(int _id, double _actualbot, double _staffmealbot, double _cashinbot, double _refundbot, double _totalsalesbot,
            double _netsales, double _billexpanses, double _beginingterm, int _ownerid, int _employeeid, byte _flag, int _managerid, List<int> _cheques,
            byte _status, List<long> _logs, List<int> _billlogs, long _lastordertime, long _time)
        {
            return new DrawerReport()
            {
                ID = _id,
                ActualBot = _actualbot,
                StaffMealBot = _staffmealbot,
                CashInBot = _cashinbot,
                RefundBot = _refundbot,
                TotalSalesBot = _totalsalesbot,
                NetSales = _netsales,
                BillExpenses = _billexpanses,
                BeginningTerm = _beginingterm,
                OwnerID = _ownerid,
                EmpolyeeID = _employeeid,
                Flag = _flag,
                ManagerID = _managerid,
                Cheques = _cheques,
                Status = _status,
                Logs = _logs,
                BillLogs = _billlogs,
                LastOrderTime = _lastordertime,
                Time = _time
            };
        }
    }
}
