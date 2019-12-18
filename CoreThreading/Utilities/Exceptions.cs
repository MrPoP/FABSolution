using CoreThreading.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreThreading.Utilities
{
    public class PerlaException : Exception
    {
        public PerlaException()
            :base()
        { }
        public PerlaException(string msg)
            :base(msg)
        { }
        public PerlaException(string msg, Exception exception)
            : base(msg, exception)
        { }
        public PerlaException(string msg, params object[] args)
            : base(string.Format(msg, args))
        { }
    }
    [Serializable]
    public class PerlaCanceledException : OperationCanceledException
    {
        [NonSerialized]
        private readonly PerlaItem _canceledTask;
        public PerlaItem Item { get { return _canceledTask; } }
        public PerlaCanceledException()
            : base()
        { }
        public PerlaCanceledException(PerlaItem item)
            : base()
        {
            this._canceledTask = item;
        }
        public PerlaCanceledException(string msg)
            : base(msg)
        { }
        public PerlaCanceledException(string msg, Exception exception, CancellationToken token)
            : base(msg, exception, token)
        { }
        public PerlaCanceledException(string msg, params object[] args)
            : base(string.Format(msg, args))
        { }
        protected PerlaCanceledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
