using System;
using System.Collections.Generic;

namespace NetCore
{
     public class FABBadFormatException : Exception
    {
        public FABBadFormatException(Type type)
            : base(string.Format("Bad format in {0}", type.Name))
        { }

        public FABBadFormatException(Type type, IEnumerable<byte> data)
            : base(string.Format("Bad format in {0}: {1}", type.Name, data.ToString()))
        { }
    }

    public class FABBadOpCodeException : Exception
    {
        public FABBadOpCodeException(string msg, params object[] args)
            : base(string.Format(msg, args))
        { }
        public FABBadOpCodeException(ushort opCode)
            : base(string.Format("Bad opCode: {0}", opCode))
        { }

        public FABBadOpCodeException(OpCode opCode)
            : base(string.Format("Bad opCode: {0}", opCode))
        { }
    }
    public class FABChannelPipelineException : Exception
    {
        public FABChannelPipelineException(string msg, params object[] args)
            : base(string.Format(msg, args))
        { }
    }
}
