using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABChannelAdapter : IFABChannelHandler
    {
        internal bool Added;
        public virtual bool IsSharable { get { return false; } }

        public virtual Task BindAsync(IFABChannelHandlerContext context, System.Net.EndPoint localAddress)
        {
            return context.BindAsync(localAddress);
        }

        public virtual void ChannelActive(IFABChannelHandlerContext context)
        {
            context.FireChannelActive();
        }

        public virtual void ChannelInactive(IFABChannelHandlerContext context)
        {
            context.FireChannelInactive();
        }

        public virtual void ChannelRead(IFABChannelHandlerContext context, object message)
        {
            context.FireChannelRead(message);
        }

        public virtual void ChannelReadComplete(IFABChannelHandlerContext context)
        {
            context.FireChannelReadComplete();
        }

        public virtual void ChannelRegistered(IFABChannelHandlerContext context)
        {
            context.FireChannelRegistered();
        }

        public virtual void ChannelUnregistered(IFABChannelHandlerContext context)
        {
            context.FireChannelUnregistered();
        }

        public virtual void ChannelWritabilityChanged(IFABChannelHandlerContext context)
        {
            context.FireChannelWritabilityChanged();
        }

        public virtual Task CloseAsync(IFABChannelHandlerContext context)
        {
            return context.CloseAsync();
        }

        public virtual Task ConnectAsync(IFABChannelHandlerContext context, System.Net.EndPoint remoteAddress, System.Net.EndPoint localAddress)
        {
            return context.ConnectAsync(remoteAddress, localAddress);
        }

        public virtual Task DeregisterAsync(IFABChannelHandlerContext context)
        {
            return context.DeregisterAsync();
        }

        public virtual Task DisconnectAsync(IFABChannelHandlerContext context)
        {
            return context.DisconnectAsync();
        }
        protected void EnsureNotSharable()
        {
            if (this.IsSharable)
            {
                throw new InvalidOperationException("ChannelHandler " + StringUtil.SimpleClassName(this) + " is not allowed to be shared");
            }
        }
        public void ExceptionCaught(IFABChannelHandlerContext context, Exception exception)
        {
            context.FireExceptionCaught(exception);
        }

        public virtual void Flush(IFABChannelHandlerContext context)
        {
            context.Flush();
        }

        public virtual void HandlerAdded(IFABChannelHandlerContext context)
        {
        }

        public virtual void HandlerRemoved(IFABChannelHandlerContext context)
        {
            throw new NotImplementedException();
        }

        public virtual void Read(IFABChannelHandlerContext context)
        {
            context.Read();
        }

        public virtual void UserEventTriggered(IFABChannelHandlerContext context, object evt)
        {
            context.FireUserEventTriggered(evt);
        }

        public virtual Task WriteAsync(IFABChannelHandlerContext context, object message)
        {
            return context.WriteAsync(message);
        }
    }
}
