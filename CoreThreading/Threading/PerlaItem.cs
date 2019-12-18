using CoreThreading.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreThreading.Threading
{
    [DebuggerDisplay("Id = {Id}, Status = {Status}")]
    public class PerlaItem : IAsyncResult, IDisposable
    {
        [ThreadStatic]
        internal static PerlaItem t_currentItem;
        internal static int s_ItemIdCounter;
        private volatile int m_ItemId;
        private volatile int m_InvokeCount;
        private readonly int MaxInvocationTimes;
        internal ExecutionContext m_capturedContext;
        internal Delegate m_action;
        private object m_stateObject;
        private PerlaScheduler m_ItemScheduler;
        private PerlaExcutionFlag m_excutionFlag;
        private CancellationToken m_cancellationToken;
        internal volatile ManualResetEventSlim m_completionEvent;
        internal volatile PerlaExceptionHolder m_exceptionsHolder;
        internal bool IsDelegateInvoked { get { return m_InvokeCount > 0; } }
        internal PerlaItem m_parent;
        internal bool IsCancellationRequested
        {
            get
            {
                return m_cancellationToken == null || m_cancellationToken.IsCancellationRequested;
            }
        }
        internal static PerlaItem InternalCurrent { get { return t_currentItem; } }
        private PerlaStatus m_status;
        private static Dictionary<int, PerlaItem> s_currentActiveItems = new Dictionary<int, PerlaItem>();
        internal static int NewId()
        {
            int newId;
            do
            {
                newId = Interlocked.Increment(ref s_ItemIdCounter);
            }
            while (newId == 0);
            return newId;
        }
        internal static bool AddToActiveItems(PerlaItem item)
        {
            Debug.Assert(item != null, "Null PerlaItem objects can't be added to the ActiveItems collection");
            LazyInitializer.EnsureInitialized(ref s_currentActiveItems, () => new Dictionary<int, PerlaItem>());
            int itemId = item.Id;
            lock (s_currentActiveItems)
            {
                s_currentActiveItems[itemId] = item;
            }
            return true;
        }
        internal static void RemoveFromActiveItems(PerlaItem item)
        {
            if (s_currentActiveItems == null)
                return;

            int taskId = item.Id;
            lock (s_currentActiveItems)
            {
                s_currentActiveItems.Remove(taskId);
            }
        }
        internal void AddException(object exceptionObject, bool representsCancellation)
        {
            Debug.Assert(exceptionObject != null, "Task.AddException: Expected a non-null exception object");

#if DEBUG
            var eoAsException = exceptionObject as Exception;
            var eoAsEnumerableException = exceptionObject as IEnumerable<Exception>;
            var eoAsEdi = exceptionObject as ExceptionDispatchInfo;
            var eoAsEnumerableEdi = exceptionObject as IEnumerable<ExceptionDispatchInfo>;

            Debug.Assert(
                eoAsException != null || eoAsEnumerableException != null || eoAsEdi != null || eoAsEnumerableEdi != null,
                "Task.AddException: Expected an Exception, ExceptionDispatchInfo, or an IEnumerable<> of one of those");

            var eoAsOce = exceptionObject as OperationCanceledException;

            Debug.Assert(
                !representsCancellation ||
                eoAsOce != null ||
                (eoAsEdi != null && eoAsEdi.SourceException is OperationCanceledException),
                "representsCancellation should be true only if an OCE was provided.");
#endif
            if (m_exceptionsHolder == null)
            {
                PerlaExceptionHolder holder = new PerlaExceptionHolder(this);
                if (Interlocked.CompareExchange(ref m_exceptionsHolder, holder, null) != null)
                {
                    holder.MarkAsHandled(false);
                }
            }

            lock (m_exceptionsHolder)
            {
                m_exceptionsHolder.Add(exceptionObject, representsCancellation);
            }
        }
        internal ExecutionContext CapturedContext
        {
            get
            {
                return m_capturedContext == null ? ExecutionContext.Capture() : m_capturedContext;
            }
            set
            {
                m_capturedContext = value;
            }
        }
        internal ManualResetEventSlim CompletedEvent
        {
            get
            {
                if (m_completionEvent == null)
                {
                    bool wasCompleted = IsCompleted;
                    ManualResetEventSlim newMre = new ManualResetEventSlim(wasCompleted);
                    if (Interlocked.CompareExchange(ref m_completionEvent, newMre, null) != null)
                    {
                        newMre.Dispose();
                    }
                    else if (!wasCompleted && IsCompleted)
                    {
                        newMre.Set();
                    }
                }

                return m_completionEvent;
            }
        }
        public static PerlaPool Factory { get { return new PerlaPool(); } }
        public PerlaStatus Status { get { return this.m_status; } }
        public PerlaExcutionFlag ExcutionFlag { get { return this.m_excutionFlag; } }
        public bool IsCanceled { get { return ContainsFlag(PerlaStatus.Canceled); } }
        public bool IsCompleted { get { return ContainsFlag(PerlaStatus.Completed) || this.m_InvokeCount == this.MaxInvocationTimes; } }
        public bool IsFaulted { get { return ContainsFlag(PerlaStatus.Faulted); } }
        public int Id
        {
            get
            {
                if (m_ItemId == 0)
                {
                    int newId = NewId();
                    Interlocked.CompareExchange(ref m_ItemId, newId, 0);
                }

                return m_ItemId;
            }
        }
        protected PerlaItem(Delegate action, object state, CancellationToken cancellationToken,
            PerlaExcutionFlag excutionFlag, PerlaScheduler ItemScheduler)
        {
            this.m_action = action;
            this.m_stateObject = state;
            this.m_excutionFlag = excutionFlag;
            this.m_ItemScheduler = ItemScheduler;
            this.m_cancellationToken = cancellationToken;
        }
        internal bool ExecuteEntry()
        {
            if (this.m_excutionFlag == PerlaExcutionFlag.Single)
                if (this.IsCompleted)
                    return false;
            if (this.m_excutionFlag == PerlaExcutionFlag.Times)
                if (this.IsCompleted)
                    return false;
            if (this.m_excutionFlag == PerlaExcutionFlag.Period)
                if (this.IsCompleted)
                    return false;
            if (!IsCancellationRequested & !IsCanceled)
            {
                ExecuteWithThreadLocal(ref t_currentItem, null);
            }
            else
            {
                ExecuteEntryCancellationRequestedOrCanceled();
            }
            return true;
        }
        private void ExecuteWithThreadLocal(ref PerlaItem currentItemSlot, Thread threadPoolThread)
        {
            PerlaItem previousItem = currentItemSlot;
            try
            {
                currentItemSlot = this;
                try
                {
                    ExecutionContext ec = CapturedContext;
                    if(ec == null)
                    {
                        InnerInvoke();
                    }
                    else
                    {
                        if (threadPoolThread == null)
                        {
                            ExecutionContext.Run(ec, s_ecCallback, this);
                        }
                        else
                        {
                            ExecutionContext.Run(ec, s_ecCallback, this);
                        }
                    }
                }
                catch (Exception exn)
                {
                    HandleException(exn);
                }
                Finish(true);
            }
            finally
            {
                currentItemSlot = previousItem;
            }
        }
        internal void Finish(bool userDelegateExecute)
        {
            FinishStageTwo();
        }
        private void FinishStageTwo()
        {
            int completionState;
            if (ExceptionRecorded)
            {
                /*completionState = TASK_STATE_FAULTED;
                if (AsyncCausalityTracer.LoggingOn)
                    AsyncCausalityTracer.TraceOperationCompletion(this, AsyncCausalityStatus.Error);

                if (s_asyncDebuggingEnabled)
                    RemoveFromActiveTasks(this);*/
            }
            else if (IsCancellationRequested)
            {
                // We transition into the TASK_STATE_CANCELED final state if the task's CT was signalled for cancellation,
                // and the user delegate acknowledged the cancellation request by throwing an OCE,
                // and the task hasn't otherwise transitioned into faulted state. (TASK_STATE_FAULTED trumps TASK_STATE_CANCELED)
                //
                // If the task threw an OCE without cancellation being requestsed (while the CT not being in signaled state),
                // then we regard it as a regular exception

                /*completionState = TASK_STATE_CANCELED;
                if (AsyncCausalityTracer.LoggingOn)
                    AsyncCausalityTracer.TraceOperationCompletion(this, AsyncCausalityStatus.Canceled);

                if (s_asyncDebuggingEnabled)
                    RemoveFromActiveTasks(this);*/
            }
            else
            {
                /*completionState = TASK_STATE_RAN_TO_COMPLETION;
                if (AsyncCausalityTracer.LoggingOn)
                    AsyncCausalityTracer.TraceOperationCompletion(this, AsyncCausalityStatus.Completed);

                if (s_asyncDebuggingEnabled)
                    RemoveFromActiveTasks(this);*/
            }
            //Interlocked.Exchange(ref m_stateFlags, m_stateFlags | completionState);
            SetCompleted();
            UnregisterCancellationCallback();
            // ready to run continuations and notify parent.
            FinishStageThree();
        }
        internal void FinishContinuations()
        {
            RunContinuations(Status);
        }
        private void RunContinuations(PerlaStatus status)
        {
            switch (status)
            {
#warning need_to_done
            }
        }
        private static readonly ContextCallback s_ecCallback = obj =>
        {
            Debug.Assert(obj is PerlaItem);
            (obj as PerlaItem).InnerInvoke();
        };
        internal virtual void InnerInvoke()
        {
            Debug.Assert(m_action != null, "Null action in InnerInvoke()");
            Action action;
            if (m_action is Action)
            {
                action = (m_action as Action);
                action();
                return;
            }
            Action<object> actionWithState;
            if (m_action is Action<object>)
            {
                actionWithState = (m_action as Action<object>);
                actionWithState(m_stateObject);
                return;
            }
            Debug.Fail("Invalid m_action in Task");
        }
        private void HandleException(Exception unhandledException)
        {
            Debug.Assert(unhandledException != null);
            if (unhandledException is OperationCanceledException && IsCancellationRequested &&
                m_cancellationToken == (unhandledException as OperationCanceledException).CancellationToken)
            {
                SetCancellationAcknowledged();
                AddException((unhandledException as OperationCanceledException), representsCancellation: true);
            }
            else
            {
                AddException(unhandledException);
            }
        }
        internal void ExecuteEntryUnsafe(Thread threadPoolThread)
        {
            ++m_InvokeCount;
            if (!IsCancellationRequested & !IsCanceled)
            {
                ExecuteWithThreadLocal(ref t_currentItem, threadPoolThread);
            }
            else
            {
                ExecuteEntryCancellationRequestedOrCanceled();
            }
        }
        internal void ExecuteEntryCancellationRequestedOrCanceled()
        {
            if (!IsCanceled)
            {
                CancellationCleanupLogic();
            }
        }
        internal void SetCompleted()
        {
            ManualResetEventSlim mres = m_completionEvent;
            if (mres != null) mres.Set();
        }
        internal void CancellationCleanupLogic()
        {
            SetCompleted();
            UnregisterCancellationCallback();
            FinishStageThree();
        }
        internal void FinishStageThree()
        {
            m_action = null;
            m_capturedContext = null;
            FinishContinuations();
        }
        private void SetCancellationAcknowledged()
        {
            //Debug.Assert(this == Task.InternalCurrent, "SetCancellationAcknowledged() should only be called while this is still the current task");
            Debug.Assert(IsCancellationRequested, "SetCancellationAcknowledged() should not be called if the task's CT wasn't signaled");
        }
        internal StrongBox<CancellationTokenRegistration> m_cancellationRegistration;
        internal volatile int m_internalCancellationRequested;
        internal void UnregisterCancellationCallback()
        {
            if (m_cancellationRegistration != null)
            {
                // Harden against ODEs thrown from disposing of the CTR.
                // Since the task has already been put into a final state by the time this
                // is called, all we can do here is suppress the exception.
                try { m_cancellationRegistration.Value.Dispose(); }
                catch (ObjectDisposedException) { }
                m_cancellationRegistration = null;
            }
        }
        internal bool TrySetResult()
        {
            Debug.Assert(m_action == null, "PerlaItem<T>.TrySetResult(): non-null m_action");

            SetCompleted();
            FinishContinuations();
            return true;
        }
        internal void AddException(object exceptionObject)
        {
            Debug.Assert(exceptionObject != null, "Task.AddException: Expected a non-null exception object");
            AddException(exceptionObject, false);
        }
        private AggregateException GetExceptions(bool includeTaskCanceledExceptions)
        {
            Exception canceledException = null;
            if (includeTaskCanceledExceptions && IsCanceled)
            {
                canceledException = new PerlaCanceledException(this);
            }

            if (ExceptionRecorded)
            {
                Debug.Assert(m_exceptionsHolder != null, "ExceptionRecorded should imply this");
                return m_exceptionsHolder.CreateExceptionObject(false, canceledException);
            }
            else if (canceledException != null)
            {
                return new AggregateException(canceledException);
            }
            return null;
        }
        internal ReadOnlyCollection<ExceptionDispatchInfo> GetExceptionDispatchInfos()
        {
            Debug.Assert(IsFaulted && ExceptionRecorded, "Must only be used when the task has faulted with exceptions.");
            return m_exceptionsHolder.GetExceptionDispatchInfos();
        }
        internal bool ExceptionRecorded
        {
            get
            {
                return (m_exceptionsHolder != null) && (m_exceptionsHolder.ContainsFaultList);
            }
        }
        internal ExceptionDispatchInfo GetCancellationExceptionDispatchInfo()
        {
            Debug.Assert(IsCanceled, "Must only be used when the task has canceled.");
            return m_exceptionsHolder.GetCancellationExceptionDispatchInfo(); // may be null
        }
        internal void ThrowIfExceptional(bool includeTaskCanceledExceptions)
        {
            Debug.Assert(IsCompleted, "ThrowIfExceptional(): Expected IsCompleted == true");
            Exception exception = GetExceptions(includeTaskCanceledExceptions);
            if (exception != null)
            {
                UpdateExceptionObservedStatus();
                throw exception;
            }
        }
        internal static void ThrowAsync(Exception exception, SynchronizationContext targetContext)
        {
            var edi = ExceptionDispatchInfo.Capture(exception);
            if (targetContext != null)
            {
                try
                {
                    targetContext.Post(state => ((ExceptionDispatchInfo)state).Throw(), edi);
                    return;
                }
                catch (Exception postException)
                {
                    edi = ExceptionDispatchInfo.Capture(new AggregateException(exception, postException));
                }
            }
            ThreadPool.QueueUserWorkItem(state => ((ExceptionDispatchInfo)state).Throw(), edi);
        }
        internal void UpdateExceptionObservedStatus()
        {
            PerlaItem parent = m_parent;
            if ((parent != null)
                && PerlaItem.InternalCurrent == parent)
            {
#warning NeedToWorkMore
            }
        }
        protected bool ContainsFlag(PerlaStatus flag)
        {
            return this.m_status.HasFlag(flag);
        }
        protected void SetFlag(PerlaStatus flag)
        {
            this.m_status = flag;
        }
        public object AsyncState
        {
            get { return m_stateObject; }
        }
        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get {
                bool isDisposed = ContainsFlag(PerlaStatus.Disposed);
                if (isDisposed)
                    throw new ObjectDisposedException("AsyncWaitHandle");
                return CompletedEvent.WaitHandle;
            }
        }
        internal PerlaScheduler ExecutingScheduler { get { return m_ItemScheduler; } }
        public bool CompletedSynchronously
        {
            get { return false; }
        }
        internal virtual void ExecuteFromThreadPool(Thread threadPoolThread) { ExecuteEntryUnsafe(threadPoolThread); }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsCompleted)
                {
                    throw new InvalidOperationException("Item Dispose NotCompleted");
                }
                this.m_status = PerlaStatus.Disposed;
                ManualResetEventSlim ev = m_completionEvent;
                if(ev != null)
                {
                    m_completionEvent = null;
                    if (!ev.IsSet)
                        ev.Set();
                    ev.Dispose();
                }
            }
        }
    }
}
