using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABMessageWatcher : IDisposable
    {
        private byte[] data = null;
        private OpCode opCode = OpCode.None;
        private Direction direction = Direction.None;
        private int IO_write_index = 0, IO_read_index = 0;
        private bool InitializedIO = false;
        readonly object syncRoot;
        public Direction Direction { get { return this.direction; } }
        public OpCode OpCode { get { return this.opCode; } }
        public ReadOnlyCollection<byte> Data { get { return new ReadOnlyCollection<byte>(Finalize().ToList()); } }
        public int Length { get { return this.Data.Count; } }
        public FABMessageWatcher(int length = Constants.MessageFileMaxSize)
        {
            this.data = new byte[length];
            this.syncRoot = new object();
            this.IO_write_index = 0;
            this.IO_read_index = 0;
            this.InitializedIO = false;
        }
        public FABMessageWatcher(byte[] indata)
        {
            this.data = new byte[indata.Length - 3];
            Array.Copy(indata, 2, this.data, 0, indata.Length - 3);
            this.syncRoot = new object();
            this.IO_write_index = 0;
            this.IO_read_index = 0;
            this.opCode = (NetCore.OpCode)indata.ReadUint16(0);
            this.direction = (NetCore.Direction)indata.ReadUint8(indata.Length - 1);
            this.InitializedIO = true;
        }
        public void SetOpCode(OpCode code)
        {
            this.opCode = code;
        }
        private byte[] Finalize()
        {
            byte[] toretdata = new byte[this.IO_write_index + 3];
            int offset = 0;
            toretdata.Write((ushort)this.opCode, ref offset);
            Array.Copy(this.data, 0, toretdata, offset, this.IO_write_index);
            offset += this.data.Length;
            toretdata.Write((byte)this.direction, ref offset);
            return toretdata;
        }
        public byte[] Finalize(Direction _direction = Direction.ServerToClient)
        {
            this.direction = _direction;
            return Finalize();
        }
        private void InitIO()
        {
            this.IO_write_index = 0;
            this.IO_read_index = 0;
            this.data = new byte[Constants.MessageFileMaxSize];
            this.InitializedIO = true;
        }
        #region Writers
        public void Write(byte value, int offset)
        {
            lock(this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public void Write(byte value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, byte value, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, ref messageWatcher.IO_write_index);
            }
        }
        public void Write(sbyte value, int offset)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public void Write(sbyte value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, sbyte value, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, ref messageWatcher.IO_write_index);
            }
        }
        public void Write(short value, int offset)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public void Write(short value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, short value, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, ref messageWatcher.IO_write_index);
            }
        }
        public void Write(ushort value, int offset)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public void Write(ushort value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, ushort value, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, ref messageWatcher.IO_write_index);
            }
        }
        public void Write(int value, int offset)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public void Write(int value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, int value, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, ref messageWatcher.IO_write_index);
            }
        }
        public void Write(uint value, int offset)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public void Write(uint value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, uint value, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, ref messageWatcher.IO_write_index);
            }
        }
        public void Write(long value, int offset)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public void Write(long value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, long value, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, ref messageWatcher.IO_write_index);
            }
        }
        public void Write(ulong value, int offset)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public void Write(ulong value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, ulong value, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, ref messageWatcher.IO_write_index);
            }
        }
        public void Write(string value, int offset)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, string value, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, ref messageWatcher.IO_write_index);
            }
        }
        public void Write(string value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, ref this.IO_write_index);
            }
        }
        public void Write(params string[] value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(ref this.IO_write_index, value);
            }
        }
        public void Write(int offset, params string[] value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(ref this.IO_write_index, value);
            }
        }
        public static void Write(FABMessageWatcher messageWatcher, int offset, params string[] value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(ref messageWatcher.IO_write_index, value);
            }
        }
        public unsafe void Write(void* value, int Length)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.data.Write(value, Length, ref this.IO_write_index);
            }
        }
        public unsafe void Write(void* value, int Length, int offset)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                this.IO_write_index = offset;
                this.data.Write(value, Length, ref this.IO_write_index);
            }
        }
        public unsafe static void Write(FABMessageWatcher messageWatcher, void* value, int Length, int offset)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                if (offset > messageWatcher.IO_write_index)
                    messageWatcher.IO_write_index = offset;
                messageWatcher.data.Write(value, Length, ref messageWatcher.IO_write_index);
            }
        }
        #endregion
        #region Readers
        public void Read(int offset, out byte value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadUint8(this.IO_read_index);
                this.IO_read_index += sizeof(byte);
            }
        }
        public void Read(out byte value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadUint8(this.IO_read_index);
                this.IO_read_index += sizeof(byte);
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out byte value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public void Read(int offset, out sbyte value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadInt8(this.IO_read_index);
                this.IO_read_index += sizeof(byte);
            }
        }
        public void Read(out sbyte value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadInt8(this.IO_read_index);
                this.IO_read_index += sizeof(sbyte);
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out sbyte value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public void Read(int offset, out short value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadInt16(this.IO_read_index);
                this.IO_read_index += sizeof(short);
            }
        }
        public void Read(out short value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadInt16(this.IO_read_index);
                this.IO_read_index += sizeof(short);
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out short value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public void Read(int offset, out ushort value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadUint16(this.IO_read_index);
                this.IO_read_index += sizeof(ushort);
            }
        }
        public void Read(out ushort value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadUint16(this.IO_read_index);
                this.IO_read_index += sizeof(ushort);
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out ushort value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public void Read(int offset, out int value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadInt32(this.IO_read_index);
                this.IO_read_index += sizeof(int);
            }
        }
        public void Read(out int value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadInt32(this.IO_read_index);
                this.IO_read_index += sizeof(int);
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out int value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public void Read(int offset, out uint value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadUint32(this.IO_read_index);
                this.IO_read_index += sizeof(uint);
            }
        }
        public void Read(out uint value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadUint32(this.IO_read_index);
                this.IO_read_index += sizeof(uint);
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out uint value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public void Read(int offset, out long value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadInt64(this.IO_read_index);
                this.IO_read_index += sizeof(long);
            }
        }
        public void Read(out long value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadInt64(this.IO_read_index);
                this.IO_read_index += sizeof(long);
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out long value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public void Read(int offset, out ulong value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadUint64(this.IO_read_index);
                this.IO_read_index += sizeof(ulong);
            }
        }
        public void Read(out ulong value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadUint64(this.IO_read_index);
                this.IO_read_index += sizeof(ulong);
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out ulong value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public void Read(int offset, out string value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadString(this.IO_read_index);
                this.IO_read_index += value.Length;
            }
        }
        public void Read(out string value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadString(this.IO_read_index);
                this.IO_read_index += value.Length;
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out string value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public void Read(int offset, out List<string> value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadStringList(this.IO_read_index);
                this.IO_read_index += sizeof(byte);
                this.IO_read_index += value.Sum(p => p.Length);
            }
        }
        public void Read(out List<string> value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadStringList(this.IO_read_index);
                this.IO_read_index += sizeof(byte);
                this.IO_read_index += value.Sum(p => p.Length);
            }
        }
        public static void Read(FABMessageWatcher messageWatcher, int offset, out List<string> value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, out value);
            }
        }
        public unsafe void Read(int offset, int Length, void* value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                if (offset > this.IO_read_index)
                    this.IO_read_index = offset;
                value = this.data.ReadStruct(this.IO_read_index, Length);
                this.IO_read_index += Length;
            }
        }
        public unsafe void Read(int Length, void* value)
        {
            lock (this.syncRoot)
            {
                if (!this.InitializedIO)
                    InitIO();
                value = this.data.ReadStruct(this.IO_read_index, Length);
                this.IO_read_index += Length;
            }
        }
        public static unsafe void Read(FABMessageWatcher messageWatcher, int offset, int Length, void* value)
        {
            lock (messageWatcher.syncRoot)
            {
                if (!messageWatcher.InitializedIO)
                    messageWatcher.InitIO();
                messageWatcher.Read(offset, Length, value);
            }
        }
        #endregion
        public void Dispose()
        {
            this.data = null;
            this.opCode = OpCode.None;
            this.direction = Direction.None;
            this.IO_write_index = 0;
            this.IO_read_index = 0;
            this.InitializedIO = false;
        }
        #region Operators
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, byte value)
        {
            watcher.Write(value);
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, byte value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, sbyte value)
        {
            watcher.Write(value);
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, sbyte value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, short value)
        {
            watcher.Write(value);
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, short value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, ushort value)
        {
            watcher.Write(value);
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, ushort value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, int value)
        {
            watcher.Write(value);
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, int value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, uint value)
        {
            watcher.Write(value);
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, uint value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, long value)
        {
            watcher.Write(value);
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, long value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, ulong value)
        {
            watcher.Write(value);
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, ulong value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, string value)
        {
            watcher.Write(value);
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, string value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator <(FABMessageWatcher watcher, List<string> value)
        {
            watcher.Write(value.ToArray());
            return watcher;
        }
        public static FABMessageWatcher operator >(FABMessageWatcher watcher, List<string> value)
        {
            watcher.Read(out value);
            return watcher;
        }
        public static FABMessageWatcher operator !(FABMessageWatcher watcher)
        {
            return watcher == null ? null : watcher;
        }
        #endregion
    }
}
