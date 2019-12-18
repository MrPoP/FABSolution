using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    [Flags]
    public enum Direction : byte
    {
        None = 0,
        ClientToServer,
        ServerToClient
    }
    [Flags]
    public enum OpCode : ushort
    {
        None = 1000,
        Ping,
        TransactionShortage,
        EndDay = ushort.MaxValue
    }
}
