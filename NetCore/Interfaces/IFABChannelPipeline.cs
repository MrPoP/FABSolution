using DotNetty.Common.Concurrency;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public interface IFABChannelPipeline : IEnumerable<IFABChannelHandler>, IEnumerable
    {
        IFABChannelPipeline AddAfter(string baseName, string name, IFABChannelHandler handler);
        IFABChannelPipeline AddAfter(IEventExecutorGroup group, string baseName, string name, IFABChannelHandler handler);
        IFABChannelPipeline AddBefore(string baseName, string name, IFABChannelHandler handler);
        IFABChannelPipeline AddBefore(IEventExecutorGroup group, string baseName, string name, IFABChannelHandler handler);
        IFABChannelPipeline AddFirst(params IFABChannelHandler[] handlers);
        IFABChannelPipeline AddFirst(IEventExecutorGroup group, params IFABChannelHandler[] handlers);
        IFABChannelPipeline AddFirst(string name, IFABChannelHandler handler);
        IFABChannelPipeline AddFirst(IEventExecutorGroup group, string name, IFABChannelHandler handler);
        IFABChannelPipeline AddLast(params IFABChannelHandler[] handlers);
        IFABChannelPipeline AddLast(IEventExecutorGroup group, params IFABChannelHandler[] handlers);
        IFABChannelPipeline AddLast(string name, IFABChannelHandler handler);
        IFABChannelPipeline AddLast(IEventExecutorGroup group, string name, IFABChannelHandler handler);
        Task BindAsync(EndPoint localAddress);
        Task CloseAsync();
        Task ConnectAsync(EndPoint remoteAddress);
        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);
        IFABChannelHandlerContext Context<T>() where T : class, IFABChannelHandler;
        IFABChannelHandlerContext Context(IFABChannelHandler handler);
        IFABChannelHandlerContext Context(string name);
        Task DeregisterAsync();
        Task DisconnectAsync();
        IFABChannelPipeline FireChannelActive();
        IFABChannelPipeline FireChannelInactive();
        IFABChannelPipeline FireChannelRead(object msg);
        IFABChannelPipeline FireChannelReadComplete();
        IFABChannelPipeline FireChannelRegistered();
        IFABChannelPipeline FireChannelUnregistered();
        IFABChannelPipeline FireChannelWritabilityChanged();
        IFABChannelPipeline FireExceptionCaught(Exception cause);
        IFABChannelPipeline FireUserEventTriggered(object evt);
        IFABChannelHandler First();
        IFABChannelHandlerContext FirstContext();
        IFABChannelPipeline Flush();
        T Get<T>() where T : class, IFABChannelHandler;
        IFABChannelHandler Get(string name);
        IFABChannelHandler Last();
        IFABChannelHandlerContext LastContext();
        IFABChannelPipeline Read();
        T Remove<T>() where T : class, IFABChannelHandler;
        IFABChannelPipeline Remove(IFABChannelHandler handler);
        IFABChannelHandler Remove(string name);
        IFABChannelHandler RemoveFirst();
        IFABChannelHandler RemoveLast();
        T Replace<T>(string newName, IFABChannelHandler newHandler) where T : class, IFABChannelHandler;
        IFABChannelPipeline Replace(IFABChannelHandler oldHandler, string newName, IFABChannelHandler newHandler);
        IFABChannelHandler Replace(string oldName, string newName, IFABChannelHandler newHandler);
        Task WriteAndFlushAsync(object msg);
        Task WriteAsync(object msg);

        IFABChannel Channel { get; }
    }
}
