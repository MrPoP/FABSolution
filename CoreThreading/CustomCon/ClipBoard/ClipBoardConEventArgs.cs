using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.CustomCon
{
    public class ClipBoardConEventArgs : EventArgs
    {
        public readonly object DataContent;
        public readonly ClipBoardOPType OPType;
        public ClipBoardConEventArgs(ClipBoardOPType opType, object obj)
        {
            this.OPType = opType;
            this.DataContent = obj;
        }
    }
}
