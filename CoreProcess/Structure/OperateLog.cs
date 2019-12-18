using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct OperateLog
    {
        public static OperateLog Default = Create();

        public DrawerOperate Status;
        public long Time;
        public int OwnerId;
        public int ManagerId;
        public int EmpolyeeId;

        public static OperateLog Create()
        {
            return new OperateLog()
            {
                Status = DrawerOperate.None,
                Time = 0,
                OwnerId = 0,
                ManagerId = 0,
                EmpolyeeId = 0
            };
        }
        public static OperateLog Create(DrawerOperate mode, int _ownerid, int _managerid, int _employeeid, long time)
        {
            return new OperateLog()
            {
                Status = mode,
                Time = time,
                OwnerId = _ownerid,
                ManagerId = _managerid,
                EmpolyeeId = _employeeid
            };
        }
    }
}
