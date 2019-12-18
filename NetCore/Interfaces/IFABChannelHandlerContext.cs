using DotNetty.Buffers;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public interface IFABChannelHandlerContext : IAttributeMap
    {
        Task BindAsync(EndPoint localAddress);
        Task CloseAsync();
        Task ConnectAsync(EndPoint remoteAddress);
        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);
        Task DeregisterAsync();
        Task DisconnectAsync();
        IFABChannelHandlerContext FireChannelActive();
        IFABChannelHandlerContext FireChannelInactive();
        IFABChannelHandlerContext FireChannelRead(object message);
        IFABChannelHandlerContext FireChannelReadComplete();
        IFABChannelHandlerContext FireChannelRegistered();
        IFABChannelHandlerContext FireChannelUnregistered();
        IFABChannelHandlerContext FireChannelWritabilityChanged();
        IFABChannelHandlerContext FireExceptionCaught(Exception ex);
        IFABChannelHandlerContext FireUserEventTriggered(object evt);
        IFABChannelHandlerContext Flush();
        IFABChannelHandlerContext Read();
        Task WriteAndFlushAsync(object message);
        Task WriteAsync(object message);

        IByteBufferAllocator Allocator { get; }

        IFABChannel Channel { get; }

        IEventExecutor Executor { get; }

        IFABChannelHandler Handler { get; }

        string Name { get; }

        bool Removed { get; }
    }
}
