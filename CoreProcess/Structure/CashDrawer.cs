using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Reports;
using CoreProcess.Collection;

namespace CoreProcess.Structure
{
    public class CashDrawer
    {
        private long time = 0, lastordertime = 0;
        private int id = 0;
        private int ownerid = 0, empolyeeid = 0, managerid = 0;
        private int lastorderid = 0;
        private byte xprint = 0, zprint = 0, dprint = 0;
        private List<Structure.Cheque> cheques = null;
        private DrawerFlag flag = DrawerFlag.None;
        private DrawerOperate status = DrawerOperate.None;
        private List<OperateLog> operatelogs = null;
        private List<Bill> billlogs = null;
        //////////finalbot////////
        private double actualcurrentbot { get { return Math.Round(((beginningterm + cashinbot) - (refundbot + billsexpenses)), 2); } }
        private double totalsalesbot { get { return Math.Round((cashinbot - refundbot), 2); } }
        private double netsales { get { return Math.Round((cashinbot - (refundbot + billsexpenses + beginningterm)), 2); } }
        //////////incomings//////
        private double cashinbot { get { return cheques.Where(p => p.Operator == ChequeCashOperator.CashIn).Sum(p => p.TotalPrice); } }//+
        private double refundbot { get { return cheques.Where(p => p.Operator == ChequeCashOperator.Refund).Sum(p => p.TotalPrice); } }//-
        private double staffmealbot { get { return cheques.Where(p => p.Operator == ChequeCashOperator.StaffMeal).Sum(p => p.TotalPrice); } }//X
        ///////////outgoings/////
        private double billsexpenses { get { return billlogs.Sum(p => p.TotalValue); } }//-
        private double beginningterm = 0;//-

        public int ID { get { return id; } }
        public double ActualBot { get { return actualcurrentbot; } }
        public double StaffMealBot { get { return staffmealbot; } }
        public double CashInBot { get { return cashinbot; } }
        public double RefundBot { get { return refundbot; } }
        public double TotalSalesBot { get { return totalsalesbot; } }
        public double NetSales { get { return netsales; } }
        public double BillExpenses { get { return billsexpenses; } }
        public double BeginningTerm { get { return beginningterm; } }
        public int OwnerID { get { return ownerid; } }
        public int EmpolyeeID { get { return empolyeeid; } }
        public DrawerFlag Flag { get { return flag; } }
        public int ManagerID { get { return managerid; } }
        public List<Cheque> Cheques { get { return cheques; } }
        public DrawerOperate Status { get { return status; } }
        public List<OperateLog> Logs { get { return operatelogs; } }
        public List<Bill> BillLogs { get { return billlogs; } }
        public DateTime LastOrderTime { get { return DateTime.FromBinary(lastordertime); } }
        public DateTime Time { get { return DateTime.FromBinary(time); } }

        public CashDrawer(DrawerFlag _flag, int _ownerid, int _managerid, long _time, double _beginningterm = 0.0)
        {
            id = new Random().Next(-1, int.MaxValue);
            billlogs = new List<Bill>();
            operatelogs = new List<OperateLog>();
            status = DrawerOperate.SetStartTerm;
            beginningterm = _beginningterm;
            operatelogs.Add(OperateLog.Create(status, _ownerid, _managerid, 0, _time));
            time = _time;
            ownerid = _ownerid;
            managerid = _managerid;
            flag = _flag;
            status = DrawerOperate.Opened;
            cheques = new List<Cheque>();
            operatelogs.Add(OperateLog.Create(status, _ownerid, _managerid, 0, _time));
            xprint = 0;
            zprint = 0;
            dprint = 0;
        }
        public CashDrawer(int _id, DrawerFlag _flag, int _ownerid, int _managerid, long _time, double _beginningterm = 0.0)
        {
            id = _id;
            billlogs = new List<Bill>();
            operatelogs = new List<OperateLog>();
            status = DrawerOperate.SetStartTerm;
            beginningterm = _beginningterm;
            operatelogs.Add(OperateLog.Create(status, _ownerid, _managerid, 0, _time));
            time = _time;
            ownerid = _ownerid;
            managerid = _managerid;
            empolyeeid = 0;
            lastorderid = 0;
            lastordertime = 0;
            flag = _flag;
            status = DrawerOperate.Opened;
            cheques = new List<Cheque>();
            operatelogs.Add(OperateLog.Create(status, _ownerid, _managerid, 0, _time));
            xprint = 0;
            zprint = 0;
            dprint = 0;
        }
        public CashDrawer(DrawerFlag _flag, int _ownerid, int _managerid, int _empolyeeid, long _time, double _beginningterm = 0.0)
        {
            id = new Random().Next(-1, int.MaxValue);
            billlogs = new List<Bill>();
            operatelogs = new List<OperateLog>();
            status = DrawerOperate.SetStartTerm;
            beginningterm = _beginningterm;
            operatelogs.Add(OperateLog.Create(status, _ownerid, _managerid, _empolyeeid, _time));
            cheques = new List<Cheque>();
            time = _time;
            ownerid = _ownerid;
            flag = _flag;
            managerid = _managerid;
            empolyeeid = _empolyeeid;
            status = DrawerOperate.Opened;
            operatelogs.Add(OperateLog.Create(status, _ownerid, _managerid, _empolyeeid, _time));
            xprint = 0;
            zprint = 0;
            dprint = 0;
            lastorderid = 0;
            lastordertime = 0;
        }
        public CashDrawer(int _id, DrawerFlag _flag, int _ownerid, int _managerid, int _empolyeeid, long _time, double _beginningterm = 0.0)
        {
            id = _id;
            billlogs = new List<Bill>();
            operatelogs = new List<OperateLog>();
            status = DrawerOperate.SetStartTerm;
            beginningterm = _beginningterm;
            operatelogs.Add(OperateLog.Create(status, _ownerid, _managerid, _empolyeeid, _time));
            cheques = new List<Cheque>();
            time = _time;
            ownerid = _ownerid;
            flag = _flag;
            managerid = _managerid;
            empolyeeid = _empolyeeid;
            status = DrawerOperate.Opened;
            operatelogs.Add(OperateLog.Create(status, _ownerid, _managerid, _empolyeeid, _time));
            xprint = 0;
            zprint = 0;
            dprint = 0;
            lastorderid = 0;
            lastordertime = 0;
        }

        public bool Open(long _time, int _managerid, bool useterm = false, double _beginningterm = 0.0)
        {
            bool ReturnedValue = true;
            switch (status)
            {
                case DrawerOperate.Closeed:
                    {
                        managerid = _managerid;
                        if (useterm)
                        {
                            status = DrawerOperate.SetStartTerm;
                            beginningterm = _beginningterm;
                            operatelogs.Add(OperateLog.Create(DrawerOperate.Opened, ownerid, managerid, 0, _time));
                        }
                        status = DrawerOperate.Opened;
                        operatelogs.Add(OperateLog.Create(DrawerOperate.Opened, ownerid, managerid, 0, _time));
                        time = _time;
                        break;
                    }
                default:
                    {
                        ReturnedValue = false;
                        break;
                    }
            }
            return ReturnedValue;
        }
        public bool Close(long _time)
        {
            bool ReturnedValue = true;
            switch (status)
            {
                case DrawerOperate.Signed_In:
                    {
                        ReturnedValue = SignOut(_time);
                        operatelogs.Add(OperateLog.Create(DrawerOperate.Closeed, ownerid, managerid, empolyeeid, _time));
                        status = DrawerOperate.Closeed;
                        managerid = 0;
                        break;
                    }
                case DrawerOperate.Signed_Out:
                    {
                        operatelogs.Add(OperateLog.Create(DrawerOperate.Closeed, ownerid, managerid, empolyeeid, _time));
                        status = DrawerOperate.Closeed;
                        managerid = 0;
                        break;
                    }
                default:
                    {
                        ReturnedValue = false;
                        break;
                    }
            }
            return ReturnedValue;
        }
        public bool SignOut(long _time)
        {
            bool ReturnedValue = true;
            switch (status)
            {
                case DrawerOperate.Signed_In:
                    {
                        operatelogs.Add(OperateLog.Create(DrawerOperate.Signed_Out, ownerid, managerid, empolyeeid, _time));
                        status = DrawerOperate.Signed_Out;
                        empolyeeid = 0;
                        break;
                    }
                default:
                    {
                        ReturnedValue = false;
                        break;
                    }
            }
            return ReturnedValue;
        }
        public bool SignIn(int _employeeid, long _time)
        {
            bool ReturnedValue = true;
            switch (status)
            {
                case DrawerOperate.Signed_Out:
                case DrawerOperate.Opened:
                    {
                        operatelogs.Add(OperateLog.Create(DrawerOperate.Signed_In, ownerid, managerid, _employeeid, _time));
                        status = DrawerOperate.Signed_In;
                        empolyeeid = _employeeid;
                        break;
                    }
                default:
                    {
                        ReturnedValue = false;
                        break;
                    }
            }
            return ReturnedValue;
        }
        public bool AddCheque(Cheque _cheque)
        {
            if (status != DrawerOperate.Signed_In)
                return false;
            lastordertime = _cheque.IOTime;
            cheques.Add(_cheque);
            lastorderid = _cheque.ID;
            PrintLastCheque();
            return true;
        }
        public bool AddBill(long _time, Bill _bill)
        {
            if (status != DrawerOperate.Opened)
                return false;
            billlogs.Add(_bill);
            return billlogs.Contains(_bill);
        }
        public string PrintableReport(DrawerReportMode Mode)
        {
            if (status != DrawerOperate.Opened || status != DrawerOperate.Signed_In)
                return null;
            string Printstr = null;
            switch (Mode)
            {
                case DrawerReportMode.None:
                default:
                    break;
                case DrawerReportMode.X_Print:
                    {
                        xprint++;
                        Printstr = new Reports.DrawerReport(this).TransformText(Mode);
                        break;
                    }
                case DrawerReportMode.Z_Print:
                    {
                        zprint++;
                        Printstr = new Reports.DrawerReport(this).TransformText(Mode);
                        break;
                    }
                case DrawerReportMode.D_Print:
                    {
                        dprint++;
                        Printstr = new Reports.DrawerReport(this).TransformText(Mode);
                        break;
                    }
            }
            return Printstr;
        }
        public string PrintLastCheque()
        {
            if (status != DrawerOperate.Opened || status != DrawerOperate.Signed_In)
                return null;
            return new ChequeReport(cheques.Find(p => p.ID == lastorderid)).TransformText();
        }

        ~CashDrawer()
        {
            id = 0;
            time = 0;
            lastordertime = 0;
            ownerid = 0;
            empolyeeid = 0;
            managerid = 0;
            beginningterm = 0;
            lastorderid = 0;
            xprint = 0;
            zprint = 0;
            dprint = 0;
            flag = DrawerFlag.None;
            status = DrawerOperate.None;
            operatelogs = null;
            billlogs = null;
            cheques = null;
        }

        public static bool operator <(CashDrawer drawer, CashDrawer[] drawers)
        {
            try
            {
                int gmanagerid = 0, rownerid = 0;
                if (drawer != null)
                {
                    gmanagerid = drawer.managerid;
                    rownerid = drawer.ownerid;
                    drawer = null;
                }
                drawer = new CashDrawer(DrawerFlag.None, rownerid, gmanagerid, 0, drawers.Sum(p => p.beginningterm))
                {
                    id = 0,
                    status = DrawerOperate.None,
                    xprint = (byte)drawers.Sum(p => p.xprint),
                    zprint = (byte)drawers.Sum(p => p.xprint),
                    dprint = (byte)drawers.Sum(p => p.xprint),
                    operatelogs = drawers.SelectMany(p => p.operatelogs).ToList(),
                    billlogs = drawers.SelectMany(p => p.billlogs).ToList(),
                    cheques = drawers.SelectMany(p => p.cheques).ToList(),
                };
                drawer.operatelogs = drawers.SelectMany(p => p.operatelogs).ToList();
                return drawer != null;
            }
            catch
            {
                return false;
            }
        }
        public static bool operator >(CashDrawer drawer, CashDrawer[] drawers)
        {
            return false;
        }
        public static bool operator >(CashDrawer drawer, DrawerReport report)
        {
            try
            {
                report = DrawerReport.Create(drawer.id, drawer.actualcurrentbot, drawer.staffmealbot,
                    drawer.cashinbot, drawer.refundbot, drawer.totalsalesbot, drawer.netsales,
                    drawer.billsexpenses, drawer.beginningterm, drawer.ownerid, drawer.empolyeeid,
                    (byte)drawer.flag, drawer.managerid, drawer.cheques.Select(x => x.ID).ToList(),
                    (byte)drawer.status, drawer.operatelogs.Select(x => x.Time).ToList(),
                    drawer.billlogs.Select(x => x.ID).ToList(), drawer.lastordertime, drawer.time);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool operator <(CashDrawer drawer, DrawerReport report)
        {
            return false;
        }
    }
}
