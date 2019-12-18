using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public interface IFABChannelHandler
    {
        Task BindAsync(IFABChannelHandlerContext context, EndPoint localAddress);
        void ChannelActive(IFABChannelHandlerContext context);
        void ChannelInactive(IFABChannelHandlerContext context);
        void ChannelRead(IFABChannelHandlerContext context, object message);
        void ChannelReadComplete(IFABChannelHandlerContext context);
        void ChannelRegistered(IFABChannelHandlerContext context);
        void ChannelUnregistered(IFABChannelHandlerContext context);
        void ChannelWritabilityChanged(IFABChannelHandlerContext context);
        Task CloseAsync(IFABChannelHandlerContext context);
        Task ConnectAsync(IFABChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress);
        Task DeregisterAsync(IFABChannelHandlerContext context);
        Task DisconnectAsync(IFABChannelHandlerContext context);
        void ExceptionCaught(IFABChannelHandlerContext context, Exception exception);
        void Flush(IFABChannelHandlerContext context);
        void HandlerAdded(IFABChannelHandlerContext context);
        void HandlerRemoved(IFABChannelHandlerContext context);
        void Read(IFABChannelHandlerContext context);
        void UserEventTriggered(IFABChannelHandlerContext context, object evt);
        Task WriteAsync(IFABChannelHandlerContext context, object message);
    }
}
