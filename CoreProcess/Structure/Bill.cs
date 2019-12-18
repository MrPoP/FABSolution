using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct Bill
    {
        public static Bill Default = Create();

        public int ID;
        public double TotalValue;
        public int ManagerID;
        public int OwnerID;
        public long Time;
        public List<string> Items;

        public static Bill Create(int _id, double _value, int _ownerid, int _managerid, long _time, List<string> _items)
        {
            return new Bill()
            {
                ID = _id,
                TotalValue = _value,
                ManagerID = _managerid,
                OwnerID = _ownerid,
                Time = _time,
                Items = _items
            };
        }
        public static Bill Create()
        {
            return new Bill()
            {
                ID = 0,
                TotalValue = 0,
                ManagerID = 0,
                OwnerID = 0,
                Time = 0,
                Items = null
            };
        }
    }
}
