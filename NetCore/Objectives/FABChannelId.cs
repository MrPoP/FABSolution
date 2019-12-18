using DotNetty.Buffers;
using DotNetty.Common;
using DotNetty.Common.Internal;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NetCore
{
    public sealed class FABChannelId : IFABChannelId
    {
        const int MachineIdLen = 8;
        const int ProcessIdLen = 4;
        const int MaxProcessId = 4194304;
        const int SequenceLen = 4;
        const int TimestampLen = 8;
        const int RandomLen = 4;
        static readonly Regex MachineIdPattern = new Regex("^(?:[0-9a-fA-F][:-]?){6,8}$");
        static readonly byte[] MachineId;
        static readonly int ProcessId;
        static int nextSequence;
        static int seed = (int)(Stopwatch.GetTimestamp() & 0xFFFFFFFF); //used to safly cast long to int, because the timestamp returned is long and it doesn't fit into an int
        static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed))); //used to simulate java ThreadLocalRandom
        readonly byte[] data = new byte[MachineIdLen + ProcessIdLen + SequenceLen + TimestampLen + RandomLen];
        int hashCode;
        string longValue;

        string shortValue;
        static FABChannelId()
        {
            int processId = -1;
            string customProcessId = SystemPropertyUtil.Get("io.netty.processId");
            if (customProcessId != null)
            {
                if (!int.TryParse(customProcessId, out processId))
                {
                    processId = -1;
                }
                if (processId < 0 || processId > MaxProcessId)
                {
                    processId = -1;
                }
            }
            if (processId < 0)
            {
                processId = DefaultProcessId();
            }
            ProcessId = processId;
            byte[] machineId = null;
            string customMachineId = SystemPropertyUtil.Get("io.netty.machineId");
            if (customMachineId != null)
            {
                if (MachineIdPattern.Match(customMachineId).Success)
                {
                    machineId = ParseMachineId(customMachineId);
                }
            }

            if (machineId == null)
            {
                machineId = DefaultMachineId();
            }
            MachineId = machineId;
        }
        public string AsShortText()
        {
            string asShortText = this.shortValue;
            if (asShortText == null)
            {
                this.shortValue = asShortText = ByteBufferUtil.HexDump(this.data, MachineIdLen + ProcessIdLen + SequenceLen + TimestampLen, RandomLen);
            }

            return asShortText;
        }
        public string AsLongText()
        {
            string asLongText = this.longValue;
            if (asLongText == null)
            {
                this.longValue = asLongText = this.NewLongValue();
            }
            return asLongText;
        }
        public int CompareTo(IFABChannelId other) { return 0; }
        static byte[] ParseMachineId(string value)
        {
            // Strip separators.
            value = value.Replace("[:-]", "");
            var machineId = new byte[MachineIdLen];
            for (int i = 0; i < value.Length; i += 2)
            {
                machineId[i] = (byte)int.Parse(value.Substring(i, i + 2), NumberStyles.AllowHexSpecifier);
            }
            return machineId;
        }
        static int DefaultProcessId()
        {
            int pId = Platform.GetCurrentProcessId();

            if (pId <= 0)
            {
                pId = ThreadLocalRandom.Value.Next(MaxProcessId + 1);
            }
            return pId;
        }
        public static FABChannelId NewInstance()
        {
            var id = new FABChannelId();
            id.Init();
            return id;
        }
        static byte[] DefaultMachineId()
        {
            byte[] bestMacAddr = Platform.GetDefaultDeviceId();
            if (bestMacAddr == null)
            {
                bestMacAddr = new byte[MacAddressUtil.MacAddressLength];
                ThreadLocalRandom.Value.NextBytes(bestMacAddr);
            }
            return bestMacAddr;
        }
        string NewLongValue()
        {
            var buf = new StringBuilder(2 * this.data.Length + 5);
            int i = 0;
            i = this.AppendHexDumpField(buf, i, MachineIdLen);
            i = this.AppendHexDumpField(buf, i, ProcessIdLen);
            i = this.AppendHexDumpField(buf, i, SequenceLen);
            i = this.AppendHexDumpField(buf, i, TimestampLen);
            i = this.AppendHexDumpField(buf, i, RandomLen);
            Debug.Assert(i == this.data.Length);
            return buf.ToString().Substring(0, buf.Length - 1);
        }
        int AppendHexDumpField(StringBuilder buf, int i, int length)
        {
            buf.Append(ByteBufferUtil.HexDump(this.data, i, length));
            buf.Append('-');
            i += length;
            return i;
        }
        void Init()
        {
            int i = 0;
            // machineId
            Array.Copy(MachineId, 0, this.data, i, MachineIdLen);
            i += MachineIdLen;

            // processId
            i = this.WriteInt(i, ProcessId);

            // sequence
            i = this.WriteInt(i, Interlocked.Increment(ref nextSequence));

            // timestamp (kind of)
            long ticks = Stopwatch.GetTimestamp();
            long nanos = (ticks / Stopwatch.Frequency) * 1000000000;
            long millis = (ticks / Stopwatch.Frequency) * 1000;
            i = this.WriteLong(i, ByteBufferUtil.SwapLong(nanos) ^ millis);

            // random
            int random = ThreadLocalRandom.Value.Next();
            this.hashCode = random;
            i = this.WriteInt(i, random);

            Debug.Assert(i == this.data.Length);
        }
        int WriteInt(int i, int value)
        {
            uint val = (uint)value;
            this.data[i++] = (byte)(val >> 24);
            this.data[i++] = (byte)(val >> 16);
            this.data[i++] = (byte)(val >> 8);
            this.data[i++] = (byte)value;
            return i;
        }

        int WriteLong(int i, long value)
        {
            ulong val = (ulong)value;
            this.data[i++] = (byte)(val >> 56);
            this.data[i++] = (byte)(val >> 48);
            this.data[i++] = (byte)(val >> 40);
            this.data[i++] = (byte)(val >> 32);
            this.data[i++] = (byte)(val >> 24);
            this.data[i++] = (byte)(val >> 16);
            this.data[i++] = (byte)(val >> 8);
            this.data[i++] = (byte)value;
            return i;
        }

        public override int GetHashCode() { return this.hashCode; }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (!(obj is FABChannelId))
            {
                return false;
            }

            return Equals(this.data, ((FABChannelId)obj).data);
        }

        public override string ToString() {return this.AsShortText();}
    }
}
