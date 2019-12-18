using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.Threading.Generic
{
    public delegate void StackQueueEventHandler<TEventArgs>(object sender, TEventArgs e);
    public delegate void ClipBoardConEventHandler<TEventArgs>(object sender, TEventArgs e);
    public delegate void UnobservedPerlaHandler<TEventArgs>(object sender, TEventArgs e);
}
