using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace CoreProcess.Structure
{
    public class Department
    {
        private DepartmentFlag flag = DepartmentFlag.None;
        private DepartmentReportFlag reportflag = DepartmentReportFlag.None;
        private string ipaddress = string.Empty;
        private int id = 0, ownerid = 0;
        private int employeeid = 0;
        private List<Cheque> orders = null;

        public DepartmentFlag Flag { get { return this.flag; } }
        public DepartmentReportFlag ReportFlag { get { return this.reportflag; } }
        public IPAddress Address { get { return IPAddress.Parse(this.ipaddress); } }
        public int ID { get { return this.id; } }
        public int OwnerID { get { return this.ownerid; } }
        public int EmployeeID { get { return this.employeeid; } }
        public List<Cheque> Orders { get { return this.orders; } }

        public Department(int _id, int _ownerid, DepartmentFlag _flag, string _ipaddress = "0.0.0.0")
        {
            id = _id;
            ownerid = _ownerid;
            ipaddress = _ipaddress;
            flag = _flag;
            orders = new List<Cheque>();
        }
        public Department(int _ownerid, DepartmentFlag _flag, string _ipaddress = "0.0.0.0")
        {
            id = new Random().Next(-1, int.MaxValue);
            ownerid = _ownerid;
            ipaddress = _ipaddress;
            flag = _flag;
            orders = new List<Cheque>();
        }

        public void Process(Cheque cheque, out List<Exception> thrownexception)
        {
            thrownexception = null;
            lock (this.orders)
            {
                switch (cheque.Status)
                {
                    case ChequeStatus.Confirmed:
                        {
                            switch (cheque.Operator)
                            {
                                case ChequeCashOperator.CashIn:
                                case ChequeCashOperator.StaffMeal:
                                    {
                                        this.orders.Add(cheque);
                                        ProcessReport(out thrownexception);
                                        break;
                                    }
                                default:
                                    break;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }
        private void ProcessReport(out List<Exception> thrownexception)
        {
            thrownexception = new List<Exception>();
            lock (this.orders)
            {
                switch (reportflag)
                {
                    case DepartmentReportFlag.Printer:
                        {
                            foreach (var order in orders)
                            {//build the report body
                                foreach (var product in order.GetProducts(this.flag))
                                {
                                }
                            }
                            orders.Clear();
                            break;
                        }
                    case DepartmentReportFlag.Screen:
                        {
                            foreach (var order in orders)
                            {//build the report body
                                foreach (var product in order.GetProducts(this.flag))
                                {
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }
        public void EndOrder(int index, long time)
        {
            lock (this.orders)
            {
                TimeSpan span = DateTime.FromBinary(time) - this.orders[index].OrderTime;
                this.orders.RemoveAt(index);
                this.ReportEmployee(span);
            }
        }
        private void ReportEmployee(TimeSpan time)
        {//complete this part of the code 
        }

        ~Department()
        {
            flag = DepartmentFlag.None;
            reportflag = DepartmentReportFlag.None;
            ipaddress = string.Empty;
            id = 0;
            employeeid = 0;
            orders = null;
        }

        public static implicit operator DepartmentReport(Department department)
        {
            return DepartmentReport.Create(department.id, department.ownerid, department.employeeid, department.ipaddress,
                (byte)department.flag, (byte)department.reportflag, department.orders.Count);
        }
    }
}
