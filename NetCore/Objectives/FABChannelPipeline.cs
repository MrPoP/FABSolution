using DotNetty.Common;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABChannelPipeline : IFABChannelPipeline
    {
        static readonly Action<object, object> CallHandlerAddedAction = (self, ctx) => ((FABChannelPipeline)self).CallHandlerAdded0((FABHandlerContext)ctx);
        static readonly NameCachesLocal NameCaches = new NameCachesLocal();
        class NameCachesLocal : FastThreadLocal<ConditionalWeakTable<Type, string>>
        {
            protected override ConditionalWeakTable<Type, string> GetInitialValue() { return new ConditionalWeakTable<Type, string>(); }
        }
        readonly FABChannel channel;
        readonly FABHandlerContext head;
        readonly FABHandlerContext tail;
        readonly bool touch = ResourceLeakDetector.Enabled;
        private Dictionary<IEventExecutorGroup, IEventExecutor> childExecutors;
        private IMessageSizeEstimatorHandle estimatorHandle;
        PendingHandlerCallback pendingHandlerCallbackHead;
        bool registered;
        public FABChannelPipeline(FABChannel channel)
        {
            Contract.Requires(channel != null);

            this.channel = channel;

            this.tail = new TailContext(this);
            this.head = new HeadContext(this);

            this.head.Next = this.tail;
            this.tail.Prev = this.head;
        }
        internal IMessageSizeEstimatorHandle EstimatorHandle { get { if(this.estimatorHandle == null){this.estimatorHandle = this.channel.Configuration.MessageSizeEstimator.NewHandle();} return this.estimatorHandle;}}

        internal object Touch(object msg, FABHandlerContext next) { return this.touch == true ? ReferenceCountUtil.Touch(msg, next) : msg;}

        public IFABChannel Channel { get { return this.channel; } }
        IEnumerator<IFABChannelHandler> IEnumerable<IFABChannelHandler>.GetEnumerator()
        {
            FABHandlerContext current = this.head;
            while (current != null)
            {
                yield return current.Handler;
                current = current.Next;
            }
        }
        FABHandlerContext NewContext(IEventExecutorGroup group, string name, IFABChannelHandler handler) { return new SealedFABHandlerContext(this, this.GetChildExecutor(group), name, handler); }

        FABHandlerContext NewContext(IEventExecutor executor, string name, IFABChannelHandler handler) { return new SealedFABHandlerContext(this, executor, name, handler); }
        IEventExecutor GetChildExecutor(IEventExecutorGroup group)
        {
            if (group == null)
            {
                return null;
            }
            // Use size of 4 as most people only use one extra EventExecutor.
            Dictionary<IEventExecutorGroup, IEventExecutor> executorMap = this.childExecutors
                ?? (this.childExecutors = new Dictionary<IEventExecutorGroup, IEventExecutor>(4, ReferenceEqualityComparer.Default));

            // Pin one of the child executors once and remember it so that the same child executor
            // is used to fire events for the same channel.
            IEventExecutor childExecutor;
            if (!executorMap.TryGetValue(group, out childExecutor))
            {
                childExecutor = group.GetNext();
                executorMap[group] = childExecutor;
            }
            return childExecutor;
        }
        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable<IFABChannelHandler>)this).GetEnumerator(); }
        public IFABChannelPipeline AddFirst(string name, IFABChannelHandler handler) { return this.AddFirst(null, name, handler); }
        public IFABChannelPipeline AddFirst(IEventExecutorGroup group, string name, IFABChannelHandler handler)
        {
            Contract.Requires(handler != null);

            FABHandlerContext newCtx;
            lock (this)
            {
                CheckMultiplicity(handler);

                newCtx = this.NewContext(group, this.FilterName(name, handler), handler);
                IEventExecutor executor = this.ExecutorSafe(newCtx.executor);

                this.AddFirst0(newCtx);

                // If the executor is null it means that the channel was not registered on an eventloop yet.
                // In this case we add the context to the pipeline and add a task that will call
                // ChannelHandler.handlerAdded(...) once the channel is registered.
                if (executor == null)
                {
                    this.CallHandlerCallbackLater(newCtx, true);
                    return this;
                }

                if (!executor.InEventLoop)
                {
                    executor.Execute(CallHandlerAddedAction, this, newCtx);
                    return this;
                }
            }

            this.CallHandlerAdded0(newCtx);
            return this;
        }
        void AddFirst0(FABHandlerContext newCtx)
        {
            FABHandlerContext nextCtx = this.head.Next;
            newCtx.Prev = this.head;
            newCtx.Next = nextCtx;
            this.head.Next = newCtx;
            nextCtx.Prev = newCtx;
        }
        public IFABChannelPipeline AddLast(string name, IFABChannelHandler handler) { return this.AddLast(null, name, handler); }

        public IFABChannelPipeline AddLast(IEventExecutorGroup group, string name, IFABChannelHandler handler)
        {
            Contract.Requires(handler != null);

            FABHandlerContext newCtx;
            lock (this)
            {
                CheckMultiplicity(handler);

                newCtx = this.NewContext(group, this.FilterName(name, handler), handler);
                IEventExecutor executor = this.ExecutorSafe(newCtx.executor);

                this.AddLast0(newCtx);

                // If the executor is null it means that the channel was not registered on an eventloop yet.
                // In this case we add the context to the pipeline and add a task that will call
                // ChannelHandler.handlerAdded(...) once the channel is registered.
                if (executor == null)
                {
                    this.CallHandlerCallbackLater(newCtx, true);
                    return this;
                }

                if (!executor.InEventLoop)
                {
                    executor.Execute(CallHandlerAddedAction, this, newCtx);
                    return this;
                }
            }
            this.CallHandlerAdded0(newCtx);
            return this;
        }

        void AddLast0(FABHandlerContext newCtx)
        {
            FABHandlerContext prev = this.tail.Prev;
            newCtx.Prev = prev;
            newCtx.Next = this.tail;
            prev.Next = newCtx;
            this.tail.Prev = newCtx;
        }
        public IFABChannelPipeline AddBefore(string baseName, string name, IFABChannelHandler handler) { return  this.AddBefore(null, baseName, name, handler);}

        public IFABChannelPipeline AddBefore(IEventExecutorGroup group, string baseName, string name, IFABChannelHandler handler)
        {
            Contract.Requires(handler != null);

            FABHandlerContext newCtx;
            lock (this)
            {
                CheckMultiplicity(handler);
                FABHandlerContext ctx = this.GetContextOrThrow(baseName);

                newCtx = this.NewContext(group, this.FilterName(name, handler), handler);
                IEventExecutor executor = this.ExecutorSafe(newCtx.executor);

                AddBefore0(ctx, newCtx);

                // If the executor is null it means that the channel was not registered on an eventloop yet.
                // In this case we add the context to the pipeline and add a task that will call
                // ChannelHandler.handlerAdded(...) once the channel is registered.
                if (executor == null)
                {
                    this.CallHandlerCallbackLater(newCtx, true);
                    return this;
                }

                if (!executor.InEventLoop)
                {
                    executor.Execute(CallHandlerAddedAction, this, newCtx);
                    return this;
                }
            }
            this.CallHandlerAdded0(newCtx);
            return this;
        }

        static void AddBefore0(FABHandlerContext ctx, FABHandlerContext newCtx)
        {
            newCtx.Prev = ctx.Prev;
            newCtx.Next = ctx;
            ctx.Prev.Next = newCtx;
            ctx.Prev = newCtx;
        }

        public IFABChannelPipeline AddAfter(string baseName, string name, IFABChannelHandler handler) { return this.AddAfter(null, baseName, name, handler); }

        public IFABChannelPipeline AddAfter(IEventExecutorGroup group, string baseName, string name, IFABChannelHandler handler)
        {
            Contract.Requires(handler != null);

            FABHandlerContext newCtx;

            lock (this)
            {
                CheckMultiplicity(handler);
                FABHandlerContext ctx = this.GetContextOrThrow(baseName);

                newCtx = this.NewContext(group, this.FilterName(name, handler), handler);
                IEventExecutor executor = this.ExecutorSafe(newCtx.executor);

                AddAfter0(ctx, newCtx);

                // If the executor is null it means that the channel was not registered on an eventloop yet.
                // In this case we remove the context from the pipeline and add a task that will call
                // ChannelHandler.handlerRemoved(...) once the channel is registered.
                if (executor == null)
                {
                    this.CallHandlerCallbackLater(newCtx, true);
                    return this;
                }

                if (!executor.InEventLoop)
                {
                    executor.Execute(CallHandlerAddedAction, this, newCtx);
                    return this;
                }
            }
            this.CallHandlerAdded0(newCtx);
            return this;
        }

        static void AddAfter0(FABHandlerContext ctx, FABHandlerContext newCtx)
        {
            newCtx.Prev = ctx;
            newCtx.Next = ctx.Next;
            ctx.Next.Prev = newCtx;
            ctx.Next = newCtx;
        }

        public IFABChannelPipeline AddFirst(params IFABChannelHandler[] handlers) { return this.AddFirst(null, handlers); }

        public IFABChannelPipeline AddFirst(IEventExecutorGroup group, params IFABChannelHandler[] handlers)
        {
            Contract.Requires(handlers != null);

            for (int i = handlers.Length - 1; i >= 0; i--)
            {
                IFABChannelHandler h = handlers[i];
                this.AddFirst(group, (string)null, h);
            }

            return this;
        }
        public IFABChannelPipeline AddLast(params IFABChannelHandler[] handlers) { return this.AddLast(null, handlers); }

        public IFABChannelPipeline AddLast(IEventExecutorGroup group, params IFABChannelHandler[] handlers)
        {
            foreach (IFABChannelHandler h in handlers)
            {
                this.AddLast(group, (string)null, h);
            }
            return this;
        }
        string GenerateName(IFABChannelHandler handler)
        {
            ConditionalWeakTable<Type, string> cache = NameCaches.Value;
            Type handlerType = handler.GetType();
            string name = cache.GetValue(handlerType, t => GenerateName0(t));

            // It's not very likely for a user to put more than one handler of the same type, but make sure to avoid
            // any name conflicts.  Note that we don't cache the names generated here.
            if (this.Context0(name) != null)
            {
                string baseName = name.Substring(0, name.Length - 1); // Strip the trailing '0'.
                for (int i = 1; ; i++)
                {
                    string newName = baseName + i;
                    if (this.Context0(newName) == null)
                    {
                        name = newName;
                        break;
                    }
                }
            }
            return name;
        }
        static string GenerateName0(Type handlerType) { return StringUtil.SimpleClassName(handlerType) + "#0"; }

        public IFABChannelPipeline Remove(IFABChannelHandler handler)
        {
            this.Remove(this.GetContextOrThrow(handler));
            return this;
        }
        public IFABChannelHandler Remove(string name) { return this.Remove(this.GetContextOrThrow(name)).Handler;}

        public T Remove<T>() where T : class, IFABChannelHandler { return (T)this.Remove(this.GetContextOrThrow<T>()).Handler;}

        FABHandlerContext Remove(FABHandlerContext ctx)
        {
            Contract.Assert(ctx != this.head && ctx != this.tail);

            lock (this)
            {
                IEventExecutor executor = this.ExecutorSafe(ctx.executor);

                Remove0(ctx);

                // If the executor is null it means that the channel was not registered on an eventloop yet.
                // In this case we remove the context from the pipeline and add a task that will call
                // ChannelHandler.handlerRemoved(...) once the channel is registered.
                if (executor == null)
                {
                    this.CallHandlerCallbackLater(ctx, false);
                    return ctx;
                }
                if (!executor.InEventLoop)
                {
                    executor.Execute((s, c) => ((FABChannelPipeline)s).CallHandlerRemoved0((FABHandlerContext)c), this, ctx);
                    return ctx;
                }
            }
            this.CallHandlerRemoved0(ctx);
            return ctx;
        }

        static void Remove0(FABHandlerContext context)
        {
            FABHandlerContext prev = context.Prev;
            FABHandlerContext next = context.Next;
            prev.Next = next;
            next.Prev = prev;
        }

        public IFABChannelHandler RemoveFirst()
        {
            if (this.head.Next == this.tail)
            {
                throw new InvalidOperationException("Pipeline is empty.");
            }
            return this.Remove(this.head.Next).Handler;
        }

        public IFABChannelHandler RemoveLast()
        {
            if (this.head.Next == this.tail)
            {
                throw new InvalidOperationException("Pipeline is empty.");
            }
            return this.Remove(this.tail.Prev).Handler;
        }

        public IFABChannelPipeline Replace(IFABChannelHandler oldHandler, string newName, IFABChannelHandler newHandler)
        {
            this.Replace(this.GetContextOrThrow(oldHandler), newName, newHandler);
            return this;
        }

        public IFABChannelHandler Replace(string oldName, string newName, IFABChannelHandler newHandler) { return this.Replace(this.GetContextOrThrow(oldName), newName, newHandler);}

        public T Replace<T>(string newName, IFABChannelHandler newHandler)
            where T : class, IFABChannelHandler { return (T)this.Replace(this.GetContextOrThrow<T>(), newName, newHandler);}

        IFABChannelHandler Replace(FABHandlerContext ctx, string newName, IFABChannelHandler newHandler)
        {
            Contract.Requires(newHandler != null);
            Contract.Assert(ctx != this.head && ctx != this.tail);

            FABHandlerContext newCtx;
            lock (this)
            {
                CheckMultiplicity(newHandler);
                if (newName == null)
                {
                    newName = this.GenerateName(newHandler);
                }
                else
                {
                    bool sameName = ctx.Name.Equals(newName, StringComparison.Ordinal);
                    if (!sameName)
                    {
                        this.CheckDuplicateName(newName);
                    }
                }

                newCtx = this.NewContext(ctx.executor, newName, newHandler);
                IEventExecutor executor = this.ExecutorSafe(ctx.executor);

                Replace0(ctx, newCtx);

                // If the executor is null it means that the channel was not registered on an event loop yet.
                // In this case we replace the context in the pipeline
                // and add a task that will signal handler it was added or removed
                // once the channel is registered.
                if (executor == null)
                {
                    this.CallHandlerCallbackLater(newCtx, true);
                    this.CallHandlerCallbackLater(ctx, false);
                    return ctx.Handler;
                }

                if (!executor.InEventLoop)
                {
                    executor.Execute(() =>
                    {
                        // Indicate new handler was added first (i.e. before old handler removed)
                        // because "removed" will trigger ChannelRead() or Flush() on newHandler and
                        // those event handlers must be called after handler was signaled "added".
                        this.CallHandlerAdded0(newCtx);
                        this.CallHandlerRemoved0(ctx);
                    });
                    return ctx.Handler;
                }
            }
            // Indicate new handler was added first (i.e. before old handler removed)
            // because "removed" will trigger ChannelRead() or Flush() on newHandler and
            // those event handlers must be called after handler was signaled "added".
            this.CallHandlerAdded0(newCtx);
            this.CallHandlerRemoved0(ctx);
            return ctx.Handler;
        }

        static void Replace0(FABHandlerContext oldCtx, FABHandlerContext newCtx)
        {
            FABHandlerContext prev = oldCtx.Prev;
            FABHandlerContext next = oldCtx.Next;
            newCtx.Prev = prev;
            newCtx.Next = next;

            // Finish the replacement of oldCtx with newCtx in the linked list.
            // Note that this doesn't mean events will be sent to the new handler immediately
            // because we are currently at the event handler thread and no more than one handler methods can be invoked
            // at the same time (we ensured that in replace().)
            prev.Next = newCtx;
            next.Prev = newCtx;

            // update the reference to the replacement so forward of buffered content will work correctly
            oldCtx.Prev = newCtx;
            oldCtx.Next = newCtx;
        }

        static void CheckMultiplicity(IFABChannelHandler handler)
        {
            var adapter = handler as FABChannelAdapter;
            if (adapter != null)
            {
                FABChannelAdapter h = adapter;
                if (!h.IsSharable && h.Added)
                {
                    throw new FABChannelPipelineException(
                        h.GetType().Name + " is not a @Sharable handler, so can't be added or removed multiple times.");
                }
                h.Added = true;
            }
        }

        void CallHandlerAdded0(FABHandlerContext ctx)
        {
            try
            {
                ctx.Handler.HandlerAdded(ctx);
                ctx.SetAdded();
            }
            catch (Exception ex)
            {
                bool removed = false;
                try
                {
                    Remove0(ctx);
                    try
                    {
                        ctx.Handler.HandlerRemoved(ctx);
                    }
                    finally
                    {
                        ctx.SetRemoved();
                    }
                    removed = true;
                }
                catch (Exception ex2)
                {
                    throw ex2;
                }

                if (removed)
                {
                    this.FireExceptionCaught(new FABChannelPipelineException("{0}.HandlerAdded() has thrown an exception; {1} removed.", ctx.Handler.GetType().Name, ex));
                }
                else
                {
                    this.FireExceptionCaught(new FABChannelPipelineException("{0}.HandlerAdded() has thrown an exception; {1} also failed to remove.", ctx.Handler.GetType().Name, ex));
                }
            }
        }

        void CallHandlerRemoved0(FABHandlerContext ctx)
        {
            // Notify the complete removal.
            try
            {
                try
                {
                    ctx.Handler.HandlerRemoved(ctx);
                }
                finally
                {
                    ctx.SetRemoved();
                }
            }
            catch (Exception ex)
            {
                this.FireExceptionCaught(new FABChannelPipelineException("{0}.HandlerRemoved() has thrown an exception. {1}", ctx.Handler.GetType().Name, ex));
            }
        }
        public IFABChannelHandler First() { return this.FirstContext() != null ? this.FirstContext().Handler : null; }
        public IFABChannelHandlerContext FirstContext()
        {
            FABHandlerContext first = this.head.Next;
            return first == this.tail ? null : first;
        }
        public IFABChannelHandler Last() { return this.LastContext() != null ? this.LastContext().Handler : null; }
        public IFABChannelHandlerContext LastContext()
        {
            FABHandlerContext last = this.tail.Prev;
            return last == this.head ? null : last;
        }
        public IFABChannelHandler Get(string name) { return this.Context(name) != null ?this.Context(name).Handler : null;}

        public T Get<T>() where T : class, IFABChannelHandler { return (T)(this.Context<T>() != null ? this.Context<T>().Handler : default(T));}

        public IFABChannelHandlerContext Context(string name)
        {
            Contract.Requires(name != null);

            return this.Context0(name);
        }
        public IFABChannelHandlerContext Context(IFABChannelHandler handler)
        {
            Contract.Requires(handler != null);

            FABHandlerContext ctx = this.head.Next;
            while (true)
            {
                if (ctx == null)
                {
                    return null;
                }

                if (ctx.Handler == handler)
                {
                    return ctx;
                }

                ctx = ctx.Next;
            }
        }
        public IFABChannelHandlerContext Context<T>() where T : class, IFABChannelHandler
        {
            FABHandlerContext ctx = this.head.Next;
            while (true)
            {
                if (ctx == null)
                {
                    return null;
                }
                if (ctx.Handler is T)
                {
                    return ctx;
                }
                ctx = ctx.Next;
            }
        }
        public sealed override string ToString()
        {
            StringBuilder buf = new StringBuilder()
                .Append(this.GetType().Name)
                .Append('{');
            FABHandlerContext ctx = this.head.Next;
            while (true)
            {
                if (ctx == this.tail)
                {
                    break;
                }

                buf.Append('(')
                    .Append(ctx.Name)
                    .Append(" = ")
                    .Append(ctx.Handler.GetType().Name)
                    .Append(')');

                ctx = ctx.Next;
                if (ctx == this.tail)
                {
                    break;
                }

                buf.Append(", ");
            }
            buf.Append('}');
            return buf.ToString();
        }
        public IFABChannelPipeline FireChannelRegistered()
        {
            FABHandlerContext.InvokeChannelRegistered(this.head);
            return this;
        }
        public IFABChannelPipeline FireChannelUnregistered()
        {
            FABHandlerContext.InvokeChannelUnregistered(this.head);
            return this;
        }
        void Destroy()
        {
            lock (this)
            {
                this.DestroyUp(this.head.Next, false);
            }
        }
        void DestroyUp(FABHandlerContext ctx, bool inEventLoop)
        {
            XThread currentThread = XThread.CurrentThread;
            FABHandlerContext tailContext = this.tail;
            while (true)
            {
                if (ctx == tailContext)
                {
                    this.DestroyDown(currentThread, tailContext.Prev, inEventLoop);
                    break;
                }

                IEventExecutor executor = ctx.Executor;
                if (!inEventLoop && !executor.IsInEventLoop(currentThread))
                {
                    executor.Execute((self, c) => ((FABChannelPipeline)self).DestroyUp((FABHandlerContext)c, true), this, ctx);
                    break;
                }

                ctx = ctx.Next;
                inEventLoop = false;
            }
        }
        void DestroyDown(XThread currentThread, FABHandlerContext ctx, bool inEventLoop)
        {
            // We have reached at tail; now traverse backwards.
            FABHandlerContext headContext = this.head;
            while (true)
            {
                if (ctx == headContext)
                {
                    break;
                }

                IEventExecutor executor = ctx.Executor;
                if (inEventLoop || executor.IsInEventLoop(currentThread))
                {
                    lock (this)
                    {
                        Remove0(ctx);
                        this.CallHandlerRemoved0(ctx);
                    }
                }
                else
                {
                    executor.Execute((self, c) => ((FABChannelPipeline)self).DestroyDown(XThread.CurrentThread, (FABHandlerContext)c, true), this, ctx);
                    break;
                }

                ctx = ctx.Prev;
                inEventLoop = false;
            }
        }
        public IFABChannelPipeline FireChannelActive()
        {
            this.head.FireChannelActive();

            if (this.channel.Configuration.AutoRead)
            {
                this.channel.Read();
            }

            return this;
        }

        public IFABChannelPipeline FireChannelInactive()
        {
            this.head.FireChannelInactive();
            return this;
        }

        public IFABChannelPipeline FireExceptionCaught(Exception cause)
        {
            this.head.FireExceptionCaught(cause);
            return this;
        }

        public IFABChannelPipeline FireUserEventTriggered(object evt)
        {
            this.head.FireUserEventTriggered(evt);
            return this;
        }

        public IFABChannelPipeline FireChannelRead(object msg)
        {
            this.head.FireChannelRead(msg);
            return this;
        }

        public IFABChannelPipeline FireChannelReadComplete()
        {
            this.head.FireChannelReadComplete();
            if (this.channel.Configuration.AutoRead)
            {
                this.Read();
            }
            return this;
        }
        public Task BindAsync(EndPoint localAddress) { return this.tail.BindAsync(localAddress);}

        public Task ConnectAsync(EndPoint remoteAddress) { return this.tail.ConnectAsync(remoteAddress);}

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress) { return this.tail.ConnectAsync(remoteAddress, localAddress);}

        public Task DisconnectAsync() { return this.tail.DisconnectAsync();}

        public Task CloseAsync() { return this.tail.CloseAsync();}

        public Task DeregisterAsync() { return this.tail.DeregisterAsync();}

        public IFABChannelPipeline Read()
        {
            this.tail.Read();
            return this;
        }

        public Task WriteAsync(object msg) { return this.tail.WriteAsync(msg);}

        public IFABChannelPipeline Flush()
        {
            this.tail.Flush();
            return this;
        }

        public Task WriteAndFlushAsync(object msg) { return this.tail.WriteAndFlushAsync(msg); }

        public IFABChannelPipeline FireChannelWritabilityChanged()
        {
            this.head.FireChannelWritabilityChanged();
            return this;
        }
        string FilterName(string name, IFABChannelHandler handler)
        {
            if (name == null)
            {
                return this.GenerateName(handler);
            }
            this.CheckDuplicateName(name);
            return name;
        }

        void CheckDuplicateName(string name)
        {
            if (this.Context0(name) != null)
            {
                throw new ArgumentException("Duplicate handler name: " + name, FABExtensions.nameof(name));
            }
        }

        FABHandlerContext Context0(string name)
        {
            FABHandlerContext context = this.head.Next;
            while (context != this.tail)
            {
                if (context.Name.Equals(name, StringComparison.Ordinal))
                {
                    return context;
                }
                context = context.Next;
            }
            return null;
        }

        FABHandlerContext GetContextOrThrow(string name)
        {
            var ctx = (FABHandlerContext)this.Context(name);
            if (ctx == null)
            {
                throw new FABChannelPipelineException("Handler of type `{0}` could not be found in the pipeline.", "name");
            }

            return ctx;
        }

        FABHandlerContext GetContextOrThrow(IFABChannelHandler handler)
        {
            var ctx = (FABHandlerContext)this.Context(handler);
            if (ctx == null)
            {
                throw new FABChannelPipelineException("Handler of type `{0}` could not be found in the pipeline.", typeof(IFABChannelHandler).Name);
            }

            return ctx;
        }

        FABHandlerContext GetContextOrThrow<T>() where T : class, IFABChannelHandler
        {
            var ctx = (FABHandlerContext)this.Context<T>();
            if (ctx == null)
            {
                throw new FABChannelPipelineException("Handler of type `{0}` could not be found in the pipeline.", typeof(T).Name);
            }

            return ctx;
        }

        void CallHandlerAddedForAllHandlers()
        {
            PendingHandlerCallback pendingHandlerCallbackHead;
            lock (this)
            {
                Contract.Assert(!this.registered);

                // This Channel itself was registered.
                this.registered = true;

                pendingHandlerCallbackHead = this.pendingHandlerCallbackHead;
                // Null out so it can be GC'ed.
                this.pendingHandlerCallbackHead = null;
            }

            // This must happen outside of the synchronized(...) block as otherwise handlerAdded(...) may be called while
            // holding the lock and so produce a deadlock if handlerAdded(...) will try to add another handler from outside
            // the EventLoop.
            PendingHandlerCallback task = pendingHandlerCallbackHead;
            while (task != null)
            {
                task.Execute();
                task = task.Next;
            }
        }

        void CallHandlerCallbackLater(FABHandlerContext ctx, bool added)
        {
            Contract.Assert(!this.registered);

            PendingHandlerCallback task = added ? (PendingHandlerCallback)new PendingHandlerAddedTask(this, ctx) : new PendingHandlerRemovedTask(this, ctx);
            PendingHandlerCallback pending = this.pendingHandlerCallbackHead;
            if (pending == null)
            {
                this.pendingHandlerCallbackHead = task;
            }
            else
            {
                // Find the tail of the linked-list.
                while (pending.Next != null)
                {
                    pending = pending.Next;
                }
                pending.Next = task;
            }
        }

        IEventExecutor ExecutorSafe(IEventExecutor eventExecutor) { return eventExecutor != null ? (this.channel.Registered || this.registered ? this.channel.EventLoop : null) : null;}

        /// <summary>
        /// Called once an <see cref="Exception" /> hits the end of the <see cref="IFABChannelPipeline" /> without being
        /// handled by the user in <see cref="IFABChannelHandler.ExceptionCaught(IFABChannelHandlerContext, Exception)" />.
        /// </summary>
        protected virtual void OnUnhandledInboundException(Exception cause)
        {
            ReferenceCountUtil.Release(cause);
        }

        /// <summary>
        /// Called once a message hits the end of the <see cref="IFABChannelPipeline" /> without being handled by the user
        /// in <see cref="IFABChannelHandler.ChannelRead(IFABChannelHandlerContext, object)" />. This method is responsible
        /// for calling <see cref="ReferenceCountUtil.Release(object)" /> on the given msg at some point.
        /// </summary>
        protected virtual void OnUnhandledInboundMessage(object msg)
        {
           ReferenceCountUtil.Release(msg);
        }
        sealed class TailContext : FABHandlerContext, IFABChannelHandler
        {
            static readonly string TailName = GenerateName0(typeof(TailContext));
            static readonly SkipFlags sSkipFlags = CalculateSkipPropagationFlags(typeof(TailContext));
            public TailContext(FABChannelPipeline pipeline)
                : base(pipeline, null, TailName, sSkipFlags)
            {
                this.SetAdded();
            }
            [Skip]
            public void HandlerAdded(IFABChannelHandlerContext context)
            {
            }

            [Skip]
            public void HandlerRemoved(IFABChannelHandlerContext context)
            {
            }
            public override IFABChannelHandler Handler { get { return this; } }
            public void ChannelRegistered(IFABChannelHandlerContext context)
            {
            }

            public void ChannelUnregistered(IFABChannelHandlerContext context)
            {
            }

            public void ChannelActive(IFABChannelHandlerContext context)
            {
            }

            public void ChannelInactive(IFABChannelHandlerContext context)
            {
            }

            public void ExceptionCaught(IFABChannelHandlerContext context, Exception exception) { this.pipeline.OnUnhandledInboundException(exception); }

            public void ChannelRead(IFABChannelHandlerContext context, object message) { this.pipeline.OnUnhandledInboundMessage(message); }

            public void ChannelReadComplete(IFABChannelHandlerContext context)
            {
            }

            public void ChannelWritabilityChanged(IFABChannelHandlerContext context)
            {
            }
            public Task DeregisterAsync(IFABChannelHandlerContext context) { return context.DeregisterAsync(); }
            [Skip]
            public Task DisconnectAsync(IFABChannelHandlerContext context) { return context.DisconnectAsync(); }

            [Skip]
            public Task CloseAsync(IFABChannelHandlerContext context) { return context.CloseAsync(); }

            [Skip]
            public void Read(IFABChannelHandlerContext context) { context.Read(); }

            public void UserEventTriggered(IFABChannelHandlerContext context, object evt) { ReferenceCountUtil.Release(evt); }

            [Skip]
            public Task WriteAsync(IFABChannelHandlerContext ctx, object message) { return ctx.WriteAsync(message); }

            [Skip]
            public void Flush(IFABChannelHandlerContext context) { context.Flush(); }

            [Skip]
            public Task BindAsync(IFABChannelHandlerContext context, EndPoint localAddress) { return context.BindAsync(localAddress); }

            [Skip]
            public Task ConnectAsync(IFABChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress) { return context.ConnectAsync(remoteAddress, localAddress); }
        }
        sealed class HeadContext  : FABHandlerContext, IFABChannelHandler
        {
            static readonly string TailName = GenerateName0(typeof(TailContext));
            static readonly SkipFlags sSkipFlags = CalculateSkipPropagationFlags(typeof(TailContext));
            readonly IFABChannelUnsafe channelUnsafe;
            bool firstRegistration = true;
            public HeadContext(FABChannelPipeline pipeline)
                : base(pipeline, null, TailName, sSkipFlags)
            {
                this.channelUnsafe = pipeline.Channel.Unsafe;
                this.SetAdded();
            }
            public override IFABChannelHandler Handler { get { return this;}}

            public void Flush(IFABChannelHandlerContext context) { this.channelUnsafe.Flush();}

            public Task BindAsync(IFABChannelHandlerContext context, EndPoint localAddress) { return this.channelUnsafe.BindAsync(localAddress);}

            public Task ConnectAsync(IFABChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress) { return this.channelUnsafe.ConnectAsync(remoteAddress, localAddress);}

            public Task DisconnectAsync(IFABChannelHandlerContext context) { return this.channelUnsafe.DisconnectAsync();}

            public Task CloseAsync(IFABChannelHandlerContext context) { return this.channelUnsafe.CloseAsync();}

            public Task DeregisterAsync(IFABChannelHandlerContext context) { return this.channelUnsafe.DeregisterAsync();}

            public void Read(IFABChannelHandlerContext context) { this.channelUnsafe.BeginRead();}

            public Task WriteAsync(IFABChannelHandlerContext context, object message) { return this.channelUnsafe.WriteAsync(message);}

            [Skip]
            public void HandlerAdded(IFABChannelHandlerContext context)
            {
            }

            [Skip]
            public void HandlerRemoved(IFABChannelHandlerContext context)
            {
            }

            [Skip]
            public void ExceptionCaught(IFABChannelHandlerContext ctx, Exception exception) { ctx.FireExceptionCaught(exception);}

            public void ChannelRegistered(IFABChannelHandlerContext context)
            {
                if (this.firstRegistration)
                {
                    this.firstRegistration = false;
                    // We are now registered to the EventLoop. It's time to call the callbacks for the ChannelHandlers,
                    // that were added before the registration was done.
                    this.pipeline.CallHandlerAddedForAllHandlers();
                }

                context.FireChannelRegistered();
            }

            public void ChannelUnregistered(IFABChannelHandlerContext context)
            {
                context.FireChannelUnregistered();

                // Remove all handlers sequentially if channel is closed and unregistered.
                if (!this.pipeline.channel.Open)
                {
                    this.pipeline.Destroy();
                }
            }

            public void ChannelActive(IFABChannelHandlerContext context)
            {
                context.FireChannelActive();

                this.ReadIfIsAutoRead();
            }

            [Skip]
            public void ChannelInactive(IFABChannelHandlerContext context) { context.FireChannelInactive();}

            [Skip]
            public void ChannelRead(IFABChannelHandlerContext ctx, object msg) {  ctx.FireChannelRead(msg);}

            public void ChannelReadComplete(IFABChannelHandlerContext ctx)
            {
                ctx.FireChannelReadComplete();

                this.ReadIfIsAutoRead();
            }

            void ReadIfIsAutoRead()
            {
                if (this.pipeline.channel.Configuration.AutoRead)
                {
                    this.pipeline.channel.Read();
                }
            }

            [Skip]
            public void UserEventTriggered(IFABChannelHandlerContext context, object evt) { this.FireUserEventTriggered(evt);}

            [Skip]
            public void ChannelWritabilityChanged(IFABChannelHandlerContext context) { context.FireChannelWritabilityChanged(); }
        }
        abstract class PendingHandlerCallback : IRunnable
        {
            protected readonly FABChannelPipeline Pipeline;
            protected readonly FABHandlerContext Ctx;
            internal PendingHandlerCallback Next;

            protected PendingHandlerCallback(FABChannelPipeline pipeline, FABHandlerContext ctx)
            {
                this.Pipeline = pipeline;
                this.Ctx = ctx;
            }

            public abstract void Run();

            internal abstract void Execute();
        }
        sealed class PendingHandlerAddedTask : PendingHandlerCallback
        {
            public PendingHandlerAddedTask(FABChannelPipeline pipeline, FABHandlerContext ctx)
                : base(pipeline, ctx)
            {
            }

            public override void Run() { this.Pipeline.CallHandlerAdded0(this.Ctx); }

            internal override void Execute()
            {
                IEventExecutor executor = this.Ctx.Executor;
                if (executor.InEventLoop)
                {
                    this.Pipeline.CallHandlerAdded0(this.Ctx);
                }
                else
                {
                    try
                    {
                        executor.Execute(this);
                    }
                    catch
                    {
                        Remove0(this.Ctx);
                        this.Ctx.SetRemoved();
                    }
                }
            }
        }
        sealed class PendingHandlerRemovedTask : PendingHandlerCallback
        {
            public PendingHandlerRemovedTask(FABChannelPipeline pipeline, FABHandlerContext ctx)
                : base(pipeline, ctx)
            {
            }

            public override void Run() { this.Pipeline.CallHandlerRemoved0(this.Ctx); }

            internal override void Execute()
            {
                IEventExecutor executor = this.Ctx.Executor;
                if (executor.InEventLoop)
                {
                    this.Pipeline.CallHandlerRemoved0(this.Ctx);
                }
                else
                {
                    try
                    {
                        executor.Execute(this);
                    }
                    catch
                    {
                        // remove0(...) was call before so just call AbstractChannelHandlerContext.setRemoved().
                        this.Ctx.SetRemoved();
                    }
                }
            }
        }
    }
}
