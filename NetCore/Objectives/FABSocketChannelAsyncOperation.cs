using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABSocketChannelAsyncOperation : SocketAsyncEventArgs
    {
        public FABSocketChannelAsyncOperation(FABSocketChannel channel)
            : this(channel, true)
        {
        }

        public FABSocketChannelAsyncOperation(FABSocketChannel channel, bool setEmptyBuffer)
        {
            Contract.Requires(channel != null);

            this.Channel = channel;
            this.Completed += FABSocketChannel.IoCompletedCallback;
            if (setEmptyBuffer)
            {
                this.SetBuffer(ArrayExtensions.ZeroBytes, 0, 0);
            }
        }

        public void Validate()
        {
            SocketError socketError = this.SocketError;
            if (socketError != SocketError.Success)
            {
                throw new SocketException((int)socketError);
            }
        }

        public FABSocketChannel Channel { get; private set; }
    }
}
