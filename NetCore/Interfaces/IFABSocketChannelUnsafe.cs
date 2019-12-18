using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    internal interface IFABSocketChannelUnsafe : IFABChannelUnsafe
    {
        /// <summary>
        ///     Finish connect
        /// </summary>
        void FinishConnect(FABSocketChannelAsyncOperation operation);

        /// <summary>
        ///     Read from underlying {@link SelectableChannel}
        /// </summary>
        void FinishRead(FABSocketChannelAsyncOperation operation);

        void FinishWrite(FABSocketChannelAsyncOperation operation);
    }
}
