using DotNetty.Buffers;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetCore.Message
{
    public class FABChannelGroup : IFABChannelGroup
    {
        static int nextId;
        readonly string Name;
        readonly IEventExecutor executor;
        readonly ConcurrentDictionary<FABChannelId, FABChannel> nonServerChannels = new ConcurrentDictionary<FABChannelId, FABChannel>();
        readonly ConcurrentDictionary<FABChannelId, FABChannel> serverChannels = new ConcurrentDictionary<FABChannelId, FABChannel>();
        public FABChannelGroup(IEventExecutor executor)
            : this(string.Format("group-{0}", Interlocked.Increment(ref nextId)), executor)
        {
        }
        public FABChannelGroup(string name, IEventExecutor executor)
        {
            if (name == null)
            {
                throw new ArgumentNullException(FABExtensions.nameof(name));
            }
            this.Name = name;
            this.executor = executor;
        }
        public bool IsEmpty { get { return this.serverChannels.Count == 0 && this.nonServerChannels.Count == 0; } }
        public IFABChannel Find(FABChannelId id)
        {
            FABChannel channel;
            if (this.nonServerChannels.TryGetValue(id, out channel))
            {
                return channel;
            }
            else
            {
                this.serverChannels.TryGetValue(id, out channel);
                return channel;
            }
        }
        public Task WriteAsync(FABMessage message) { return this.WriteAsync(message, FABChannelMatchers.All()); }
        public Task WriteAsync(FABMessage message, IFABChannelMatcher matcher)
        {
            Contract.Requires(message != null);
            Contract.Requires(matcher != null);
            var futures = new Dictionary<IFABChannel, Task>();
            foreach (FABChannel c in this.nonServerChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.WriteAsync(SafeDuplicate(message)));
                }
            }

            ReferenceCountUtil.Release(message);
            return new FABChannelGroupCompletionSource(this, futures /*, this.executor*/).Task;
        }
        public IFABChannelGroup Flush(IFABChannelMatcher matcher)
        {
            foreach (IFABChannel c in this.nonServerChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    c.Flush();
                }
            }
            return this;
        }
        public IFABChannelGroup Flush() { return this.Flush(FABChannelMatchers.All()); }
        public void Clear()
        {
            this.serverChannels.Clear();
            this.nonServerChannels.Clear();
        }
        public bool Contains(FABChannel item)
        {
            FABChannel channel;
            if (item is IServerChannel)
            {
                return this.serverChannels.TryGetValue(item.Id, out channel) && channel == item;
            }
            else
            {
                return this.nonServerChannels.TryGetValue(item.Id, out channel) && channel == item;
            }
        }
        public int Count { get { return this.nonServerChannels.Count + this.serverChannels.Count; } }
        public bool IsReadOnly { get { return false; } }
        public bool Remove(FABChannel channel)
        {
            FABChannel ch;
            if (channel is IServerChannel)
            {
                return this.serverChannels.TryRemove(channel.Id, out ch);
            }
            else
            {
                return this.nonServerChannels.TryRemove(channel.Id, out ch);
            }
        }
        public IEnumerator<FABChannel> GetEnumerator()
        {
            return new CombinedEnumerator<FABChannel>(this.serverChannels.Values.GetEnumerator(),
                this.nonServerChannels.Values.GetEnumerator());
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CombinedEnumerator<IFABChannel>(this.serverChannels.Values.GetEnumerator(),
            this.nonServerChannels.Values.GetEnumerator());
        }
        public Task WriteAndFlushAsync(FABMessage message) { return this.WriteAndFlushAsync(message, FABChannelMatchers.All()); }
        public Task WriteAndFlushAsync(FABMessage message, IFABChannelMatcher matcher)
        {
            Contract.Requires(message != null);
            Contract.Requires(matcher != null);
            var futures = new Dictionary<IFABChannel, Task>();
            foreach (IFABChannel c in this.nonServerChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.WriteAndFlushAsync(SafeDuplicate(message)));
                }
            }

            ReferenceCountUtil.Release(message);
            return new FABChannelGroupCompletionSource(this, futures /*, this.executor*/).Task;
        }
        public Task DisconnectAsync() { return this.DisconnectAsync(FABChannelMatchers.All()); }
        public Task DisconnectAsync(IFABChannelMatcher matcher)
        {
            Contract.Requires(matcher != null);
            var futures = new Dictionary<IFABChannel, Task>();
            foreach (IFABChannel c in this.nonServerChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.DisconnectAsync());
                }
            }
            foreach (IFABChannel c in this.serverChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.DisconnectAsync());
                }
            }

            return new FABChannelGroupCompletionSource(this, futures /*, this.executor*/).Task;
        }
        public Task CloseAsync() { return this.CloseAsync(FABChannelMatchers.All()); }
        public Task CloseAsync(IFABChannelMatcher matcher)
        {
            Contract.Requires(matcher != null);
            var futures = new Dictionary<IFABChannel, Task>();
            foreach (IFABChannel c in this.nonServerChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.CloseAsync());
                }
            }
            foreach (IFABChannel c in this.serverChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.CloseAsync());
                }
            }

            return new FABChannelGroupCompletionSource(this, futures /*, this.executor*/).Task;
        }
        public Task DeregisterAsync() { return this.DeregisterAsync(FABChannelMatchers.All()); }
        public Task DeregisterAsync(IFABChannelMatcher matcher)
        {
            Contract.Requires(matcher != null);
            var futures = new Dictionary<IFABChannel, Task>();
            foreach (IFABChannel c in this.nonServerChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.DeregisterAsync());
                }
            }
            foreach (IFABChannel c in this.serverChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.DeregisterAsync());
                }
            }

            return new FABChannelGroupCompletionSource(this, futures /*, this.executor*/).Task;
        }
        public Task NewCloseFuture() { return this.NewCloseFuture(FABChannelMatchers.All()); }
        public Task NewCloseFuture(IFABChannelMatcher matcher)
        {
            Contract.Requires(matcher != null);
            var futures = new Dictionary<IFABChannel, Task>();
            foreach (IFABChannel c in this.nonServerChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.CloseCompletion);
                }
            }
            foreach (IFABChannel c in this.serverChannels.Values)
            {
                if (matcher.Matches(c))
                {
                    futures.Add(c, c.CloseCompletion);
                }
            }

            return new FABChannelGroupCompletionSource(this, futures /*, this.executor*/).Task;
        }
        static object SafeDuplicate(FABMessage message)
        {
            //AbstractByteBuffer buffer = new AbstractByteBuffer(message.DataLength);
            var buffer = message.Data as IByteBuffer;
            if (buffer != null)
            {
                return buffer.Duplicate().Retain();
            }

            var byteBufferHolder = message.Data as IByteBufferHolder;
            if (byteBufferHolder != null)
            {
                return byteBufferHolder.Duplicate().Retain();
            }

            return ReferenceCountUtil.Retain(message);
        }
        public bool Add(FABChannel channel)
        {
            ConcurrentDictionary<FABChannelId, FABChannel> map = channel is IServerChannel ? this.serverChannels : this.nonServerChannels;
            bool added = map.TryAdd(channel.Id, channel);
            if (added)
            {
                channel.CloseCompletion.ContinueWith(x => this.Remove(channel));
            }
            return added;
        }
        public FABChannel[] ToArray()
        {
            var channels = new List<FABChannel>(this.Count);
            channels.AddRange(this.serverChannels.Values);
            channels.AddRange(this.nonServerChannels.Values);
            return channels.ToArray();
        }
        public bool Remove(FABChannelId channelId)
        {
            FABChannel ch;

            if (this.serverChannels.TryRemove(channelId, out ch))
            {
                return true;
            }

            if (this.nonServerChannels.TryRemove(channelId, out ch))
            {
                return true;
            }

            return false;
        }
        public bool Remove(object o)
        {
            var id = o as FABChannelId;
            if (id != null)
            {
                return this.Remove(id);
            }
            else
            {
                var channel = o as FABChannel;
                if (channel != null)
                {
                    return this.Remove(channel);
                }
            }
            return false;
        }
        string IFABChannelGroup.Name
        {
            get { return this.Name; }
        }
        public Task WriteAndFlushAsync(object message, IFABChannelMatcher matcher)
        {
            return this.WriteAndFlushAsync(message, matcher);
        }

        public Task WriteAndFlushAsync(object message)
        {
            return this.WriteAndFlushAsync(message);
        }

        public Task WriteAsync(object message, IFABChannelMatcher matcher)
        {
            return this.WriteAsync(message, matcher);
        }

        public Task WriteAsync(object message)
        {
            return this.WriteAsync(message);
        }

        public void Add(IFABChannel item)
        {
            this.Add(item);
        }

        public bool Contains(IFABChannel item)
        {
            return this.Contains(item);
        }

        public void CopyTo(IFABChannel[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        public bool Remove(IFABChannel item)
        {
            return this.Remove(item);
        }

        IEnumerator<IFABChannel> IEnumerable<IFABChannel>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int CompareTo(IFABChannelGroup other)
        {
            return this.CompareTo(other);
        }
    }
}
