using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreThreading.Threading
{
    internal sealed class ThreadPoolPerlaScheduler : PerlaScheduler
    {
        internal ThreadPoolPerlaScheduler()
        {
            int id = base.Id;
        }
        private static readonly ParameterizedThreadStart s_longRunningThreadWork = s =>
        {
            Debug.Assert(s is PerlaItem);
            ((PerlaItem)s).ExecuteEntryUnsafe(threadPoolThread: null);
        };
        protected internal override void QueueTask(PerlaItem item)
        {
            if(item.ExcutionFlag != PerlaExcutionFlag.Single)
            {
                Thread thread = new Thread(s_longRunningThreadWork);
                thread.IsBackground = true;
                thread.Start(item);
            }
        }
        protected override bool TryExecuteTaskInline(PerlaItem item, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued /*&& !ThreadPool.TryPopCustomWorkItem(item)*/)
                return false;
            try
            {
                item.ExecuteEntryUnsafe(threadPoolThread: null);
            }
            finally
            {
                if (taskWasPreviouslyQueued) NotifyWorkItemProgress();
            }
            return true;
        }
        protected internal override bool TryDequeue(PerlaItem item)
        {
            /* return ThreadPool.TryPopCustomWorkItem(item);*/
            return false;
        }
        protected override IEnumerable<PerlaItem> GetScheduledItems()
        {
            return null;/* return FilterTasksFromWorkItems(ThreadPool.GetQueuedWorkItems());*/
        }
        private IEnumerable<PerlaItem> FilterTasksFromWorkItems(IEnumerable<object> tpwItems)
        {
            foreach (object tpwi in tpwItems)
            {
                if (tpwi is PerlaItem)
                {
                    yield return (tpwi as PerlaItem);
                }
            }
        }
        internal override void NotifyWorkItemProgress()
        {
            /*ThreadPool.NotifyWorkItemProgress();*/
        }
        internal override bool RequiresAtomicStartTransition { get { return false; } }
    }
}
