using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public interface IFABChannelGroup : ICollection<IFABChannel>, IEnumerable<IFABChannel>, IEnumerable, IComparable<IFABChannelGroup>
    {
        Task CloseAsync();
        Task CloseAsync(IFABChannelMatcher matcher);
        Task DeregisterAsync();
        Task DeregisterAsync(IFABChannelMatcher matcher);
        Task DisconnectAsync();
        Task DisconnectAsync(IFABChannelMatcher matcher);
        IFABChannel Find(FABChannelId id);
        IFABChannelGroup Flush();
        IFABChannelGroup Flush(IFABChannelMatcher matcher);
        Task NewCloseFuture();
        Task NewCloseFuture(IFABChannelMatcher matcher);
        Task WriteAndFlushAsync(object message);
        Task WriteAndFlushAsync(object message, IFABChannelMatcher matcher);
        Task WriteAsync(object message);
        Task WriteAsync(object message, IFABChannelMatcher matcher);

        string Name { get; }
    }
}
