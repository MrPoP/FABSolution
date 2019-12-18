using DotNetty.Buffers;
using DotNetty.Common;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using NetCore.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetCore
{
    public sealed class FABChannelOutboundBuffer
    {
        static readonly ThreadLocalByteBufferList NioBuffers = new ThreadLocalByteBufferList();
        readonly IFABChannel channel;
        int flushed;
        long nioBufferSize;
        bool inFail;
        long totalPendingSize;
        volatile int unwritable;
        Entry flushedEntry;
        Entry unflushedEntry;
        Entry tailEntry;
        internal FABChannelOutboundBuffer(IFABChannel channel)
        {
            this.channel = channel;
        }
        public void AddMessage(object msg, int size, TaskCompletionSource promise)
        {
            Entry entry = Entry.NewInstance(msg, size, promise);
            if (this.tailEntry == null)
            {
                this.flushedEntry = null;
                this.tailEntry = entry;
            }
            else
            {
                Entry tail = this.tailEntry;
                tail.Next = entry;
                this.tailEntry = entry;
            }
            if (this.unflushedEntry == null)
            {
                this.unflushedEntry = entry;
            }

            // increment pending bytes after adding message to the unflushed arrays.
            // See https://github.com/netty/netty/issues/1619
            this.IncrementPendingOutboundBytes(size, false);
        }
        public void AddFlush()
        {
            // There is no need to process all entries if there was already a flush before and no new messages
            // where added in the meantime.
            //
            // See https://github.com/netty/netty/issues/2577
            Entry entry = this.unflushedEntry;
            if (entry != null)
            {
                if (this.flushedEntry == null)
                {
                    // there is no flushedEntry yet, so start with the entry
                    this.flushedEntry = entry;
                }
                do
                {
                    this.flushed++;
                    if (!entry.Promise.SetUncancellable())
                    {
                        // Was cancelled so make sure we free up memory and notify about the freed bytes
                        int pending = entry.Cancel();
                        this.DecrementPendingOutboundBytes(pending, false, true);
                    }
                    entry = entry.Next;
                }
                while (entry != null);

                // All flushed so reset unflushedEntry
                this.unflushedEntry = null;
            }
        }
        internal void IncrementPendingOutboundBytes(long size) { this.IncrementPendingOutboundBytes(size, true); }
        void IncrementPendingOutboundBytes(long size, bool invokeLater)
        {
            if (size == 0)
            {
                return;
            }

            long newWriteBufferSize = Interlocked.Add(ref this.totalPendingSize, size);
            if (newWriteBufferSize >= this.channel.Configuration.WriteBufferHighWaterMark)
            {
                this.SetUnwritable(invokeLater);
            }
        }
        internal void DecrementPendingOutboundBytes(long size) { this.DecrementPendingOutboundBytes(size, true, true); }
        void DecrementPendingOutboundBytes(long size, bool invokeLater, bool notifyWritability)
        {
            if (size == 0)
            {
                return;
            }

            long newWriteBufferSize = Interlocked.Add(ref this.totalPendingSize, -size);
            if (notifyWritability && (newWriteBufferSize == 0
                || newWriteBufferSize <= this.channel.Configuration.WriteBufferLowWaterMark))
            {
                this.SetWritable(invokeLater);
            }
        }
        public object Current { get { return this.flushedEntry != null ? this.flushedEntry.Message : null; } }
        public void Progress(long amount)
        {
            // todo: support progress report?
            //Entry e = this.flushedEntry;
            //Contract.Assert(e != null);
            //var p = e.promise;
            //if (p is ChannelProgressivePromise)
            //{
            //    long progress = e.progress + amount;
            //    e.progress = progress;
            //    ((ChannelProgressivePromise)p).tryProgress(progress, e.Total);
            //}
        }
        public bool Remove()
        {
            Entry e = this.flushedEntry;
            if (e == null)
            {
                this.ClearNioBuffers();
                return false;
            }
            object msg = e.Message;

            TaskCompletionSource promise = e.Promise;
            int size = e.PendingSize;

            this.RemoveEntry(e);

            if (!e.Cancelled)
            {
                // only release message, notify and decrement if it was not canceled before.
                ReferenceCountUtil.SafeRelease(msg);
                SafeSuccess(promise);
                this.DecrementPendingOutboundBytes(size, false, true);
            }

            // recycle the entry
            e.Recycle();

            return true;
        }
        public bool Remove(Exception cause) { return this.Remove0(cause, true); }
        bool Remove0(Exception cause, bool notifyWritability)
        {
            Entry e = this.flushedEntry;
            if (e == null)
            {
                this.ClearNioBuffers();
                return false;
            }
            object msg = e.Message;

            TaskCompletionSource promise = e.Promise;
            int size = e.PendingSize;

            this.RemoveEntry(e);

            if (!e.Cancelled)
            {
                // only release message, fail and decrement if it was not canceled before.
                ReferenceCountUtil.SafeRelease(msg);
                SafeFail(promise, cause);
                this.DecrementPendingOutboundBytes(size, false, notifyWritability);
            }

            // recycle the entry
            e.Recycle();

            return true;
        }
        void RemoveEntry(Entry e)
        {
            if (--this.flushed == 0)
            {
                // processed everything
                this.flushedEntry = null;
                if (e == this.tailEntry)
                {
                    this.tailEntry = null;
                    this.unflushedEntry = null;
                }
            }
            else
            {
                this.flushedEntry = e.Next;
            }
        }
        public void RemoveBytes(long writtenBytes)
        {
            while (true)
            {
                IByteBuffer buf;
                object msg = this.Current;
                if (!(msg is IByteBuffer))
                {
                    Contract.Assert(writtenBytes == 0);
                    break;
                }
                buf = (msg as IByteBuffer);
                int readerIndex = buf.ReaderIndex;
                int readableBytes = buf.WriterIndex - readerIndex;

                if (readableBytes <= writtenBytes)
                {
                    if (writtenBytes != 0)
                    {
                        this.Progress(readableBytes);
                        writtenBytes -= readableBytes;
                    }
                    this.Remove();
                }
                else
                {
                    // readableBytes > writtenBytes
                    if (writtenBytes != 0)
                    {
                        //Invalid nio buffer cache for partial writen, see https://github.com/Azure/DotNetty/issues/422
                        this.flushedEntry.Buffer = new ArraySegment<byte>();
                        this.flushedEntry.Buffers = null;

                        buf.SetReaderIndex(readerIndex + (int)writtenBytes);
                        this.Progress(writtenBytes);
                    }
                    break;
                }
            }
            this.ClearNioBuffers();
        }
        void ClearNioBuffers() { NioBuffers.Value.Clear(); }
        public List<ArraySegment<byte>> GetSharedBufferList() { return this.GetSharedBufferList(int.MaxValue, int.MaxValue); }
        public List<ArraySegment<byte>> GetSharedBufferList(int maxCount, long maxBytes)
        {
            Debug.Assert(maxCount > 0);
            Debug.Assert(maxBytes > 0);

            long ioBufferSize = 0;
            int nioBufferCount = 0;
            InternalThreadLocalMap threadLocalMap = InternalThreadLocalMap.Get();
            List<ArraySegment<byte>> nioBuffers = NioBuffers.Get(threadLocalMap);
            Entry entry = this.flushedEntry;
            while (this.IsFlushedEntry(entry) && entry.Message is IByteBuffer)
            {
                if (!entry.Cancelled)
                {
                    var buf = (IByteBuffer)entry.Message;
                    int readerIndex = buf.ReaderIndex;
                    int readableBytes = buf.WriterIndex - readerIndex;

                    if (readableBytes > 0)
                    {
                        if (maxBytes - readableBytes < ioBufferSize && nioBufferCount != 0)
                        {
                            // If the nioBufferSize + readableBytes will overflow an Integer we stop populate the
                            // ByteBuffer array. This is done as bsd/osx don't allow to write more bytes then
                            // Integer.MAX_VALUE with one writev(...) call and so will return 'EINVAL', which will
                            // raise an IOException. On Linux it may work depending on the
                            // architecture and kernel but to be safe we also enforce the limit here.
                            // This said writing more the Integer.MAX_VALUE is not a good idea anyway.
                            //
                            // See also:
                            // - https://www.freebsd.org/cgi/man.cgi?query=write&sektion=2
                            // - http://linux.die.net/man/2/writev
                            break;
                        }
                        ioBufferSize += readableBytes;
                        int count = entry.Count;
                        if (count == -1)
                        {
                            entry.Count = count = buf.IoBufferCount;
                        }
                        if (count == 1)
                        {
                            ArraySegment<byte> nioBuf = entry.Buffer;
                            if (nioBuf.Array == null)
                            {
                                // cache ByteBuffer as it may need to create a new ByteBuffer instance if its a
                                // derived buffer
                                entry.Buffer = nioBuf = buf.GetIoBuffer(readerIndex, readableBytes);
                            }
                            nioBuffers.Add(nioBuf);
                            nioBufferCount++;
                        }
                        else
                        {
                            ArraySegment<byte>[] nioBufs = entry.Buffers;
                            if (nioBufs == null)
                            {
                                // cached ByteBuffers as they may be expensive to create in terms
                                // of Object allocation
                                entry.Buffers = nioBufs = buf.GetIoBuffers();
                            }
                            for (int i = 0; i < nioBufs.Length && nioBufferCount < maxCount; i++)
                            {
                                ArraySegment<byte> nioBuf = nioBufs[i];
                                if (nioBuf.Array == null)
                                {
                                    break;
                                }
                                else if (nioBuf.Count == 0)
                                {
                                    continue;
                                }
                                nioBuffers.Add(nioBuf);
                                nioBufferCount++;
                            }
                        }
                        if (nioBufferCount == maxCount)
                        {
                            break;
                        }
                    }
                }
                entry = entry.Next;
            }
            this.nioBufferSize = ioBufferSize;

            return nioBuffers;
        }
        public long NioBufferSize { get { return this.nioBufferSize; } }
        public bool IsWritable { get { return this.unwritable == 0; } }
        public bool GetUserDefinedWritability(int index) { return (this.unwritable & WritabilityMask(index)) == 0; }
        public void SetUserDefinedWritability(int index, bool writable)
        {
            if (writable)
            {
                this.SetUserDefinedWritability(index);
            }
            else
            {
                this.ClearUserDefinedWritability(index);
            }
        }
        void SetUserDefinedWritability(int index)
        {
            int mask = ~WritabilityMask(index);
            while (true)
            {
                int oldValue = this.unwritable;
                int newValue = oldValue & mask;
                if (Interlocked.CompareExchange(ref this.unwritable, newValue, oldValue) == oldValue)
                {
                    if (oldValue != 0 && newValue == 0)
                    {
                        this.FireChannelWritabilityChanged(true);
                    }
                    break;
                }
            }
        }
        void ClearUserDefinedWritability(int index)
        {
            int mask = WritabilityMask(index);
            while (true)
            {
                int oldValue = this.unwritable;
                int newValue = oldValue | mask;
                if (Interlocked.CompareExchange(ref this.unwritable, newValue, oldValue) == oldValue)
                {
                    if (oldValue == 0 && newValue != 0)
                    {
                        this.FireChannelWritabilityChanged(true);
                    }
                    break;
                }
            }
        }
        static int WritabilityMask(int index)
        {
            if (index < 1 || index > 31)
            {
                throw new InvalidOperationException("index: " + index + " (expected: 1~31)");
            }
            return 1 << index;
        }
        void SetWritable(bool invokeLater)
        {
            while (true)
            {
                int oldValue = this.unwritable;
                int newValue = oldValue & ~1;
                if (Interlocked.CompareExchange(ref this.unwritable, newValue, oldValue) == oldValue)
                {
                    if (oldValue != 0 && newValue == 0)
                    {
                        this.FireChannelWritabilityChanged(invokeLater);
                    }
                    break;
                }
            }
        }
        void SetUnwritable(bool invokeLater)
        {
            while (true)
            {
                int oldValue = this.unwritable;
                int newValue = oldValue | 1;
                if (Interlocked.CompareExchange(ref this.unwritable, newValue, oldValue) == oldValue)
                {
                    if (oldValue == 0 && newValue != 0)
                    {
                        this.FireChannelWritabilityChanged(invokeLater);
                    }
                    break;
                }
            }
        }
        void FireChannelWritabilityChanged(bool invokeLater)
        {
            IChannelPipeline pipeline = this.channel.Pipeline;
            if (invokeLater)
            {
                this.channel.EventLoop.Execute(p => ((IChannelPipeline)p).FireChannelWritabilityChanged(), pipeline);
            }
            else
            {
                pipeline.FireChannelWritabilityChanged();
            }
        }
        public int Size { get { return this.flushed; } }
        public bool IsEmpty { get { return this.flushed == 0; } }
        public void FailFlushed(Exception cause, bool notify)
        {
            // Make sure that this method does not reenter.  A listener added to the current promise can be notified by the
            // current thread in the tryFailure() call of the loop below, and the listener can trigger another fail() call
            // indirectly (usually by closing the channel.)
            //
            // See https://github.com/netty/netty/issues/1501
            if (this.inFail)
            {
                return;
            }

            try
            {
                this.inFail = true;
                for (; ; )
                {
                    if (!this.Remove0(cause, notify))
                    {
                        break;
                    }
                }
            }
            finally
            {
                this.inFail = false;
            }
        }
        sealed class CloseChannelTask : IRunnable
        {
            readonly FABChannelOutboundBuffer buf;
            readonly Exception cause;
            readonly bool allowChannelOpen;

            public CloseChannelTask(FABChannelOutboundBuffer buf, Exception cause, bool allowChannelOpen)
            {
                this.buf = buf;
                this.cause = cause;
                this.allowChannelOpen = allowChannelOpen;
            }

            public void Run() { this.buf.Close(this.cause, this.allowChannelOpen); }
        }
        internal void Close(Exception cause, bool allowChannelOpen)
        {
            if (this.inFail)
            {
                this.channel.EventLoop.Execute(new CloseChannelTask(this, cause, allowChannelOpen));
                return;
            }

            this.inFail = true;

            if (!allowChannelOpen && this.channel.Open)
            {
                throw new InvalidOperationException("close() must be invoked after the channel is closed.");
            }

            if (!this.IsEmpty)
            {
                throw new InvalidOperationException("close() must be invoked after all flushed writes are handled.");
            }

            // Release all unflushed messages.
            try
            {
                Entry e = this.unflushedEntry;
                while (e != null)
                {
                    // Just decrease; do not trigger any events via DecrementPendingOutboundBytes()
                    int size = e.PendingSize;
                    Interlocked.Add(ref this.totalPendingSize, -size);

                    if (!e.Cancelled)
                    {
                        ReferenceCountUtil.SafeRelease(e.Message);
                        SafeFail(e.Promise, cause);
                    }
                    e = e.RecycleAndGetNext();
                }
            }
            finally
            {
                this.inFail = false;
            }
            this.ClearNioBuffers();
        }
        internal void Close(ClosedChannelException cause) { this.Close(cause, false); }

        static void SafeSuccess(TaskCompletionSource promise)
        {
            // TODO:ChannelPromise
            // Only log if the given promise is not of type VoidChannelPromise as trySuccess(...) is expected to return
            // false.
            Util.SafeSetSuccess(promise);
        }
        static void SafeFail(TaskCompletionSource promise, Exception cause)
        {
            // TODO:ChannelPromise
            // Only log if the given promise is not of type VoidChannelPromise as tryFailure(...) is expected to return
            // false.
            Util.SafeSetFailure(promise, cause);
        }
        public long TotalPendingWriteBytes() { return Volatile.Read(ref this.totalPendingSize); }
        public long BytesBeforeUnwritable()
        {
            long bytes = this.channel.Configuration.WriteBufferHighWaterMark - this.totalPendingSize;
            // If bytes is negative we know we are not writable, but if bytes is non-negative we have to check writability.
            // Note that totalPendingSize and isWritable() use different volatile variables that are not synchronized
            // together. totalPendingSize will be updated before isWritable().
            if (bytes > 0)
            {
                return this.IsWritable ? bytes : 0;
            }
            return 0;
        }
        public long BytesBeforeWritable()
        {
            long bytes = this.totalPendingSize - this.channel.Configuration.WriteBufferLowWaterMark;
            // If bytes is negative we know we are writable, but if bytes is non-negative we have to check writability.
            // Note that totalPendingSize and isWritable() use different volatile variables that are not synchronized
            // together. totalPendingSize will be updated before isWritable().
            if (bytes > 0)
            {
                return this.IsWritable ? 0 : bytes;
            }
            return 0;
        }
        public void ForEachFlushedMessage(IMessageProcessor processor)
        {
            Contract.Requires(processor != null);

            Entry entry = this.flushedEntry;
            if (entry == null)
            {
                return;
            }

            do
            {
                if (!entry.Cancelled)
                {
                    if (!processor.ProcessMessage(entry.Message))
                    {
                        return;
                    }
                }
                entry = entry.Next;
            }
            while (this.IsFlushedEntry(entry));
        }
        bool IsFlushedEntry(Entry e) { return e != null && e != this.unflushedEntry; }
        public interface IMessageProcessor
        {
            /// <summary>
            /// Will be called for each flushed message until it either there are no more flushed messages or this method returns <c>false</c>.
            /// </summary>
            /// <param name="msg">The message to process.</param>
            /// <returns><c>true</c> if the given message was successfully processed, otherwise <c>false</c>.</returns>
            bool ProcessMessage(object msg);
        }
        sealed class Entry
        {
            static readonly ThreadLocalPool<Entry> Pool = new ThreadLocalPool<Entry>(h => new Entry(h));

            readonly ThreadLocalPool.Handle handle;
            public Entry Next;
            public object Message;
            public ArraySegment<byte>[] Buffers;
            public ArraySegment<byte> Buffer;
            public TaskCompletionSource Promise;
            public int PendingSize;
            public int Count = -1;
            public bool Cancelled;

            Entry(ThreadLocalPool.Handle handle)
            {
                this.handle = handle;
            }

            public static Entry NewInstance(object msg, int size, TaskCompletionSource promise)
            {
                Entry entry = Pool.Take();
                entry.Message = msg;
                entry.PendingSize = size;
                entry.Promise = promise;
                return entry;
            }

            public int Cancel()
            {
                if (!this.Cancelled)
                {
                    this.Cancelled = true;
                    int pSize = this.PendingSize;

                    // release message and replace with an empty buffer
                    ReferenceCountUtil.SafeRelease(this.Message);
                    this.Message = Unpooled.Empty;

                    this.PendingSize = 0;
                    this.Buffers = null;
                    this.Buffer = new ArraySegment<byte>();
                    return pSize;
                }
                return 0;
            }

            public void Recycle()
            {
                this.Next = null;
                this.Buffers = null;
                this.Buffer = new ArraySegment<byte>();
                this.Message = null;
                this.Promise = null;
                this.PendingSize = 0;
                this.Count = -1;
                this.Cancelled = false;
                this.handle.Release(this);
            }

            public Entry RecycleAndGetNext()
            {
                Entry next = this.Next;
                this.Recycle();
                return next;
            }
        }
        sealed class ThreadLocalByteBufferList : FastThreadLocal<List<ArraySegment<byte>>>
        {
            protected override List<ArraySegment<byte>> GetInitialValue() { return new List<ArraySegment<byte>>(1024); }
        }
    }
}
