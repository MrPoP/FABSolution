using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABServerHandler : FABChannelInboundHandler<FABMessage>
    {
        readonly FABServerFactory Factory;
        private FABHandlerContext context = null;
        public FABServerHandler()
        {
            Factory = FABServerFactory.Default;
        }
        public override void Flush(IFABChannelHandlerContext context)
        {
            context.Flush();
            if (this.context == null)
                this.context = (FABHandlerContext)context;
        }
        public override void ChannelReadComplete(IFABChannelHandlerContext context)
        {
            context.Flush();
        }
        protected override void ChannelRead0(IFABChannelHandlerContext ctx, FABMessage msg)
        {
            try
            {
                var item = Factory.GetStructure(msg);
            }
            finally
            {
                ctx.FireChannelReadComplete();
            }
        }
        protected override Task WriteAsync0(IFABChannelHandlerContext ctx, FABMessage msg)
        {
            return ctx.WriteAsync(msg);
        }
        public override Task BindAsync(IFABChannelHandlerContext ctx, EndPoint localAddress)
        {
            return ctx.BindAsync(localAddress);
        }
        public virtual Task BindAsync(string ip, int Port)
        {
            if (this.context == null)
                return null;
            return this.context.BindAsync(new IPEndPoint(IPAddress.Parse(ip), Port));
        }
        public override void ChannelActive(IFABChannelHandlerContext context)
        {
            if (this.context == null)
                this.context = (FABHandlerContext)context;
            base.ChannelActive(context);
        } 
    }
}
