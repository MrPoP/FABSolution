using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Packets
{
    [Packet(OpCode.Ping)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Ping
    {
        public long Time;
    }
}
