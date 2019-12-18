using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public class Transference : Dictionary<string, RawItem>
    {
        private int id = 0, senderid = 0, recipientid = 0, smanagerid = 0, semployeeid = 0, rmanagerid = 0;
        private InventoryTransferenceFlag flag = InventoryTransferenceFlag.None;
        private DateTime reportdate;

        public int ID { get { return this.id; } }
        public int SenderID { get { return this.senderid; } }
        public int RecipientID { get { return this.recipientid; } }
        public int SManagerID { get { return this.smanagerid; } }
        public int SEmployeeID { get { return this.semployeeid; } }
        public int RManagerID { get { return this.rmanagerid; } }
        public InventoryTransferenceFlag Flag { get { return this.flag; } }
        public DateTime ReportDate { get { return this.reportdate; } }
        public RawItem[] Items { get { return Values.ToArray(); } }

        public Transference(int _senderid, int _smanagerid, int _semployeeid)
            : base()
        {
            id = new Random().Next(-1, int.MaxValue);
            senderid = _senderid;
            smanagerid = _smanagerid;
            semployeeid = _semployeeid;
        }
        public Transference(int _id, int _recipientid, int _rmanagerid, int _senderid, int _smanagerid, int _semployeeid)
            :base()
        {
            id = _id;
            senderid = _senderid;
            smanagerid = _smanagerid;
            semployeeid = _semployeeid;
            recipientid = _recipientid;
            rmanagerid = _rmanagerid;
            flag = InventoryTransferenceFlag.TransferenceGrowing;
        }

        public void SetDestination(int _recipientid, int _rmanagerid)
        {
            flag = InventoryTransferenceFlag.TransferenceShortage;
            recipientid = _recipientid;
            rmanagerid = _rmanagerid;
        }
        public void AddItem(params RawItem[] items)
        {
            foreach (var item in items)
                Add(item.Name, item);
        }
        public void SetDate(DateTime time)
        {
            reportdate = time;
        }

        ~Transference()
        {
            flag = InventoryTransferenceFlag.None;
            id = 0;
            senderid = 0;
            recipientid = 0;
            smanagerid = 0;
            semployeeid = 0;
            rmanagerid = 0;
        }
    }
}
