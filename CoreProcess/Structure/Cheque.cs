using CoreProcess.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public class Cheque : List<Product>
    {
        private long time = 0;
        private int id = 0, empolyeeid = 0, managerid = 0, ownerid = 0;
        private ChequeStatus status = ChequeStatus.None;
        private OrderType type = OrderType.None;
        private ChequeCashOperator operat = ChequeCashOperator.CashIn;

        public int ID { get { return id; } }
        public int EmpolyeeID { get { return empolyeeid; } }
        public int ManagerID { get { return managerid; } }
        public int OwnerID { get { return this.ownerid; } }
        public ChequeStatus Status { get { return status; } }
        public ChequeCashOperator Operator { get { return operat; } }
        public OrderType Type { get { return type; } }
        public double TotalPrice { get { return this.Sum(p => p.Price); } }
        public DateTime OrderTime { get { return DateTime.FromBinary(time); } }
        public long IOTime { get { return time; } }

        public Cheque(OrderType _type, int empid, int mangid, int _ownerid, long _time)
            :base()
        {
            id = new Random().Next(-1, int.MaxValue);
            status = ChequeStatus.Ordering;
            type = _type;
            time = _time;
            empolyeeid = empid;
            managerid = mangid;
            ownerid = _ownerid;
        }

        public void AddProduct(Product product)
        {
            if (status == ChequeStatus.Done)
                return;
            Add(product);
        }

        public void Remove(int index)
        {
            if (status == ChequeStatus.Done)
                return;
            RemoveAt(index);
        }

        public void SetStatus(ChequeStatus _status)
        {
            status = _status;
        }

        public void SetOperator(ChequeCashOperator _operat)
        {
            if (status == ChequeStatus.Done)
                return;
            operat = _operat;
        }

        public string PrintableReport { get { return new ChequeReport(this).TransformText(); } }

        ~Cheque()
        {
            id = 0;
            time = 0;
            empolyeeid = 0;
            managerid = 0;
            status = ChequeStatus.None;
            type = OrderType.None;
            operat = ChequeCashOperator.CashIn;
        }
    }
}
