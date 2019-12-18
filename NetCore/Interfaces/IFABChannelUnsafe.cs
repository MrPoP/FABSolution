using DotNetty.Transport.Channels;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NetCore
{
    public interface IFABChannelUnsafe
    {
        void BeginRead();
        Task BindAsync(EndPoint localAddress);
        Task CloseAsync();
        void CloseForcibly();
        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);
        Task DeregisterAsync();
        Task DisconnectAsync();
        void Flush();
        Task RegisterAsync(IEventLoop eventLoop);
        Task WriteAsync(object message);
        FABChannelOutboundBuffer OutboundBuffer { get; }
        IRecvByteBufAllocatorHandle RecvBufAllocHandle { get; }
    }
}
