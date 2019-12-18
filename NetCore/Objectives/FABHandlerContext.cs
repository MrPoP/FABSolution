using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Common;
using System.Runtime.CompilerServices;
using System.Net;
using System.Reflection;
using DotNetty.Common.Concurrency;
using System.Diagnostics.Contracts;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Common.Internal;

namespace NetCore
{
    sealed class SealedFABHandlerContext : FABHandlerContext
    {
        readonly IFABChannelHandler handler;
        public SealedFABHandlerContext(
            FABChannelPipeline pipeline, IEventExecutor executor, string name, IFABChannelHandler handler)
            : base(pipeline, executor, name, GetSkipPropagationFlags(handler))
        {
            Contract.Requires(handler != null);

            this.handler = handler;
        }

        public override IFABChannelHandler Handler { get { return this.handler; } }
    }
    abstract class FABHandlerContext : IFABChannelHandlerContext, IResourceLeakHint
    {
        static readonly Action<object> InvokeChannelReadCompleteAction = ctx => ((FABHandlerContext)ctx).InvokeChannelReadComplete();
        static readonly Action<object> InvokeReadAction = ctx => ((FABHandlerContext)ctx).InvokeRead();
        static readonly Action<object> InvokeChannelWritabilityChangedAction = ctx => ((FABHandlerContext)ctx).InvokeChannelWritabilityChanged();
        static readonly Action<object> InvokeFlushAction = ctx => ((FABHandlerContext)ctx).InvokeFlush();
        static readonly Action<object, object> InvokeUserEventTriggeredAction = (ctx, evt) => ((FABHandlerContext)ctx).InvokeUserEventTriggered(evt);
        static readonly Action<object, object> InvokeChannelReadAction = (ctx, msg) => ((FABHandlerContext)ctx).InvokeChannelRead(msg);
        [Flags]
        protected internal enum SkipFlags
        {
            HandlerAdded = 1,
            HandlerRemoved = 1 << 1,
            ExceptionCaught = 1 << 2,
            ChannelRegistered = 1 << 3,
            ChannelUnregistered = 1 << 4,
            ChannelActive = 1 << 5,
            ChannelInactive = 1 << 6,
            ChannelRead = 1 << 7,
            ChannelReadComplete = 1 << 8,
            ChannelWritabilityChanged = 1 << 9,
            UserEventTriggered = 1 << 10,
            Bind = 1 << 11,
            Connect = 1 << 12,
            Disconnect = 1 << 13,
            Close = 1 << 14,
            Deregister = 1 << 15,
            Read = 1 << 16,
            Write = 1 << 17,
            Flush = 1 << 18,

            Inbound = ExceptionCaught |
                ChannelRegistered |
                ChannelUnregistered |
                ChannelActive |
                ChannelInactive |
                ChannelRead |
                ChannelReadComplete |
                ChannelWritabilityChanged |
                UserEventTriggered,

            Outbound = Bind |
                Connect |
                Disconnect |
                Close |
                Deregister |
                Read |
                Write |
                Flush,
        }
        static readonly ConditionalWeakTable<Type, Tuple<SkipFlags>> SkipTable = new ConditionalWeakTable<Type, Tuple<SkipFlags>>();
        protected static SkipFlags GetSkipPropagationFlags(IFABChannelHandler handler)
        {
            Tuple<SkipFlags> skipDirection = SkipTable.GetValue(
                handler.GetType(),
                handlerType => Tuple.Create(CalculateSkipPropagationFlags(handlerType)));

            return skipDirection == null ? 0 : skipDirection.Item1;
        }
        protected static SkipFlags CalculateSkipPropagationFlags(Type handlerType)
        {
            SkipFlags flags = 0;

            // this method should never throw
            if (IsSkippable(handlerType, "HandlerAdded"))
            {
                flags |= SkipFlags.HandlerAdded;
            }
            if (IsSkippable(handlerType, "HandlerRemoved"))
            {
                flags |= SkipFlags.HandlerRemoved;
            }
            if (IsSkippable(handlerType, "ExceptionCaught", typeof(Exception)))
            {
                flags |= SkipFlags.ExceptionCaught;
            }
            if (IsSkippable(handlerType, "ChannelRegistered"))
            {
                flags |= SkipFlags.ChannelRegistered;
            }
            if (IsSkippable(handlerType, "ChannelUnregistered"))
            {
                flags |= SkipFlags.ChannelUnregistered;
            }
            if (IsSkippable(handlerType, "ChannelActive"))
            {
                flags |= SkipFlags.ChannelActive;
            }
            if (IsSkippable(handlerType, "ChannelInactive"))
            {
                flags |= SkipFlags.ChannelInactive;
            }
            if (IsSkippable(handlerType, "ChannelRead", typeof(object)))
            {
                flags |= SkipFlags.ChannelRead;
            }
            if (IsSkippable(handlerType, "ChannelReadComplete"))
            {
                flags |= SkipFlags.ChannelReadComplete;
            }
            if (IsSkippable(handlerType, "ChannelWritabilityChanged"))
            {
                flags |= SkipFlags.ChannelWritabilityChanged;
            }
            if (IsSkippable(handlerType, "UserEventTriggered", typeof(object)))
            {
                flags |= SkipFlags.UserEventTriggered;
            }
            if (IsSkippable(handlerType, "BindAsync", typeof(EndPoint)))
            {
                flags |= SkipFlags.Bind;
            }
            if (IsSkippable(handlerType, "ConnectAsync", typeof(EndPoint), typeof(EndPoint)))
            {
                flags |= SkipFlags.Connect;
            }
            if (IsSkippable(handlerType, "DisconnectAsync"))
            {
                flags |= SkipFlags.Disconnect;
            }
            if (IsSkippable(handlerType, "CloseAsync"))
            {
                flags |= SkipFlags.Close;
            }
            if (IsSkippable(handlerType, "DeregisterAsync"))
            {
                flags |= SkipFlags.Deregister;
            }
            if (IsSkippable(handlerType, "Read"))
            {
                flags |= SkipFlags.Read;
            }
            if (IsSkippable(handlerType, "WriteAsync", typeof(object)))
            {
                flags |= SkipFlags.Write;
            }
            if (IsSkippable(handlerType, "Flush"))
            {
                flags |= SkipFlags.Flush;
            }
            return flags;
        }
        protected static bool IsSkippable(Type handlerType, string methodName) { return IsSkippable(handlerType, methodName, Type.EmptyTypes); }

        protected static bool IsSkippable(Type handlerType, string methodName, params Type[] paramTypes)
        {
            var newParamTypes = new Type[paramTypes.Length + 1];
            newParamTypes[0] = typeof(IChannelHandlerContext);
            Array.Copy(paramTypes, 0, newParamTypes, 1, paramTypes.Length);
            return handlerType.GetMethod(methodName, newParamTypes).GetCustomAttribute<SkipAttribute>(false) != null;
        }
        internal volatile FABHandlerContext Next;
        internal volatile FABHandlerContext Prev;
        internal readonly SkipFlags SkipPropagationFlags;
        enum HandlerState
        {
            /// <summary>Neither <see cref="IChannelHandler.HandlerAdded"/> nor <see cref="IChannelHandler.HandlerRemoved"/> was called.</summary>
            Init = 0,
            /// <summary><see cref="IChannelHandler.HandlerAdded"/> was called.</summary>
            Added = 1,
            /// <summary><see cref="IChannelHandler.HandlerRemoved"/> was called.</summary>
            Removed = 2
        }
        internal readonly FABChannelPipeline pipeline;
        internal readonly IEventExecutor executor;
        HandlerState handlerState = HandlerState.Init;
        internal readonly string Name;
        protected FABHandlerContext(FABChannelPipeline pipeline, IEventExecutor executor,
            string name, SkipFlags skipPropagationDirections)
        {
            Contract.Requires(pipeline != null);
            Contract.Requires(name != null);

            this.pipeline = pipeline;
            this.Name = name;
            this.executor = executor;
            this.SkipPropagationFlags = skipPropagationDirections;
        }
        public IFABChannel Channel { get { return this.pipeline.Channel; } }
        public IByteBufferAllocator Allocator { get { return this.Channel.Allocator; } }
        public abstract IFABChannelHandler Handler { get; }
        public bool Added { get { return handlerState == HandlerState.Added; } }
        public bool Removed { get { return handlerState == HandlerState.Removed; } }
        internal void SetAdded() { handlerState = HandlerState.Added; }
        internal void SetRemoved() { handlerState = HandlerState.Removed; }
        public IEventExecutor Executor { get { return this.executor == null ? this.Channel.EventLoop : this.executor; } }
        public IAttribute<T> GetAttribute<T>(AttributeKey<T> key)
            where T : class
        {
            return this.Channel.GetAttribute(key);
        }
        public bool HasAttribute<T>(AttributeKey<T> key)
            where T : class
        {
            return this.Channel.HasAttribute(key);
        }
        public IFABChannelHandlerContext FireChannelRegistered()
        {
            InvokeChannelRegistered(this.FindContextInbound());
            return this;
        }
        internal static void InvokeChannelRegistered(FABHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelRegistered();
            }
            else
            {
                nextExecutor.Execute(c => ((FABHandlerContext)c).InvokeChannelRegistered(), next);
            }
        }
        void InvokeChannelRegistered()
        {
            if (this.Added)
            {
                try
                {
                    this.Handler.ChannelRegistered(this);
                }
                catch (Exception ex)
                {
                    this.NotifyHandlerException(ex);
                }
            }
            else
            {
                this.FireChannelRegistered();
            }
        }
        public IFABChannelHandlerContext FireChannelUnregistered()
        {
            InvokeChannelUnregistered(this.FindContextInbound());
            return this;
        }
        internal static void InvokeChannelUnregistered(FABHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelUnregistered();
            }
            else
            {
                nextExecutor.Execute(c => ((FABHandlerContext)c).InvokeChannelUnregistered(), next);
            }
        }
        void InvokeChannelUnregistered()
        {
            if (this.Added)
            {
                try
                {
                    this.Handler.ChannelUnregistered(this);
                }
                catch (Exception t)
                {
                    this.NotifyHandlerException(t);
                }
            }
            else
            {
                this.FireChannelUnregistered();
            }
        }
        public IFABChannelHandlerContext FireChannelActive()
        {
            InvokeChannelActive(this.FindContextInbound());
            return this;
        }
        internal static void InvokeChannelActive(FABHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelActive();
            }
            else
            {
                nextExecutor.Execute(c => ((FABHandlerContext)c).InvokeChannelActive(), next);
            }
        }
        void InvokeChannelActive()
        {
            if (this.Added)
            {
                try
                {
                    (this.Handler).ChannelActive(this);
                }
                catch (Exception ex)
                {
                    this.NotifyHandlerException(ex);
                }
            }
            else
            {
                this.FireChannelActive();
            }
        }
        public IFABChannelHandlerContext FireChannelInactive()
        {
            InvokeChannelInactive(this.FindContextInbound());
            return this;
        }
        internal static void InvokeChannelInactive(FABHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelInactive();
            }
            else
            {
                nextExecutor.Execute(c => ((FABHandlerContext)c).InvokeChannelInactive(), next);
            }
        }
        void InvokeChannelInactive()
        {
            if (this.Added)
            {
                try
                {
                    this.Handler.ChannelInactive(this);
                }
                catch (Exception ex)
                {
                    this.NotifyHandlerException(ex);
                }
            }
            else
            {
                this.FireChannelInactive();
            }
        }
        public virtual IFABChannelHandlerContext FireExceptionCaught(Exception cause)
        {
            InvokeExceptionCaught(this.FindContextInbound(), cause);
            return this;
        }
        internal static void InvokeExceptionCaught(FABHandlerContext next, Exception cause)
        {
            Contract.Requires(cause != null);

            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeExceptionCaught(cause);
            }
            else
            {
                try
                {
                    nextExecutor.Execute((c, e) => ((FABHandlerContext)c).InvokeExceptionCaught((Exception)e), next, cause);
                }
                catch (Exception t)
                {
                    throw t;
                }
            }
        }
        void InvokeExceptionCaught(Exception cause)
        {
            if (this.Added)
            {
                try
                {
                    this.Handler.ExceptionCaught(this, cause);
                }
                catch (Exception t)
                {
                    throw t;
                }
            }
            else
            {
                this.FireExceptionCaught(cause);
            }
        }
        public IFABChannelHandlerContext FireUserEventTriggered(object evt)
        {
            InvokeUserEventTriggered(this.FindContextInbound(), evt);
            return this;
        }
        internal static void InvokeUserEventTriggered(FABHandlerContext next, object evt)
        {
            Contract.Requires(evt != null);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeUserEventTriggered(evt);
            }
            else
            {
                nextExecutor.Execute(InvokeUserEventTriggeredAction, next, evt);
            }
        }

        void InvokeUserEventTriggered(object evt)
        {
            if (this.Added)
            {
                try
                {
                    this.Handler.UserEventTriggered(this, evt);
                }
                catch (Exception ex)
                {
                    this.NotifyHandlerException(ex);
                }
            }
            else
            {
                this.FireUserEventTriggered(evt);
            }
        }

        public IFABChannelHandlerContext FireChannelRead(object msg)
        {
            InvokeChannelRead(this.FindContextInbound(), msg);
            return this;
        }

        internal static void InvokeChannelRead(FABHandlerContext next, object msg)
        {
            Contract.Requires(msg != null);

            object m = next.pipeline.Touch(msg, next);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelRead(m);
            }
            else
            {
                nextExecutor.Execute(InvokeChannelReadAction, next, msg);
            }
        }

        void InvokeChannelRead(object msg)
        {
            if (this.Added)
            {
                try
                {
                    this.Handler.ChannelRead(this, msg);
                }
                catch (Exception ex)
                {
                    this.NotifyHandlerException(ex);
                }
            }
            else
            {
                this.FireChannelRead(msg);
            }
        }

        public IFABChannelHandlerContext FireChannelReadComplete()
        {
            InvokeChannelReadComplete(this.FindContextInbound());
            return this;
        }

        internal static void InvokeChannelReadComplete(FABHandlerContext next) {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelReadComplete();
            }
            else
            {
                // todo: consider caching task
                nextExecutor.Execute(InvokeChannelReadCompleteAction, next);
            }
        }

        void InvokeChannelReadComplete()
        {
            if (this.Added)
            {
                try
                {
                    (this.Handler).ChannelReadComplete(this);
                }
                catch (Exception ex)
                {
                    this.NotifyHandlerException(ex);
                }
            }
            else
            {
                this.FireChannelReadComplete();
            }
        }

        public IFABChannelHandlerContext FireChannelWritabilityChanged()
        {
            InvokeChannelWritabilityChanged(this.FindContextInbound());
            return this;
        }

        internal static void InvokeChannelWritabilityChanged(FABHandlerContext next)
        {
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeChannelWritabilityChanged();
            }
            else
            {
                // todo: consider caching task
                nextExecutor.Execute(InvokeChannelWritabilityChangedAction, next);
            }
        }

        void InvokeChannelWritabilityChanged()
        {
            if (this.Added)
            {
                try
                {
                    this.Handler.ChannelWritabilityChanged(this);
                }
                catch (Exception ex)
                {
                    this.NotifyHandlerException(ex);
                }
            }
            else
            {
                this.FireChannelWritabilityChanged();
            }
        }

        public Task BindAsync(EndPoint localAddress)
        {
            Contract.Requires(localAddress != null);
            // todo: check for cancellation
            //if (!validatePromise(ctx, promise, false)) {
            //    // promise cancelled
            //    return;
            //}

            FABHandlerContext next = this.FindContextOutbound();
            IEventExecutor nextExecutor = next.Executor;
            return nextExecutor.InEventLoop 
                ? next.InvokeBindAsync(localAddress) 
                : SafeExecuteOutboundAsync(nextExecutor, () => next.InvokeBindAsync(localAddress));
        }

        Task InvokeBindAsync(EndPoint localAddress)
        {
            if (this.Added)
            {
                try
                {
                    return this.Handler.BindAsync(this, localAddress);
                }
                catch (Exception ex)
                {
                    return ComposeExceptionTask(ex);
                }
            }

            return this.BindAsync(localAddress);
        }

        public Task ConnectAsync(EndPoint remoteAddress) { return this.ConnectAsync(remoteAddress, null); }

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            FABHandlerContext next = this.FindContextOutbound();
            Contract.Requires(remoteAddress != null);
            // todo: check for cancellation

            IEventExecutor nextExecutor = next.Executor;
            return nextExecutor.InEventLoop
                ? next.InvokeConnectAsync(remoteAddress, localAddress)
                : SafeExecuteOutboundAsync(nextExecutor, () => next.InvokeConnectAsync(remoteAddress, localAddress));
        }

        Task InvokeConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            if (this.Added)
            {
                try
                {
                    return this.Handler.ConnectAsync(this, remoteAddress, localAddress);
                }
                catch (Exception ex)
                {
                    return ComposeExceptionTask(ex);
                }
            }

            return this.ConnectAsync(remoteAddress, localAddress);
        }

        public Task DisconnectAsync()
        {
            if (!this.Channel.Metadata.HasDisconnect)
            {
                return this.CloseAsync();
            }

            // todo: check for cancellation
            FABHandlerContext next = this.FindContextOutbound();
            IEventExecutor nextExecutor = next.Executor;
            return nextExecutor.InEventLoop
                ? next.InvokeDisconnectAsync()
                : SafeExecuteOutboundAsync(nextExecutor, () => next.InvokeDisconnectAsync());
        }

        Task InvokeDisconnectAsync()
        {
            if (this.Added)
            {
                try
                {
                    return this.Handler.DisconnectAsync(this);
                }
                catch (Exception ex)
                {
                    return ComposeExceptionTask(ex);
                }
            }
            return this.DisconnectAsync();
        }

        public Task CloseAsync()
        {
            // todo: check for cancellation
            FABHandlerContext next = this.FindContextOutbound();
            IEventExecutor nextExecutor = next.Executor;
            return nextExecutor.InEventLoop
                ? next.InvokeCloseAsync()
                : SafeExecuteOutboundAsync(nextExecutor, () => next.InvokeCloseAsync());
        }

        Task InvokeCloseAsync()
        {
            if (this.Added)
            {
                try
                {
                    return this.Handler.CloseAsync(this);
                }
                catch (Exception ex)
                {
                    return ComposeExceptionTask(ex);
                }
            }
            return this.CloseAsync();
        }

        public Task DeregisterAsync()
        {
            // todo: check for cancellation
            FABHandlerContext next = this.FindContextOutbound();
            IEventExecutor nextExecutor = next.Executor;
            return nextExecutor.InEventLoop
                ? next.InvokeDeregisterAsync()
                : SafeExecuteOutboundAsync(nextExecutor, () => next.InvokeDeregisterAsync());
        }

        Task InvokeDeregisterAsync()
        {
            if (this.Added)
            {
                try
                {
                    return this.Handler.DeregisterAsync(this);
                }
                catch (Exception ex)
                {
                    return ComposeExceptionTask(ex);
                }
            }
            return this.DeregisterAsync();
        }

        public IFABChannelHandlerContext Read()
        {
            FABHandlerContext next = this.FindContextOutbound();
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeRead();
            }
            else
            {
                // todo: consider caching task
                nextExecutor.Execute(InvokeReadAction, next);
            }
            return this;
        }

        void InvokeRead()
        {
            if (this.Added)
            {
                try
                {
                    this.Handler.Read(this);
                }
                catch (Exception ex)
                {
                    this.NotifyHandlerException(ex);
                }
            }
            else
            {
                this.Read();
            }
        }

        public Task WriteAsync(object msg)
        {
            Contract.Requires(msg != null);
            // todo: check for cancellation
            return this.WriteAsync(msg, false);
        }

        Task InvokeWriteAsync(object msg) { return this.Added ? this.InvokeWriteAsync0(msg) : this.WriteAsync(msg);}

        Task InvokeWriteAsync0(object msg)
        {
            try
            {
                return this.Handler.WriteAsync(this, msg);
            }
            catch (Exception ex)
            {
                return ComposeExceptionTask(ex);
            }
        }

        public IFABChannelHandlerContext Flush()
        {
            FABHandlerContext next = this.FindContextOutbound();
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                next.InvokeFlush();
            }
            else
            {
                nextExecutor.Execute(InvokeFlushAction, next);
            }
            return this;
        }

        void InvokeFlush()
        {
            if (this.Added)
            {
                this.InvokeFlush0();
            }
            else
            {
                this.Flush();
            }
        }

        void InvokeFlush0()
        {
            try
            {
                this.Handler.Flush(this);
            }
            catch (Exception ex)
            {
                this.NotifyHandlerException(ex);
            }
        }

        public Task WriteAndFlushAsync(object message)
        {
            Contract.Requires(message != null);
            // todo: check for cancellation

            return this.WriteAsync(message, true);
        }

        Task InvokeWriteAndFlushAsync(object msg)
        {
            if (this.Added)
            {
                Task task = this.InvokeWriteAsync0(msg);
                this.InvokeFlush0();
                return task;
            }
            return this.WriteAndFlushAsync(msg);
        }

        Task WriteAsync(object msg, bool flush)
        {
            FABHandlerContext next = this.FindContextOutbound();
            object m = this.pipeline.Touch(msg, next);
            IEventExecutor nextExecutor = next.Executor;
            if (nextExecutor.InEventLoop)
            {
                return flush
                    ? next.InvokeWriteAndFlushAsync(m)
                    : next.InvokeWriteAsync(m);
            }
            else
            {
                var promise = new TaskCompletionSource();
                AbstractWriteTask task = flush 
                    ? WriteAndFlushTask.NewInstance(next, m, promise)
                    : (AbstractWriteTask)WriteTask.NewInstance(next, m, promise);
                SafeExecuteOutbound(nextExecutor, task, promise, msg);
                return promise.Task;
            }
        }

        void NotifyHandlerException(Exception cause)
        {
            if (InExceptionCaught(cause))
            {
                throw cause;
            }
            this.InvokeExceptionCaught(cause);
        }

        static Task ComposeExceptionTask(Exception cause) {return TaskEx.FromException(cause);}

        static string ExceptionCaughtMethodName { get { return "ExceptionCaught";}}

        static bool InExceptionCaught(Exception cause) { return cause.StackTrace.IndexOf("." + ExceptionCaughtMethodName + "(", StringComparison.Ordinal) >= 0;}

        FABHandlerContext FindContextInbound()
        {
            FABHandlerContext ctx = this;
            do
            {
                ctx = ctx.Next;
            }
            while ((ctx.SkipPropagationFlags & SkipFlags.Inbound) == SkipFlags.Inbound);
            return ctx;
        }

        FABHandlerContext FindContextOutbound()
        {
            FABHandlerContext ctx = this;
            do
            {
                ctx = ctx.Prev;
            }
            while ((ctx.SkipPropagationFlags & SkipFlags.Outbound) == SkipFlags.Outbound);
            return ctx;
        }

        static Task SafeExecuteOutboundAsync(IEventExecutor executor, Func<Task> function)
        {
            var promise = new TaskCompletionSource();
            try
            {
                executor.Execute((p, func) => ((Func<Task>)func)().LinkOutcome((TaskCompletionSource)p), promise, function);
            }
            catch (Exception cause)
            {
                promise.TrySetException(cause);
            }
            return promise.Task;
        }

        static void SafeExecuteOutbound(IEventExecutor executor, IRunnable task, TaskCompletionSource promise, object msg)
        {
            try
            {
                executor.Execute(task);
            }
            catch (Exception cause)
            {
                try
                {
                    promise.TrySetException(cause);
                }
                finally
                {
                    ReferenceCountUtil.Release(msg);
                }
            }
        }

        public string ToHintString() { return string.Format("\'{0}\' will handle the message from this point.", this.Name);}

        public override string ToString() { return string.Format("({0}, {1})", this.Name, this.Channel);}
        abstract class AbstractWriteTask : IRunnable
        {
            static readonly bool EstimateTaskSizeOnSubmit =
                SystemPropertyUtil.GetBoolean("io.netty.transport.estimateSizeOnSubmit", true);

            // Assuming a 64-bit .NET VM, 16 bytes object header, 4 reference fields and 2 int field
            static readonly int WriteTaskOverhead =
                SystemPropertyUtil.GetInt("io.netty.transport.writeTaskSizeOverhead", 56);

            ThreadLocalPool.Handle handle;
            FABHandlerContext ctx;
            object msg;
            TaskCompletionSource promise;
            int size;

            protected static void Init(AbstractWriteTask task, FABHandlerContext ctx, object msg, TaskCompletionSource promise)
            {
                task.ctx = ctx;
                task.msg = msg;
                task.promise = promise;

                if (EstimateTaskSizeOnSubmit)
                {
                    FABChannelOutboundBuffer buffer = ctx.Channel.Unsafe.OutboundBuffer;

                    // Check for null as it may be set to null if the channel is closed already
                    if (buffer != null)
                    {
                        task.size = ctx.pipeline.EstimatorHandle.Size(msg) + WriteTaskOverhead;
                        buffer.IncrementPendingOutboundBytes(task.size);
                    }
                    else
                    {
                        task.size = 0;
                    }
                }
                else
                {
                    task.size = 0;
                }
            }

            protected AbstractWriteTask(ThreadLocalPool.Handle handle)
            {
                this.handle = handle;
            }

            public void Run()
            {
                try
                {
                    FABChannelOutboundBuffer buffer = this.ctx.Channel.Unsafe.OutboundBuffer;
                    // Check for null as it may be set to null if the channel is closed already
                    if (EstimateTaskSizeOnSubmit)
                    {
                        if(buffer != null)
                            buffer.DecrementPendingOutboundBytes(this.size);
                    }
                    this.WriteAsync(this.ctx, this.msg).LinkOutcome(this.promise);
                }
                finally
                {
                    // Set to null so the GC can collect them directly
                    this.ctx = null;
                    this.msg = null;
                    this.promise = null;
                    this.handle.Release(this);
                }
            }

            protected virtual Task WriteAsync(FABHandlerContext ctx, object msg) { return ctx.InvokeWriteAsync(msg);}
        }
        sealed class WriteTask : AbstractWriteTask {

            static readonly ThreadLocalPool<WriteTask> Recycler = new ThreadLocalPool<WriteTask>(handle => new WriteTask(handle));

            public static WriteTask NewInstance(FABHandlerContext ctx, object msg, TaskCompletionSource promise)
            {
                WriteTask task = Recycler.Take();
                Init(task, ctx, msg, promise);
                return task;
            }

            WriteTask(ThreadLocalPool.Handle handle)
                : base(handle)
            {
            }
        }
        sealed class WriteAndFlushTask : AbstractWriteTask
    {

            static readonly ThreadLocalPool<WriteAndFlushTask> Recycler = new ThreadLocalPool<WriteAndFlushTask>(handle => new WriteAndFlushTask(handle));

            public static WriteAndFlushTask NewInstance(
                    FABHandlerContext ctx, object msg,  TaskCompletionSource promise) {
                WriteAndFlushTask task = Recycler.Take();
                Init(task, ctx, msg, promise);
                return task;
            }

            WriteAndFlushTask(ThreadLocalPool.Handle handle)
                : base(handle)
            {
            }

            protected override Task WriteAsync(FABHandlerContext ctx, object msg)
            {
                Task result = base.WriteAsync(ctx, msg);
                ctx.InvokeFlush();
                return result;
            }
        }
        string IFABChannelHandlerContext.Name
        {
            get { return this.Name; }
        }
    }
}