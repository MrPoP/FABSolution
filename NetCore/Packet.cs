using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public class Packet : Attribute
    {
        public static readonly Func<Packet, ushort> TypeTranslator = (attr) => (ushort)attr;
        readonly OpCode _Code;
        public OpCode Code { get { return this._Code; } }
        public Packet(OpCode code)
        {
            this._Code = code;
        }
        public static implicit operator ushort(Packet message)
        {
            return (ushort)message._Code;
        }
    }
}
