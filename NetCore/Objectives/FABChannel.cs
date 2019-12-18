using DotNetty.Buffers;
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
    public abstract class FABChannel : DefaultAttributeMap, IFABChannel
    {
        static readonly NotYetConnectedException NotYetConnectedException = new NotYetConnectedException();
        readonly IFABChannelUnsafe channelUnsafe;
        readonly IFABChannelPipeline pipeline;
        readonly TaskCompletionSource closeFuture = new TaskCompletionSource();
        volatile EndPoint localAddress;
        volatile EndPoint remoteAddress;
        volatile IEventLoop eventLoop;
        volatile bool registered;
        bool strValActive;
        string strVal;
        public readonly FABChannelId Id;
        public readonly IFABChannel Parent;
        public IFABChannelPipeline Pipeline { get { return this.pipeline; } }
        public abstract IChannelConfiguration Configuration { get; }
        public bool IsWritable
        {
            get
            {
                FABChannelOutboundBuffer buf = this.channelUnsafe.OutboundBuffer;
                return buf != null && buf.IsWritable;
            }
        }
        public IByteBufferAllocator Allocator { get { return this.Configuration.Allocator; } }
        public IEventLoop EventLoop
        {
            get
            {
                IEventLoop eventLoop = this.eventLoop;
                if (eventLoop == null)
                {
                    throw new InvalidOperationException("channel not registered to an event loop");
                }
                return eventLoop;
            }
        }
        public abstract bool Open { get; }
        public abstract bool Active { get; }
        public abstract ChannelMetadata Metadata { get; }
        public EndPoint LocalAddress
        {
            get
            {
                EndPoint address = this.localAddress;
                return address ?? this.CacheLocalAddress();
            }
        }
        public EndPoint RemoteAddress
        {
            get
            {
                EndPoint address = this.remoteAddress;
                return address ?? this.CacheRemoteAddress();
            }
        }
        protected abstract EndPoint LocalAddressInternal { get; }
        protected void InvalidateLocalAddress() { this.localAddress = null; }
        protected EndPoint CacheLocalAddress()
        {
            try
            {
                return this.localAddress = this.LocalAddressInternal;
            }
            catch (Exception)
            {
                // Sometimes fails on a closed socket in Windows.
                return null;
            }
        }
        protected abstract EndPoint RemoteAddressInternal { get; }
        protected void InvalidateRemoteAddress() { this.remoteAddress = null; }
        protected EndPoint CacheRemoteAddress()
        {
            try
            {
                return this.remoteAddress = this.RemoteAddressInternal;
            }
            catch (Exception)
            {
                // Sometimes fails on a closed socket in Windows.
                return null;
            }
        }
        public bool Registered { get { return this.registered; } }
        protected virtual FABChannelId NewID() { return FABChannelId.NewInstance(); }
        protected virtual FABChannelPipeline NewChannelPipeline() { return new FABChannelPipeline(this); }
        public virtual Task BindAsync(EndPoint localAddress) { return this.pipeline.BindAsync(localAddress);}
        public virtual Task ConnectAsync(EndPoint remoteAddress) { return this.pipeline.ConnectAsync(remoteAddress);}
        public virtual Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress) { return this.pipeline.ConnectAsync(remoteAddress, localAddress);}
        public virtual Task DisconnectAsync() { return this.pipeline.DisconnectAsync();}
        public virtual Task CloseAsync() { return this.pipeline.CloseAsync();}
        public Task DeregisterAsync() { return this.pipeline.DeregisterAsync(); }
        public IFABChannel Flush()
        {
            this.pipeline.Flush();
            return this;
        }
        public IFABChannel Read()
        {
            this.pipeline.Read();
            return this;
        }
        public Task WriteAsync(object msg) { return this.pipeline.WriteAsync(msg); }
        public Task WriteAndFlushAsync(object message) { return this.pipeline.WriteAndFlushAsync(message);}
        public Task CloseCompletion { get { return this.closeFuture.Task; } }
        public IFABChannelUnsafe Unsafe { get { return this.channelUnsafe; } }
        protected abstract bool IsCompatible(IEventLoop eventLoop);
        protected abstract void DoBeginRead();
        protected abstract void DoWrite(FABChannelOutboundBuffer input);
        protected virtual object FilterOutboundMessage(object msg) { return msg; }
        protected abstract void DoClose();
        public override int GetHashCode() { return this.Id.GetHashCode(); }
        protected abstract IFABChannelUnsafe NewUnsafe();
        public override bool Equals(object o) { return this == o; }
        public int CompareTo(IFABChannel o) { return ReferenceEquals(this, o) ? 0 : this.Id.CompareTo(o.Id); }
        public override string ToString()
        {
            bool active = this.Active;
            if (this.strValActive == active && this.strVal != null)
            {
                return this.strVal;
            }

            EndPoint remoteAddr = this.RemoteAddress;
            EndPoint localAddr = this.LocalAddress;
            if (remoteAddr != null)
            {
                EndPoint srcAddr;
                EndPoint dstAddr;
                if (this.Parent == null)
                {
                    srcAddr = localAddr;
                    dstAddr = remoteAddr;
                }
                else
                {
                    srcAddr = remoteAddr;
                    dstAddr = localAddr;
                }

                StringBuilder buf = new StringBuilder(96)
                    .Append("[id: 0x")
                    .Append(this.Id.AsShortText())
                    .Append(", ")
                    .Append(srcAddr)
                    .Append(active ? " => " : " :> ")
                    .Append(dstAddr)
                    .Append(']');
                this.strVal = buf.ToString();
            }
            else if (localAddr != null)
            {
                StringBuilder buf = new StringBuilder(64)
                    .Append("[id: 0x")
                    .Append(this.Id.AsShortText())
                    .Append(", ")
                    .Append(localAddr)
                    .Append(']');
                this.strVal = buf.ToString();
            }
            else
            {
                StringBuilder buf = new StringBuilder(16)
                    .Append("[id: 0x")
                    .Append(this.Id.AsShortText())
                    .Append(']');
                this.strVal = buf.ToString();
            }

            this.strValActive = active;
            return this.strVal;
        }
        protected FABChannel(IFABChannel parent)
        {
            this.Parent = parent;
            this.Id = this.NewID();
            this.channelUnsafe = this.NewUnsafe();
            this.pipeline = this.NewChannelPipeline();
        }
        protected FABChannel(IFABChannel parent, FABChannelId id)
        {
            this.Parent = parent;
            this.Id = id;
            this.channelUnsafe = this.NewUnsafe();
            this.pipeline = this.NewChannelPipeline();
        }

        IFABChannelId IFABChannel.Id
        {
            get { return this.Id; }
        }
        IFABChannel IFABChannel.Parent
        {
            get { return this.Parent; }
        }
        protected abstract class FABUnsafe : IFABChannelUnsafe
        {
            protected readonly FABChannel channel;
            FABChannelOutboundBuffer outboundBuffer;
            IRecvByteBufAllocatorHandle recvHandle;
            bool inFlush0;
            /// <summary> true if the channel has never been registered, false otherwise /// </summary>
            bool neverRegistered = true;
            public IRecvByteBufAllocatorHandle RecvBufAllocHandle
            {
                get
                {
                    if (this.recvHandle == null)
                        this.recvHandle = this.channel.Configuration.RecvByteBufAllocator.NewHandle();
                    return this.recvHandle;
                }
            }
            //public ChannelHandlerInvoker invoker() {
            //    // return the unwrapped invoker.
            //    return ((PausableChannelEventExecutor) eventLoop().asInvoker()).unwrapInvoker();
            //}
            protected FABUnsafe(FABChannel channel)
            {
                this.channel = channel;
                this.outboundBuffer = new FABChannelOutboundBuffer(channel);
            }
            public FABChannelOutboundBuffer OutboundBuffer { get { return this.outboundBuffer; } }

            void AssertEventLoop() { Contract.Assert(!this.channel.registered || this.channel.eventLoop.InEventLoop); }

            public Task RegisterAsync(IEventLoop eventLoop)
            {
                Contract.Requires(eventLoop != null);

                if (this.channel.Registered)
                {
                    return TaskEx.FromException(new InvalidOperationException("registered to an event loop already"));
                }

                if (!this.channel.IsCompatible(eventLoop))
                {
                    return TaskEx.FromException(new InvalidOperationException("incompatible event loop type: " + eventLoop.GetType().Name));
                }

                this.channel.eventLoop = eventLoop;

                var promise = new TaskCompletionSource();

                if (eventLoop.InEventLoop)
                {
                    this.Register0(promise);
                }
                else
                {
                    try
                    {
                        eventLoop.Execute((u, p) => ((FABUnsafe)u).Register0((TaskCompletionSource)p), this, promise);
                    }
                    catch (Exception ex)
                    {
                        this.CloseForcibly();
                        this.channel.closeFuture.Complete();
                        Util.SafeSetFailure(promise, ex);
                    }
                }

                return promise.Task;
            }

            void Register0(TaskCompletionSource promise)
            {
                try
                {
                    // check if the channel is still open as it could be closed input the mean time when the register
                    // call was outside of the eventLoop
                    if (!promise.SetUncancellable() || !this.EnsureOpen(promise))
                    {
                        Util.SafeSetFailure(promise, new ClosedChannelException());
                        return;
                    }
                    bool firstRegistration = this.neverRegistered;
                    this.channel.DoRegister();
                    this.neverRegistered = false;
                    this.channel.registered = true;

                    Util.SafeSetSuccess(promise);
                    this.channel.pipeline.FireChannelRegistered();
                    // Only fire a channelActive if the channel has never been registered. This prevents firing
                    // multiple channel actives if the channel is deregistered and re-registered.
                    if (this.channel.Active)
                    {
                        if (firstRegistration)
                        {
                            this.channel.pipeline.FireChannelActive();
                        }
                        else if (this.channel.Configuration.AutoRead)
                        {
                            // This channel was registered before and autoRead() is set. This means we need to begin read
                            // again so that we process inbound data.
                            //
                            // See https://github.com/netty/netty/issues/4805
                            this.BeginRead();
                        }
                    }
                }
                catch (Exception t)
                {
                    // Close the channel directly to avoid FD leak.
                    this.CloseForcibly();
                    this.channel.closeFuture.Complete();
                    Util.SafeSetFailure(promise, t, Logger);
                }
            }

            public Task BindAsync(EndPoint localAddress)
            {
                this.AssertEventLoop();

                // todo: cancellation support
                if ( /*!promise.setUncancellable() || */!this.channel.Open)
                {
                    return this.CreateClosedChannelExceptionTask();
                }

                //// See: https://github.com/netty/netty/issues/576
                //if (bool.TrueString.Equals(this.channel.Configuration.getOption(ChannelOption.SO_BROADCAST)) &&
                //    localAddress is IPEndPoint &&
                //    !((IPEndPoint)localAddress).Address.getAddress().isAnyLocalAddress() &&
                //    !Environment.OSVersion.Platform == PlatformID.Win32NT && !Environment.isRoot())
                //{
                //    // Warn a user about the fact that a non-root user can't receive a
                //    // broadcast packet on *nix if the socket is bound on non-wildcard address.
                //    logger.Warn(
                //        "A non-root user can't receive a broadcast packet if the socket " +
                //            "is not bound to a wildcard address; binding to a non-wildcard " +
                //            "address (" + localAddress + ") anyway as requested.");
                //}

                bool wasActive = this.channel.Active;
                try
                {
                    this.channel.DoBind(localAddress);
                }
                catch (Exception t)
                {
                    this.CloseIfClosed();
                    return TaskEx.FromException(t);
                }

                if (!wasActive && this.channel.Active)
                {
                    this.InvokeLater(() => this.channel.pipeline.FireChannelActive());
                }

                return TaskEx.Completed;
            }

            public abstract Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

            public Task DisconnectAsync()
            {
                this.AssertEventLoop();

                bool wasActive = this.channel.Active;
                try
                {
                    this.channel.DoDisconnect();
                }
                catch (Exception t)
                {
                    this.CloseIfClosed();
                    return TaskEx.FromException(t);
                }

                if (wasActive && !this.channel.Active)
                {
                    this.InvokeLater(() => this.channel.pipeline.FireChannelInactive());
                }

                this.CloseIfClosed(); // doDisconnect() might have closed the channel

                return TaskEx.Completed;
            }

            public Task CloseAsync() /*CancellationToken cancellationToken) */
            {
                this.AssertEventLoop();

                return this.CloseAsync(new ClosedChannelException(), false);
            }

            protected Task CloseAsync(Exception cause, bool notify)
            {
                var promise = new TaskCompletionSource();
                if (!promise.SetUncancellable())
                {
                    return promise.Task;
                }

                FABChannelOutboundBuffer outboundBuffer = this.outboundBuffer;
                if (outboundBuffer == null)
                {
                    // Only needed if no VoidChannelPromise.
                    if (promise != TaskCompletionSource.Void)
                    {
                        // This means close() was called before so we just register a listener and return
                        return this.channel.closeFuture.Task;
                    }
                    return promise.Task;
                }

                if (this.channel.closeFuture.Task.IsCompleted)
                {
                    // Closed already.
                    Util.SafeSetSuccess(promise);
                    return promise.Task;
                }

                bool wasActive = this.channel.Active;
                this.outboundBuffer = null; // Disallow adding any messages and flushes to outboundBuffer.
                IEventExecutor closeExecutor = null; // todo closeExecutor();
                if (closeExecutor != null)
                {
                    closeExecutor.Execute(() =>
                    {
                        try
                        {
                            // Execute the close.
                            this.DoClose0(promise);
                        }
                        finally
                        {
                            // Call invokeLater so closeAndDeregister is executed input the EventLoop again!
                            this.InvokeLater(() =>
                            {
                                // Fail all the queued messages
                                outboundBuffer.FailFlushed(cause, notify);
                                outboundBuffer.Close(new ClosedChannelException());
                                this.FireChannelInactiveAndDeregister(wasActive);
                            });
                        }
                    });
                }
                else
                {
                    try
                    {
                        // Close the channel and fail the queued messages input all cases.
                        this.DoClose0(promise);
                    }
                    finally
                    {
                        // Fail all the queued messages.
                        outboundBuffer.FailFlushed(cause, notify);
                        outboundBuffer.Close(new ClosedChannelException());
                    }
                    if (this.inFlush0)
                    {
                        this.InvokeLater(() => this.FireChannelInactiveAndDeregister(wasActive));
                    }
                    else
                    {
                        this.FireChannelInactiveAndDeregister(wasActive);
                    }
                }

                return promise.Task;
            }

            void DoClose0(TaskCompletionSource promise)
            {
                try
                {
                    this.channel.DoClose();
                    this.channel.closeFuture.Complete();
                    Util.SafeSetSuccess(promise);
                }
                catch (Exception t)
                {
                    this.channel.closeFuture.Complete();
                    Util.SafeSetFailure(promise, t);
                }
            }

            void FireChannelInactiveAndDeregister(bool wasActive) { this.DeregisterAsync(wasActive && !this.channel.Active); }

            public void CloseForcibly()
            {
                this.AssertEventLoop();

                try
                {
                    this.channel.DoClose();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            /// <summary>
            /// This method must NEVER be called directly, but be executed as an
            /// extra task with a clean call stack instead. The reason for this
            /// is that this method calls <see cref="IChannelPipeline.FireChannelUnregistered"/>
            /// directly, which might lead to an unfortunate nesting of independent inbound/outbound
            /// events. See the comments input <see cref="InvokeLater"/> for more details.
            /// </summary>
            public Task DeregisterAsync()
            {
                this.AssertEventLoop();

                return this.DeregisterAsync(false);
            }

            Task DeregisterAsync(bool fireChannelInactive)
            {
                //if (!promise.setUncancellable())
                //{
                //    return;
                //}

                if (!this.channel.registered)
                {
                    return TaskEx.Completed;
                }

                var promise = new TaskCompletionSource();

                // As a user may call deregister() from within any method while doing processing in the ChannelPipeline,
                // we need to ensure we do the actual deregister operation later. This is needed as for example,
                // we may be in the ByteToMessageDecoder.callDecode(...) method and so still try to do processing in
                // the old EventLoop while the user already registered the Channel to a new EventLoop. Without delay,
                // the deregister operation this could lead to have a handler invoked by different EventLoop and so
                // threads.
                //
                // See:
                // https://github.com/netty/netty/issues/4435
                this.InvokeLater(() =>
                {
                    try
                    {
                        this.channel.DoDeregister();
                    }
                    catch (Exception t)
                    {
                        throw t;
                    }
                    finally
                    {
                        if (fireChannelInactive)
                        {
                            this.channel.pipeline.FireChannelInactive();
                        }
                        // Some transports like local and AIO does not allow the deregistration of
                        // an open channel.  Their doDeregister() calls close(). Consequently,
                        // close() calls deregister() again - no need to fire channelUnregistered, so check
                        // if it was registered.
                        if (this.channel.registered)
                        {
                            this.channel.registered = false;
                            this.channel.pipeline.FireChannelUnregistered();
                        }
                        Util.SafeSetSuccess(promise);
                    }
                });

                return promise.Task;
            }

            public void BeginRead()
            {
                this.AssertEventLoop();

                if (!this.channel.Active)
                {
                    return;
                }

                try
                {
                    this.channel.DoBeginRead();
                }
                catch (Exception e)
                {
                    this.InvokeLater(() => this.channel.pipeline.FireExceptionCaught(e));
                    this.CloseSafe();
                }
            }

            public Task WriteAsync(object msg)
            {
                this.AssertEventLoop();

                FABChannelOutboundBuffer outboundBuffer = this.outboundBuffer;
                if (outboundBuffer == null)
                {
                    // If the outboundBuffer is null we know the channel was closed and so
                    // need to fail the future right away. If it is not null the handling of the rest
                    // will be done input flush0()
                    // See https://github.com/netty/netty/issues/2362

                    // release message now to prevent resource-leak
                    ReferenceCountUtil.Release(msg);
                    return TaskEx.FromException(new ClosedChannelException());
                }

                int size;
                try
                {
                    msg = this.channel.FilterOutboundMessage(msg);
                    size = this.channel.pipeline.EstimatorHandle.Size(msg);
                    if (size < 0)
                    {
                        size = 0;
                    }
                }
                catch (Exception t)
                {
                    ReferenceCountUtil.Release(msg);

                    return TaskEx.FromException(t);
                }

                var promise = new TaskCompletionSource();
                outboundBuffer.AddMessage(msg, size, promise);
                return promise.Task;
            }

            public void Flush()
            {
                this.AssertEventLoop();

                FABChannelOutboundBuffer outboundBuffer = this.outboundBuffer;
                if (outboundBuffer == null)
                {
                    return;
                }

                outboundBuffer.AddFlush();
                this.Flush0();
            }

            protected virtual void Flush0()
            {
                if (this.inFlush0)
                {
                    // Avoid re-entrance
                    return;
                }

                FABChannelOutboundBuffer outboundBuffer = this.outboundBuffer;
                if (outboundBuffer == null || outboundBuffer.IsEmpty)
                {
                    return;
                }

                this.inFlush0 = true;

                // Mark all pending write requests as failure if the channel is inactive.
                if (!this.CanWrite)
                {
                    try
                    {
                        if (this.channel.Open)
                        {
                            outboundBuffer.FailFlushed(NotYetConnectedException, true);
                        }
                        else
                        {
                            // Do not trigger channelWritabilityChanged because the channel is closed already.
                            outboundBuffer.FailFlushed(new ClosedChannelException(), false);
                        }
                    }
                    finally
                    {
                        this.inFlush0 = false;
                    }
                    return;
                }

                try
                {
                    this.channel.DoWrite(outboundBuffer);
                }
                catch (Exception ex)
                {
                    Util.CompleteChannelCloseTaskSafely(this.channel, this.CloseAsync(new ClosedChannelException("Failed to write", ex), false));
                }
                finally
                {
                    this.inFlush0 = false;
                }
            }

            protected virtual bool CanWrite { get {return this.channel.Active;}}

            protected bool EnsureOpen(TaskCompletionSource promise)
            {
                if (this.channel.Open)
                {
                    return true;
                }

                Util.SafeSetFailure(promise, new ClosedChannelException());
                return false;
            }

            protected Task CreateClosedChannelExceptionTask() { return TaskEx.FromException(new ClosedChannelException());}

            protected void CloseIfClosed()
            {
                if (this.channel.Open)
                {
                    return;
                }
                this.CloseSafe();
            }

            void InvokeLater(Action task)
            {
                try
                {
                    // This method is used by outbound operation implementations to trigger an inbound event later.
                    // They do not trigger an inbound event immediately because an outbound operation might have been
                    // triggered by another inbound event handler method.  If fired immediately, the call stack
                    // will look like this for example:
                    //
                    //   handlerA.inboundBufferUpdated() - (1) an inbound handler method closes a connection.
                    //   -> handlerA.ctx.close()
                    //      -> channel.unsafe.close()
                    //         -> handlerA.channelInactive() - (2) another inbound handler method called while input (1) yet
                    //
                    // which means the execution of two inbound handler methods of the same handler overlap undesirably.
                    this.channel.EventLoop.Execute(task);
                }
                catch (RejectedExecutionException e)
                {
                    throw e;
                }
            }

            protected Exception AnnotateConnectException(Exception exception, EndPoint remoteAddress)
            {
                if (exception is SocketException)
                {
                    return new ConnectException("LogError connecting to " + remoteAddress, exception);
                }

                return exception;
            }

            /// <summary>
            /// Prepares to close the <see cref="IChannel"/>. If this method returns an <see cref="IEventExecutor"/>, the
            /// caller must call the <see cref="IEventExecutor.Execute(DotNetty.Common.Concurrency.IRunnable)"/> method with a task that calls
            /// <see cref="AbstractChannel.DoClose"/> on the returned <see cref="IEventExecutor"/>. If this method returns <c>null</c>,
            /// <see cref="AbstractChannel.DoClose"/> must be called from the caller thread. (i.e. <see cref="IEventLoop"/>)
            /// </summary>
            protected virtual IEventExecutor PrepareToClose() { return null;}
        }
    }
}
