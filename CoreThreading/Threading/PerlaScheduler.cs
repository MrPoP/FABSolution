using CoreThreading.Collections.Generic;
using CoreThreading.Threading.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreThreading.Threading
{
    public abstract class PerlaScheduler
    {
        public static UnobservedPerlaHandler<UnobservedPerlaExceptionEventArgs> UnobservedPerlaException;
        internal static void PublishUnobservedPerlaException(object sender, UnobservedPerlaExceptionEventArgs ueea)
        {
            UnobservedPerlaException.Invoke(sender, ueea);
        }
        protected internal abstract void QueueTask(PerlaItem item);
        protected abstract bool TryExecuteTaskInline(PerlaItem item, bool taskWasPreviouslyQueued);
        protected abstract IEnumerable<PerlaItem> GetScheduledItems();
        public virtual int MaximumConcurrencyLevel { get { return int.MaxValue; } }
        internal bool TryRunInline(PerlaItem item, bool taskWasPreviouslyQueued)
        {
            PerlaScheduler ets = item.ExecutingScheduler;
            if (ets != this && ets != null) return ets.TryRunInline(item, taskWasPreviouslyQueued);
            if ((ets == null) ||
                (item.m_action == null) ||
                item.IsDelegateInvoked ||
                item.IsCanceled)
            {
                return false;
            }
            bool inlined = TryExecuteTaskInline(item, taskWasPreviouslyQueued);
            if (inlined && !(item.IsDelegateInvoked || item.IsCanceled))
            {
                throw new InvalidOperationException("PerlaScheduler_InconsistentStateAfterTryExecuteTaskInline");
            }
            return inlined;
        }
        protected internal virtual bool TryDequeue(PerlaItem item)
        {
            return false;
        }
        internal virtual void NotifyWorkItemProgress()
        {
        }
        internal virtual bool RequiresAtomicStartTransition { get { return true; } }
        private static readonly PerlaScheduler s_defaultPerlaScheduler = new ThreadPoolPerlaScheduler();
        public static PerlaScheduler Default { get { return s_defaultPerlaScheduler; } }
        public static PerlaScheduler Current { get { return InternalCurrent == null ? Default : InternalCurrent; } }
        internal void InternalQueueTask(PerlaItem task)
        {
            Debug.Assert(task != null);
            this.QueueTask(task);
        }
        internal static int s_perlaSchedulerIdCounter;
        private volatile int m_perlaSchedulerId;
        internal static PerlaScheduler InternalCurrent
        {
            get
            {
                PerlaItem currentTask = PerlaItem.InternalCurrent;
                return (currentTask != null) ? currentTask.ExecutingScheduler : null;
            }
        }
        public int Id
        {
            get
            {
                if (m_perlaSchedulerId == 0)
                {
                    int newId;
                    do
                    {
                        newId = Interlocked.Increment(ref s_perlaSchedulerIdCounter);
                    } while (newId == 0);

                    Interlocked.CompareExchange(ref m_perlaSchedulerId, newId, 0);
                }

                return m_perlaSchedulerId;
            }
        }
        protected bool TryExecute(PerlaItem item)
        {
            if (item.ExecutingScheduler != this)
            {
                throw new InvalidOperationException("PerlaScheduler ExecuteMethod WrongScheduler");
            }

            return item.ExecuteEntry();
        }
    }
    internal sealed class SynchronizationContextPerlaScheduler : PerlaScheduler
    {
        private readonly SynchronizationContext m_synchronizationContext;
        internal SynchronizationContextPerlaScheduler()
        {
            Debug.Assert(SynchronizationContext.Current == null, "Current SynchronizationContext Null Exception");
            m_synchronizationContext = SynchronizationContext.Current;
        }
        protected override bool TryExecuteTaskInline(PerlaItem item, bool taskWasPreviouslyQueued)
        {
            if (SynchronizationContext.Current == m_synchronizationContext)
            {
                return TryExecute(item);
            }
            else
            {
                return false;
            }
        }
        protected internal override void QueueTask(PerlaItem item)
        {
            m_synchronizationContext.Post(s_postCallback, (object)item);
        }
        protected override IEnumerable<PerlaItem> GetScheduledItems()
        {
            return null;
        }
        private static readonly SendOrPostCallback s_postCallback = s =>
        {
            Debug.Assert(s is PerlaItem);
            ((PerlaItem)s).ExecuteEntry();
        };
        public override int MaximumConcurrencyLevel { get { return 1; } }
    }
}
