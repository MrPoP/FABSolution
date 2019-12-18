using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public interface IFABMessage
    {
        OpCode OpCode { get; }
        int DataLength { get; }
        Direction Direction { get; }
        ReadOnlyCollection<byte> Data { get; }
        byte[] this[Direction direction] { get; }
    }
}
