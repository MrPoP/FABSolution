using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.Threading
{
    public class StackQueueEventArgs : EventArgs
    {
        public readonly StackAction Order;
        public DateTime InvocationTime { get { return DateTime.FromBinary(this.time); } }
        protected readonly long time;
        public readonly object Object;
        protected StackQueueEventArgs(StackAction order, object obj)
        {
            this.Order = order;
            this.time = DateTime.Now.ToBinary();
            this.Object = obj;
        }
        public StackQueueEventArgs()
            : this(StackAction.Pop, null)
        {
        }
        protected StackQueueEventArgs(bool Empty)
        {
            this.Order = StackAction.None;
            this.Object = null;
            this.time = DateTime.Now.ToBinary();
        }
        public StackQueueEventArgs(StackAction order)
            : this(order, null)
        {
        }
    }
}
