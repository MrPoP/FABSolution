using CoreThreading.CustomCon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.Threading
{
    public delegate void StackQueueEventHandler(object sender, StackQueueEventArgs e);
    public delegate void ClipBoardConEventHandler(object sender, ClipBoardConEventArgs e);
}
