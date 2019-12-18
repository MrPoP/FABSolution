using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading
{
    public class UnobservedPerlaExceptionEventArgs : EventArgs
    {
        public UnobservedPerlaExceptionEventArgs(AggregateException exception)
        {
            this.Exception = exception;
            this.Observed = false;
        }

        public readonly AggregateException Exception;
        public bool Observed { get; private set; }

        public void SetObserved() { this.Observed = true; }
    }
}
