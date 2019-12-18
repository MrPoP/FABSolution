using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreThreading.Threading
{
    public class PerlaPool
    {

        protected CancellationTokenSource cancelsource;
        public CancellationToken CancelToken { get { return this.cancelsource.Token; } }
        protected readonly TaskScheduler Scheduler;
        private volatile int Handlers = 0;
        private readonly int MaxHandlers;
        private object syncRoot;
        public PerlaPool()
            : this(64)
        {
        }
        public PerlaPool(int MaxPoolThreads)
        {
            this.cancelsource = new CancellationTokenSource();
            this.Scheduler = TaskScheduler.Current;
            this.MaxHandlers = MaxPoolThreads;
            this.syncRoot = new object();
        }
    }
}
