using NetCore.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABServerFactory : MessageFactory<OpCode, None>
    {
        public readonly static FABServerFactory Default = new FABServerFactory();
        public FABServerFactory()
        {
            Register<Ping>(OpCode.Ping);
        }
    }
}
