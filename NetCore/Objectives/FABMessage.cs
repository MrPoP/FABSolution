using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABMessage : IFABMessage
    {
        public OpCode OpCode { get { return this.Watcher.OpCode; } }
        public int DataLength { get { return this.Watcher.Length; } }
        public Direction Direction { get { return this.Watcher.Direction; } }
        public ReadOnlyCollection<byte> Data { get { return this.Watcher.Data; } }
        readonly FABMessageWatcher Watcher;

        public FABMessage(int length = Constants.MessageFileMaxSize)
        {
            this.Watcher = new FABMessageWatcher(length);
        }
        public FABMessage(byte[] data)
        {
            this.Watcher = new FABMessageWatcher(data);
        }
        public FABMessage(OpCode op)
        {
            this.Watcher = new FABMessageWatcher();
            this.Watcher.SetOpCode(op);
        }

        public byte[] this[Direction direction]
        {
            get { return this.Watcher.Finalize(direction); }
        }

        public static implicit operator FABMessageWatcher(FABMessage message)
        {
            return message == null ? null : message.Watcher;
        }
        public static FABMessage operator !(FABMessage message)
        {
            return message == null ? null : message;
        }
    }
}
