using NetCore.Packets;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public unsafe class FABMessageSerializer<T>
    {
        public FABMessageSerializer()
        {
        }
        public bool CanHandle(None type)
        {
            return type.GetType() == typeof(None);
        }
        public T Deserialize(FABMessage data, IntPtr value)
        {
            ((FABMessageWatcher)data).Read(Marshal.SizeOf(value), (void*)value);
            return Marshal.PtrToStructure<T>(value);
        }

        public FABMessage Serialize(IntPtr value)
        {
            var message = new FABMessage();
            ((FABMessageWatcher)message).Write((void*)value, Marshal.SizeOf(value));
            return message;
        }
    }
}
