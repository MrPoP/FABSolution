using CoreThreading.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security;
using System.Threading;

namespace CoreThreading.Collections.Generic
{
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class StackQueue<T> : IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
    {
        private object syncRoot;
        private Stack<T> bag;
        private int count;
        private readonly StackQueueEventHandler onEnqueue;
        private readonly StackQueueEventHandler onDequeue;
        public int Count { get { return this.count; } }
        private readonly int Capacity;
        private bool Capable { get { return this.Capacity != -1; } }
        public StackQueue()
            :this(null, -1)
        { }
        public StackQueue(IEnumerable<T> collection)
            : this(collection, -1)
        { }
        public StackQueue(int capacity)
            : this(null, capacity)
        {
        }
        [SecuritySafeCritical]
        protected StackQueue(IEnumerable<T> collection, int capacity)
        {
            this.syncRoot = new object();
            this.bag = new Stack<T>();
            this.Capacity = capacity;
            this.count = 0;
            if (collection != null)
                collection.ToList().ForEach(p => this.Enqueue(p));
            this.onEnqueue = new StackQueueEventHandler((sender, arg) => { if (sender == this && arg == StackQueueEventArgs.Empty) { Interlocked.Increment(ref this.count); } });
            this.onDequeue = new StackQueueEventHandler((sender, arg) => { if (sender == this && arg == StackQueueEventArgs.Empty) { Interlocked.Decrement(ref this.count); } });
        }
        [SecuritySafeCritical]
        public void Enqueue(T element)
        {
            lock(this.syncRoot)
            {
                if (Capable)
                    Contract.Assert(this.Capacity <= this.count, "Reached the maximum capacity for the collection.");
                this.bag.Push(element);
                this.onEnqueue.Invoke(this, (StackQueueEventArgs)StackQueueEventArgs.Empty);
            }
        }
        public T Dequeue()
        {
            T element = default(T);
            this.Dequeue(StackOrder.Peek, out element);
            return element;
        }
        public T ReversedDequeue()
        {
            T element = default(T);
            this.Dequeue(StackOrder.Pop, out element);
            return element;
        }
        [SecuritySafeCritical]
        protected bool Dequeue(StackOrder order, out T element)
        {
            if (this.Count == 0)
            {
                element = default(T);
                return false;
            }
            lock(this.syncRoot)
            {
                switch(order)
                {
                    case StackOrder.Peek:
                        {
                            element = this.bag.Peek();
                            break;
                        }
                    default:
                    case StackOrder.Pop:
                        {
                            element = this.bag.Pop();
                            break;
                        }
                }
                this.onDequeue.Invoke(this, (StackQueueEventArgs)StackQueueEventArgs.Empty);
                return element.Equals(default(T)) == false;
            }
        }
        public void Clear()
        {
            lock(this.syncRoot)
            {
                this.count = 0;
                this.bag.Clear();
                this.bag.TrimExcess();
            }
        }
        public void Dispose()
        {
            Clear();
            this.bag = null;
            this.syncRoot = null;
            this.count = 0;
        }
        public void TrimExcess()
        {
            lock(this.syncRoot)
            {
                Contract.Assert(this.count > 0, "Need to clear the queue before trimming memory.");
                this.bag.TrimExcess();
            }
        }
        public T[] ToArray()
        {
            lock (this.syncRoot)
            {
                return this.bag.ToArray();
            }
        }
        public bool Contains(T item)
        {
            lock(this.syncRoot)
            {
                return this.bag.Contains(item);
            }
        }
        ~StackQueue()
        {
            Clear();
            this.bag = null;
            this.syncRoot = null;
            this.count = 0;
        }
        public bool IsSynchronized
        {
            get { return true; }
        }
        public object SyncRoot
        {
            get { return this.syncRoot; }
        }
        public IEnumerator GetEnumerator()
        {
            lock(this.syncRoot)
            {
                return this.bag.GetEnumerator();
            }
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            lock (this.syncRoot)
            {
                return this.bag.GetEnumerator();
            }
        }
        public void CopyTo(Array array, int index)
        {
            lock (this.syncRoot)
            {
                var values = this.bag.ToArray();
                array.SetValue(this.bag.ToArray(), index);
            }
        }
    }
}
