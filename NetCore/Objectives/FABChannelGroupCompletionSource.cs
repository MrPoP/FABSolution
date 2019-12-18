using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABChannelGroupCompletionSource : TaskCompletionSource<int>, IFABChannelGroupTaskCompletionSource, IEnumerator<Task>, IDisposable, IEnumerator
    {
        readonly Dictionary<IFABChannel, Task> futures;
        int failureCount;
        int successCount;

        public FABChannelGroupCompletionSource(IFABChannelGroup sgroup, Dictionary<IFABChannel, Task> futures /*, IEventExecutor executor*/)
            : this(sgroup, futures /*,executor*/, null)
        {
        }

        public FABChannelGroupCompletionSource(IFABChannelGroup sgroup, Dictionary<IFABChannel, Task> futures /*, IEventExecutor executor*/, object state)
            : base(state)
        {
            Contract.Requires(sgroup != null);
            Contract.Requires(futures != null);

            this.groub = sgroup;
            this.futures = new Dictionary<IFABChannel, Task>();
            foreach (KeyValuePair<IFABChannel, Task> pair in futures)
            {
                this.futures.Add(pair.Key, pair.Value);
                pair.Value.ContinueWith(x =>
                {
                    bool success = x.Status == TaskStatus.RanToCompletion;
                    bool callSetDone;
                    lock (this)
                    {
                        if (success)
                        {
                            this.successCount++;
                        }
                        else
                        {
                            this.failureCount++;
                        }

                        callSetDone = this.successCount + this.failureCount == this.futures.Count;
                        Contract.Assert(this.successCount + this.failureCount <= this.futures.Count);
                    }

                    if (callSetDone)
                    {
                        if (this.failureCount > 0)
                        {
                            var failed = new List<KeyValuePair<IFABChannel, Exception>>();
                            foreach (KeyValuePair<IFABChannel, Task> ft in this.futures)
                            {
                                IFABChannel c = ft.Key;
                                Task f = ft.Value;
                                if (f.IsFaulted || f.IsCanceled)
                                {
                                    if (f.Exception != null)
                                    {
                                        failed.Add(new KeyValuePair<IFABChannel, Exception>(c, f.Exception.InnerException));
                                    }
                                }
                            }
                            this.TrySetException(new FABChannelGroupException(failed));
                        }
                        else
                        {
                            this.TrySetResult(0);
                        }
                    }
                });
            }

            // Done on arrival?
            if (futures.Count == 0)
            {
                this.TrySetResult(0);
            }
        }
        readonly IFABChannelGroup groub;
        public IFABChannelGroup Group { get { return groub == null ? null : groub; } }

        public Task Find(IFABChannel channel) { return this.futures[channel]; }

        public bool IsPartialSucess()
        {
            lock (this)
            {
                return this.successCount != 0 && this.successCount != this.futures.Count;
            }
        }

        public bool IsSucess() { return this.Task.IsCompleted && !this.Task.IsFaulted && !this.Task.IsCanceled; }

        public bool IsPartialFailure()
        {
            lock (this)
            {
                return this.failureCount != 0 && this.failureCount != this.futures.Count;
            }
        }

        public FABChannelGroupException Cause { get { return (FABChannelGroupException)this.Task.Exception.InnerException; } }

        public Task Current { get { return this.futures.Values.GetEnumerator().Current; } }

        public void Dispose() { this.futures.Values.GetEnumerator().Dispose(); }

        object IEnumerator.Current { get { return this.futures.Values.GetEnumerator().Current; } }

        public bool MoveNext() { return this.futures.Values.GetEnumerator().MoveNext(); }

        public void Reset() { ((IEnumerator)this.futures.Values.GetEnumerator()).Reset(); }
    }
}
