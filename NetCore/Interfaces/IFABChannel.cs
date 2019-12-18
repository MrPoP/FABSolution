using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public interface IFABChannel : IAttributeMap, IComparable<IFABChannel>
    {
        Task BindAsync(EndPoint localAddress);
        Task CloseAsync();
        Task ConnectAsync(EndPoint remoteAddress);
        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);
        Task DeregisterAsync();
        Task DisconnectAsync();
        IFABChannel Flush();
        IFABChannel Read();
        Task WriteAndFlushAsync(object message);
        Task WriteAsync(object message);

        bool Active { get; }

        IByteBufferAllocator Allocator { get; }

        Task CloseCompletion { get; }

        IChannelConfiguration Configuration { get; }

        IEventLoop EventLoop { get; }

        IFABChannelId Id { get; }

        bool IsWritable { get; }

        EndPoint LocalAddress { get; }

        ChannelMetadata Metadata { get; }

        bool Open { get; }

        IFABChannel Parent { get; }

        IFABChannelPipeline Pipeline { get; }

        bool Registered { get; }

        EndPoint RemoteAddress { get; }

        IFABChannelUnsafe Unsafe { get; }
    }
}
