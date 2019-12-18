using NetCore.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace NetCore
{
    public class MessageFactory<T> : Dictionary<Type, ushort> where T : struct
    {
        private readonly Dictionary<ushort, IntPtr> _typeLookup = new Dictionary<ushort, IntPtr>();
        private readonly Dictionary<IntPtr, ushort> _opCodeLookup = new Dictionary<IntPtr, ushort>();
        private FABMessageSerializer<T> serializer = null;
        public MessageFactory()
            :base()
        {
            this.serializer = new FABMessageSerializer<T>();
        }
        protected void Register<T>(ushort opCode)
        {
            var type = default(T);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(type));
            Marshal.StructureToPtr(type, ptr, false);
            _opCodeLookup.Add(ptr, opCode);
            _typeLookup.Add(opCode, ptr);
            System.Attribute attrs = System.Attribute.GetCustomAttributes(typeof(T)).Where(p => p is Packet).FirstOrDefault();
            if(attrs != null)
            {
                base.Add((attrs as Packet).Code.GetType(), opCode);
            }
        }
        public ushort GetOpCode(Type type)
        {
            ushort opCode;
            if (base.TryGetValue(type, out opCode))
                return opCode;

            throw new FABBadOpCodeException("No opcode found for type {0}", type.GetType().FullName);
        }
        public T GetStructure(FABMessage message)
        {
            IntPtr type;
            if (!_typeLookup.TryGetValue((ushort)message.OpCode, out type))
                throw new FABBadOpCodeException("No type found for opcode {0}", (ushort)message.OpCode);
            return this.serializer.Deserialize(message, type);
        }
        public FABMessage GetMessage(T value)
        {
            IntPtr pointer = Marshal.OffsetOf(typeof(T), value.GetType().Name);
            ushort code;
            if (!_opCodeLookup.TryGetValue(pointer, out code))
            {
#if DEBUG
                throw new FABBadOpCodeException("No opcode found for type {0}", value.GetType().FullName);
#else
                throw new FABBadOpCodeException(opCode);
#endif
            }
            if (!_typeLookup.ContainsKey(code))
                Register<T>(code);
            FABMessage message = serializer.Serialize(pointer);
            if((ushort)message.OpCode != code)
            {
                #if DEBUG
                throw new FABBadOpCodeException("No opcode found for type {0}", value.GetType().FullName);
#else
                throw new ProudBadOpCodeException(opCode);
#endif
            }
            return message;
        }
        public bool ContainsType(T type)
        {
            IntPtr pointer = Marshal.OffsetOf(typeof(T), type.GetType().Name);
            return _opCodeLookup.ContainsKey(pointer);
        }
        public bool ContainsOpCode(ushort opCode)
        {
            return _typeLookup.ContainsKey(opCode);
        }
        ~MessageFactory()
        {
            foreach (IntPtr pointer in _typeLookup.Values)
                Marshal.FreeHGlobal(pointer);
            _typeLookup.Clear();
            _opCodeLookup.Clear();
        }
    }
    public class MessageFactory<TOpCode, TMessage> : MessageFactory<TMessage> where TMessage : struct
    {
        public MessageFactory()
            : base() { }
        protected void Register<TMessage>(TOpCode opCode)
        {
            base.Register<TMessage>(DynamicCast<ushort>.From(opCode));
        }
        public new TOpCode GetOpCode(Type type)
        {
            return DynamicCast<TOpCode>.From(base.GetOpCode(type));
        }
        public TMessage GetStructureMap(FABMessage message)
        {
            return (TMessage)base.GetStructure(message);
        }
        public FABMessage GetMessageMap(TMessage value)
        {
            return base.GetMessage(value);
        }
        public bool ContainsOpCode(TOpCode opCode)
        {
            return base.ContainsOpCode(DynamicCast<ushort>.From(opCode));
        }
    }
}
