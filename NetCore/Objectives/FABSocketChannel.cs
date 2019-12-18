using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using NetCore.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public abstract class FABSocketChannel : FABChannel
    {
        [Flags]
        protected enum StateFlags
        {
            Open = 1,
            ReadScheduled = 1 << 1,
            WriteScheduled = 1 << 2,
            Active = 1 << 3,
        }
        internal static readonly EventHandler<SocketAsyncEventArgs> IoCompletedCallback = OnIoCompleted;
        static readonly Action<object, object> ConnectCallbackAction = (u, e) => ((IFABSocketChannelUnsafe)u).FinishConnect((FABSocketChannelAsyncOperation)e);
        static readonly Action<object, object> ReadCallbackAction = (u, e) => ((IFABSocketChannelUnsafe)u).FinishRead((FABSocketChannelAsyncOperation)e);
        static readonly Action<object, object> WriteCallbackAction = (u, e) => ((IFABSocketChannelUnsafe)u).FinishWrite((FABSocketChannelAsyncOperation)e);
        protected readonly Socket Socket;
        FABSocketChannelAsyncOperation readOperation;
        FABSocketChannelAsyncOperation writeOperation;
        volatile bool inputShutdown;
        internal bool ReadPending;
        volatile StateFlags state;
        TaskCompletionSource connectPromise;
        IScheduledTask connectCancellationTask;
        protected FABSocketChannel(IFABChannel parent, Socket socket)
            : base(parent)
        {
            this.Socket = socket;
            this.state = StateFlags.Open;

            try
            {
                this.Socket.Blocking = false;
            }
            catch (SocketException ex)
            {
                try
                {
                    socket.Dispose();
                }
                catch (SocketException ex2)
                {
                    throw ex2;
                }

                throw new FABChannelException("Failed to enter non-blocking mode.", ex);
            }
        }
        public override bool Open { get { return this.IsInState(StateFlags.Open); } }
        public override bool Active { get { return this.IsInState(StateFlags.Active); } }
        protected internal void ClearReadPending()
        {
            if (this.Registered)
            {
                IEventLoop eventLoop = this.EventLoop;
                if (eventLoop.InEventLoop)
                {
                    this.ClearReadPending0();
                }
                else
                {
                    eventLoop.Execute(channel => ((FABSocketChannel)channel).ClearReadPending0(), this);
                }
            }
            else
            {
                // Best effort if we are not registered yet clear ReadPending. This happens during channel initialization.
                // NB: We only set the boolean field instead of calling ClearReadPending0(), because the SelectionKey is
                // not set yet so it would produce an assertion failure.
                this.ReadPending = false;
            }
        }
        void ClearReadPending0() { this.ReadPending = false; }
        protected bool InputShutdown { get { return this.inputShutdown; } }
        protected void ShutdownInput() { this.inputShutdown = true;}
        protected void SetState(StateFlags stateToSet) { this.state |= stateToSet; }
        protected StateFlags ResetState(StateFlags stateToReset)
        {
            StateFlags oldState = this.state;
            if ((oldState & stateToReset) != 0)
            {
                this.state = oldState & ~stateToReset;
            }
            return oldState;
        }
        protected bool TryResetState(StateFlags stateToReset)
        {
            StateFlags oldState = this.state;
            if ((oldState & stateToReset) != 0)
            {
                this.state = oldState & ~stateToReset;
                return true;
            }
            return false;
        }
        protected bool IsInState(StateFlags stateToCheck) { return (this.state & stateToCheck) == stateToCheck; }
        protected FABSocketChannelAsyncOperation ReadOperation 
        { 
            get 
            { 
                if(this.readOperation == null) 
                {
                    this.readOperation = new FABSocketChannelAsyncOperation(this, true);
                }
                return this.readOperation;
            }
        }
        FABSocketChannelAsyncOperation WriteOperation
        { 
            get 
            { 
                if(this.writeOperation == null) 
                {
                    this.writeOperation = new FABSocketChannelAsyncOperation(this, false);
                }
                return this.writeOperation;
            }
        }
        protected FABSocketChannelAsyncOperation PrepareWriteOperation(ArraySegment<byte> buffer)
        {
            FABSocketChannelAsyncOperation operation = this.WriteOperation;
            operation.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
            return operation;
        }
        protected FABSocketChannelAsyncOperation PrepareWriteOperation(IList<ArraySegment<byte>> buffers)
        {
            FABSocketChannelAsyncOperation operation = this.WriteOperation;
            operation.BufferList = buffers;
            return operation;
        }
        protected void ResetWriteOperation()
        {
            FABSocketChannelAsyncOperation operation = this.writeOperation;
            Contract.Assert(operation != null);
            if (operation.BufferList == null)
            {
                operation.SetBuffer(null, 0, 0);
            }
            else
            {
                operation.BufferList = null;
            }
        }
        protected abstract class FABSocketUnsafe : FABUnsafe, IFABSocketChannelUnsafe
        {
            protected FABSocketUnsafe(FABSocketChannel channel)
                : base(channel)
            {
            }

            public FABSocketChannel Channel { get { return (FABSocketChannel)this.channel; } set { this.Channel = value; } }

            public sealed override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                // todo: handle cancellation
                FABSocketChannel ch = this.Channel;
                if (!ch.Open)
                {
                    return this.CreateClosedChannelExceptionTask();
                }

                try
                {
                    if (ch.connectPromise != null)
                    {
                        throw new InvalidOperationException("connection attempt already made");
                    }

                    bool wasActive = this.channel.Active;
                    if (ch.DoConnect(remoteAddress, localAddress))
                    {
                        this.FulfillConnectPromise(wasActive);
                        return TaskEx.Completed;
                    }
                    else
                    {
                        ch.connectPromise = new TaskCompletionSource(remoteAddress);

                        // Schedule connect timeout.
                        TimeSpan connectTimeout = ch.Configuration.ConnectTimeout;
                        if (connectTimeout > TimeSpan.Zero)
                        {
                            ch.connectCancellationTask = ch.EventLoop.Schedule(
                                (c, a) =>
                                {
                                    // todo: make static / cache delegate?..
                                    var self = (FABSocketChannel)c;
                                    // todo: call Socket.CancelConnectAsync(...)
                                    TaskCompletionSource promise = self.connectPromise;
                                    var cause = new ConnectTimeoutException("connection timed out: " + a.ToString());
                                    if (promise != null && promise.TrySetException(cause))
                                    {
                                        self.CloseSafe();
                                    }
                                },
                                this.channel,
                                remoteAddress,
                                connectTimeout);
                        }

                        ch.connectPromise.Task.ContinueWith(
                            (t, s) =>
                            {
                                var c = (FABSocketChannel)s;
                                c.connectCancellationTask.Cancel();
                                c.connectPromise = null;
                                c.CloseSafe();
                            },
                            ch,
                            TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously);

                        return ch.connectPromise.Task;
                    }
                }
                catch (Exception ex)
                {
                    this.CloseIfClosed();
                    return TaskEx.FromException(this.AnnotateConnectException(ex, remoteAddress));
                }
            }

            void FulfillConnectPromise(bool wasActive)
            {
                // Regardless if the connection attempt was cancelled, channelActive() event should be triggered,
                // because what happened is what happened.
                if (!wasActive && this.channel.Active)
                {
                    this.channel.Pipeline.FireChannelActive();
                }

                TaskCompletionSource promise = this.Channel.connectPromise;
                // If promise is null, then it the channel was Closed via cancellation and the promise has been notified already.
                if (promise != null)
                {
                    // trySuccess() will return false if a user cancelled the connection attempt.
                    bool promiseSet = promise.TryComplete();

                    // If a user cancelled the connection attempt, close the channel, which is followed by channelInactive().
                    if (!promiseSet)
                    {
                        this.CloseSafe();
                    }

                }
            }

            void FulfillConnectPromise(Exception cause)
            {
                TaskCompletionSource promise = this.Channel.connectPromise;
                if (promise == null)
                {
                    // Closed via cancellation and the promise has been notified already.
                    return;
                }

                // Use tryFailure() instead of setFailure() to avoid the race against cancel().
                promise.TrySetException(cause);
                this.CloseIfClosed();
            }

            public void FinishConnect(FABSocketChannelAsyncOperation operation)
            {
                Contract.Assert(this.channel.EventLoop.InEventLoop);

                FABSocketChannel ch = this.Channel;
                try
                {
                    bool wasActive = ch.Active;
                    ch.DoFinishConnect(operation);
                    this.FulfillConnectPromise(wasActive);
                }
                catch (Exception ex)
                {
                    TaskCompletionSource promise = ch.connectPromise;
                    var remoteAddress = (EndPoint)promise.Task.AsyncState;
                    this.FulfillConnectPromise(this.AnnotateConnectException(ex, remoteAddress));
                }
                finally
                {
                    // Check for null as the connectTimeoutFuture is only created if a connectTimeoutMillis > 0 is used
                    // See https://github.com/netty/netty/issues/1770
                    if(ch.connectCancellationTask != null)
                        ch.connectCancellationTask.Cancel();
                    ch.connectPromise = null;
                }
            }

            public abstract void FinishRead(FABSocketChannelAsyncOperation operation);

            protected sealed override void Flush0()
            {
                // Flush immediately only when there's no pending flush.
                // If there's a pending flush operation, event loop will call FinishWrite() later,
                // and thus there's no need to call it now.
                if (this.IsFlushPending())
                {
                    return;
                }
                base.Flush0();
            }

            public void FinishWrite(FABSocketChannelAsyncOperation operation)
            {
                bool resetWritePending = this.Channel.TryResetState(StateFlags.WriteScheduled);

                Contract.Assert(resetWritePending);

                FABChannelOutboundBuffer input = this.OutboundBuffer;
                try
                {
                    operation.Validate();
                    int sent = operation.BytesTransferred;
                    this.Channel.ResetWriteOperation();
                    if (sent > 0)
                    {
                        input.RemoveBytes(sent);
                    }
                }
                catch (Exception ex)
                {
                    Util.CompleteChannelCloseTaskSafely(this.channel, this.CloseAsync(new ClosedChannelException("Failed to write", ex), false));
                }

                // Double check if there's no pending flush
                // See https://github.com/Azure/DotNetty/issues/218
                this.Flush0(); // todo: does it make sense now that we've actually written out everything that was flushed previously? concurrent flush handling?
            }

            bool IsFlushPending() { return this.Channel.IsInState(StateFlags.WriteScheduled); }
        }
        /// <remarks>PORT NOTE: matches behavior of NioEventLoop.processSelectedKey</remarks>
        static void OnIoCompleted(object sender, SocketAsyncEventArgs args)
        {
            var operation = (FABSocketChannelAsyncOperation)args;
            FABSocketChannel channel = operation.Channel;
            var @unsafe = (IFABChannelUnsafe)channel.Unsafe;
            IEventLoop eventLoop = channel.EventLoop;
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishRead(operation);
                    }
                    else
                    {
                        eventLoop.Execute(ReadCallbackAction, @unsafe, operation);
                    }
                    break;
                case SocketAsyncOperation.Connect:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishConnect(operation);
                    }
                    else
                    {
                        eventLoop.Execute(ConnectCallbackAction, @unsafe, operation);
                    }
                    break;
                case SocketAsyncOperation.Receive:
                case SocketAsyncOperation.ReceiveFrom:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishRead(operation);
                    }
                    else
                    {
                        eventLoop.Execute(ReadCallbackAction, @unsafe, operation);
                    }
                    break;
                case SocketAsyncOperation.Send:
                case SocketAsyncOperation.SendTo:
                    if (eventLoop.InEventLoop)
                    {
                        @unsafe.FinishWrite(operation);
                    }
                    else
                    {
                        eventLoop.Execute(WriteCallbackAction, @unsafe, operation);
                    }
                    break;
                default:
                    // todo: think of a better way to comm exception
                    throw new ArgumentException("The last operation completed on the socket was not expected");
            }
        }
        public abstract void FinishRead(FABSocketChannelAsyncOperation operation);
        protected override bool IsCompatible(IEventLoop eventLoop) { return true; }
        protected override void DoBeginRead()
        {
            if (this.inputShutdown)
            {
                return;
            }

            if (!this.Open)
            {
                return;
            }

            this.ReadPending = true;

            if (!this.IsInState(StateFlags.ReadScheduled))
            {
                this.state |= StateFlags.ReadScheduled;
                this.ScheduleSocketRead();
            }
        }
        protected abstract void ScheduleSocketRead();
        protected abstract bool DoConnect(EndPoint remoteAddress, EndPoint localAddress);
        protected abstract void DoFinishConnect(FABSocketChannelAsyncOperation operation);
        protected override void DoClose()
        {
            TaskCompletionSource promise = this.connectPromise;
            if (promise != null)
            {
                // Use TrySetException() instead of SetException() to avoid the race against cancellation due to timeout.
                promise.TrySetException(new ClosedChannelException());
                this.connectPromise = null;
            }

            IScheduledTask cancellationTask = this.connectCancellationTask;
            if (cancellationTask != null)
            {
                cancellationTask.Cancel();
                this.connectCancellationTask = null;
            }

            FABSocketChannelAsyncOperation readOp = this.readOperation;
            if (readOp != null)
            {
                readOp.Dispose();
                this.readOperation = null;
            }

            FABSocketChannelAsyncOperation writeOp = this.writeOperation;
            if (writeOp != null)
            {
                writeOp.Dispose();
                this.writeOperation = null;
            }
        }
    }
}
