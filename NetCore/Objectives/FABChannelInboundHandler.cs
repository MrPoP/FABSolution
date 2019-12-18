using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public abstract class FABChannelInboundHandler<I> : FABChannelAdapter
    {
        private readonly bool autoRelease;
        protected FABChannelInboundHandler()
            : this(true)
        {
        }

        protected FABChannelInboundHandler(bool autoRelease)
        {
            this.autoRelease = autoRelease;
        }
        public bool AcceptInboundMessage(object msg)
        {
            return (msg is I);
        }

        public override void ChannelRead(IFABChannelHandlerContext ctx, object msg)
        {
            bool flag = true;
            try
            {
                if (this.AcceptInboundMessage(msg))
                {
                    I local = (I)msg;
                    this.ChannelRead0(ctx, local);
                }
                else
                {
                    flag = false;
                    ctx.FireChannelRead(msg);
                }
            }
            finally
            {
                if (this.autoRelease & flag)
                {
                    ReferenceCountUtil.Release(msg);
                }
            }
        }
        protected abstract void ChannelRead0(IFABChannelHandlerContext ctx, I msg);
        public override Task WriteAsync(IFABChannelHandlerContext ctx, object msg)
        {
            bool flag = true;
            Task towork = null;
            try
            {
                if (this.AcceptInboundMessage(msg))
                {
                    I local = (I)msg;
                    towork = this.WriteAsync0(ctx, local);
                }
            }
            finally
            {
                if (this.autoRelease & flag)
                {
                    ReferenceCountUtil.Release(msg);
                }
            }
            return towork;
        }
        protected abstract Task WriteAsync0(IFABChannelHandlerContext ctx, I msg);
    }
}
