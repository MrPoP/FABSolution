using System;
using System.IO;
using System.Linq;
using SharpLzo;
using SlimMath;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Net;

namespace NetCore
{
    public static class FABExtensions
    {
        public static IPEndPoint Parse(string endpointstring)
        {
            return Parse(endpointstring, -1);
        }

        public static IPEndPoint Parse(string endpointstring, int defaultport)
        {
            if (string.IsNullOrEmpty(endpointstring)
                || endpointstring.Trim().Length == 0)
            {
                throw new ArgumentException("Endpoint descriptor may not be empty.");
            }

            if (defaultport != -1 &&
                (defaultport < IPEndPoint.MinPort
                || defaultport > IPEndPoint.MaxPort))
            {
                throw new ArgumentException(string.Format("Invalid default port '{0}'", defaultport));
            }

            string[] values = endpointstring.Split(new char[] { ':' });
            IPAddress ipaddy;
            int port = -1;

            //check if we have an IPv6 or ports
            if (values.Length <= 2) // ipv4 or hostname
            {
                if (values.Length == 1)
                    //no port is specified, default
                    port = defaultport;
                else
                    port = getPort(values[1]);

                //try to use the address as IPv4, otherwise get hostname
                if (!IPAddress.TryParse(values[0], out ipaddy))
                    ipaddy = getIPfromHost(values[0]);
            }
            else if (values.Length > 2) //ipv6
            {
                //could [a:b:c]:d
                if (values[0].StartsWith("[") && values[values.Length - 2].EndsWith("]"))
                {
                    string ipaddressstring = string.Join(":", values.Take(values.Length - 1).ToArray());
                    ipaddy = IPAddress.Parse(ipaddressstring);
                    port = getPort(values[values.Length - 1]);
                }
                else //[a:b:c] or a:b:c
                {
                    ipaddy = IPAddress.Parse(endpointstring);
                    port = defaultport;
                }
            }
            else
            {
                throw new FormatException(string.Format("Invalid endpoint ipaddress '{0}'", endpointstring));
            }

            if (port == -1)
                throw new ArgumentException(string.Format("No port specified: '{0}'", endpointstring));

            return new IPEndPoint(ipaddy, port);
        }

        private static int getPort(string p)
        {
            int port;

            if (!int.TryParse(p, out port)
             || port < IPEndPoint.MinPort
             || port > IPEndPoint.MaxPort)
            {
                throw new FormatException(string.Format("Invalid end point port '{0}'", p));
            }

            return port;
        }

        private static IPAddress getIPfromHost(string p)
        {
            var hosts = Dns.GetHostAddresses(p);

            if (hosts == null || hosts.Length == 0)
                throw new ArgumentException(string.Format("Host not found: {0}", p));

            return hosts[0];
        }
        public static string ToHexString(this byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789ABCDEF";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }

            return Result.ToString();
        }
        public static List<T> ToList<T>(this T[] arr)
        {
            List<T> list = new List<T>();
            arr.Foreach(p => list.Add(p));
            return list;
        }
        public static async void Foreach<T>(this T[] arr, Action<T> action)
        {
            await Task.Run(() => 
            {
                foreach (T element in arr)
                    action.Invoke(element);
            });
        }

        public static Vector2 ReadRotation(this BinaryReader r)
        {
            return new Vector2(r.ReadByte(), r.ReadByte());
        }

        public static void WriteRotation(this BinaryWriter w, Vector2 v)
        {
            w.Write((byte)v.X);
            w.Write((byte)v.Y);
        }

        public static short Compress(this float value)
        {
            var tmp = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
            var value1 = (tmp & 0x80000000) >> 0x1F;
            var value2 = ((tmp & 0x7F800000) >> 0x17) - 0x7F;
            var value3 = tmp & 0x7FFFFF;

            return (short)((uint)(value1 << 0xF) | (uint)((value2 + 0x7) << 0x9) | (uint)(value3 >> 0xE));
        }

        public static float Decompress(this ushort value)
        {
            var result = ((value & 0x1FF) << 14) | ((((value & 0x7F00) >> 9) - 7 + 127) << 23) | ((value & 0x8000) >> 15 << 31);
            return BitConverter.ToSingle(BitConverter.GetBytes(result), 0);
        }

        public static float Decompress(this short value)
        {
            var result = ((value & 0x1FF) << 14) | ((((value & 0x7F00) >> 9) - 7 + 127) << 23) | ((value & 0x8000) >> 15 << 31);
            return BitConverter.ToSingle(BitConverter.GetBytes(result), 0);
        }

        public static float ReadCompressedFloat(this BinaryReader r)
        {
            return r.ReadInt16().Decompress();
        }

        public static Vector3 ReadCompressedVector3(this BinaryReader r)
        {
            return new Vector3(r.ReadCompressedFloat(), r.ReadCompressedFloat(), r.ReadCompressedFloat());
        }

        public static void WriteCompressed(this BinaryWriter w, float value)
        {
            w.Write(value.Compress());
        }

        public static void WriteCompressed(this BinaryWriter w, Vector3 value)
        {
            w.WriteCompressed(value.X);
            w.WriteCompressed(value.Y);
            w.WriteCompressed(value.Z);
        }
        public static string nameof<T>(T element) where T : class
        {
            return getnameof(() => element);
        }
        public static string getnameof<T>(Expression<Func<T>> propertyLambda)
        {
            MemberExpression me = propertyLambda.Body as MemberExpression;
            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            string result = string.Empty;
            do
            {
                result = me.Member.Name + "." + result;
                me = me.Expression as MemberExpression;
            } while (me != null);

            result = result.Remove(result.Length - 1); // remove the trailing "."
            return result;
        }

        ///////UnsafeMethods
        public static unsafe sbyte ReadInt8(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(sbyte) > arr.Length, "Message length is too short.");
            fixed(byte* ptr = arr)
            {
                return *((sbyte*)ptr + offset);
            }
        }
        public static unsafe short ReadInt16(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(short) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                return *((short*)ptr + offset);
            }
        }
        public static unsafe int ReadInt32(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(int) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                return *((int*)ptr + offset);
            }
        }
        public static unsafe long ReadInt64(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(long) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                return *((long*)ptr + offset);
            }
        }
        public static unsafe byte ReadUint8(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(byte) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                return *((byte*)ptr + offset);
            }
        }
        public static unsafe ushort ReadUint16(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(ushort) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                return *((ushort*)ptr + offset);
            }
        }
        public static unsafe uint ReadUint32(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(uint) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                return *((uint*)ptr + offset);
            }
        }
        public static unsafe ulong ReadUint64(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(ulong) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                return *((ulong*)ptr + offset);
            }
        }
        public static unsafe string ReadString(this byte[] arr, int offset, int count)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + count > arr.Length, "Message length is too short.");
            string val = null;
            fixed(byte* ptr = arr)
            {
                val = new string((sbyte*)ptr, offset, count);
                int idx = val.IndexOf('\0');
                val = (idx > -1) ? val.Substring(0, idx) : val;
            }
            return val;
        }
        public static unsafe string ReadString(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            byte count = 0;
            fixed (byte* ptr = arr)
            {
                count = *((byte*)ptr + offset++);
            }
            Debug.Assert(count == 0, "Couldn't find count member in message.");
            Debug.Assert(offset + count > arr.Length, "Message length is too short.");
            string val = null;
            fixed (byte* ptr = arr)
            {
                val = new string((sbyte*)ptr, offset, count);
                int idx = val.IndexOf('\0');
                val = (idx > -1) ? val.Substring(0, idx) : val;
            }
            return val;
        }
        public static unsafe List<string> ReadStringList(this byte[] arr, int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            byte count = 0;
            fixed (byte* ptr = arr)
            {
                count = *((byte*)ptr + offset++);
            }
            Debug.Assert(count == 0, "Couldn't find count member in message.");
            List<string> vals = new List<string>();
            while(count > 0)
            {
                string value = arr.ReadString(offset);
                offset += value.Length;
                count--;
            }
            return vals;
        }
        public static unsafe void* ReadStruct(this byte[] arr, int offset, int count)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + count > arr.Length, "Message length is too short.");
            void* buf = null;
            fixed(byte* ptr = arr)
            {
                memcpy(buf, ptr + offset, count);
            }
            return buf;
        }
        public unsafe static void memcpy(void* dest, void* src, Int32 size)
        {
            Int32 count = size / sizeof(long);
            for (Int32 i = 0; i < count; i++)
                *(((long*)dest) + i) = *(((long*)src) + i);

            Int32 pos = size - (size % sizeof(long));
            for (Int32 i = 0; i < size % sizeof(long); i++)
                *(((Byte*)dest) + pos + i) = *(((Byte*)src) + pos + i);
        }
        public static byte[] CloneBaseMessage(this byte[] arr, int MessageFlag)
        {
            byte[] ptr =new byte[arr.Length - 3];
            switch(MessageFlag)
            {
                default:
                    {
                        for (int x = 0, l = 0; x < arr.Length; x++)
                        {
                            if (x == 0 || x == 1 || x == arr.Length - 1)
                                continue;
                            ptr[l] = arr[x];
                        }
                        break;
                    }
            }
            return ptr;
        }
        public static unsafe void Write(this byte[] arr, byte value, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(byte) > arr.Length, "Message length is too short.");
            fixed(byte* ptr = arr)
            {
                *((byte*)ptr + offset) = value;
            }
            offset += sizeof(byte);
        }
        public static unsafe void Write(this byte[] arr, sbyte value, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(sbyte) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                *((sbyte*)ptr + offset) = value;
            }
            offset += sizeof(sbyte);
        }
        public static unsafe void Write(this byte[] arr, short value, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(short) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                *((short*)ptr + offset) = value;
            }
            offset += sizeof(short);
        }
        public static unsafe void Write(this byte[] arr, ushort value, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(ushort) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                *((ushort*)ptr + offset) = value;
            }
            offset += sizeof(ushort);
        }
        public static unsafe void Write(this byte[] arr, int value, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(int) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                *((int*)ptr + offset) = value;
            }
            offset += sizeof(int);
        }
        public static unsafe void Write(this byte[] arr, uint value, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(uint) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                *((uint*)ptr + offset) = value;
            }
            offset += sizeof(uint);
        }
        public static unsafe void Write(this byte[] arr, long value, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(long) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                *((long*)ptr + offset) = value;
            }
            offset += sizeof(long);
        }
        public static unsafe void Write(this byte[] arr, ulong value, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + sizeof(ulong) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                *((ulong*)ptr + offset) = value;
            }
            offset += sizeof(ulong);
        }
        public static unsafe void Write(this byte[] arr, string value, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + value.Length > arr.Length, "Message length is too short.");
            Debug.Assert(offset + value.Length + 1 > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr, ptr2 = Constants.Encoder.GetBytes(value))
            {
                *((byte*)ptr + offset++) = (byte)value.Length;
                memcpy((ptr + offset), ptr2, value.Length);
                offset += value.Length;
            }
        }
        public static unsafe void Write(this byte[] arr, ref int offset, params string[] values)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + values.Length > arr.Length, "Message length is too short.");
            Debug.Assert(offset + values.Length + values.Sum((p) => p.Length) > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                *((byte*)ptr + offset++) = (byte)values.Length;
            }
            for (int i = 0; i < values.Length; i++)
            {
                var str = values[i];
                if (string.IsNullOrEmpty(str))
                {
                    arr.Write((byte)0, ref offset);
                    continue;
                }
                arr.Write(str, ref offset);
            }
        }
        public static unsafe void Write(this byte[] arr, void* value, int length, ref int offset)
        {
            Debug.Assert(offset > arr.Length, "Message length is too short.");
            Debug.Assert(offset + length > arr.Length, "Message length is too short.");
            fixed (byte* ptr = arr)
            {
                memcpy(ptr + offset, value, length);
                offset += length;
            }
        }
        //////////////////////
        
    }
    public static class DynamicCast<TTarget>
    {
        public static TTarget From<TSource>(TSource value)
        {
            return FunctionCache<TSource>.Function(value);
        }

        private static class FunctionCache<TSource>
        {
            public static Func<TSource, TTarget> Function { get { return Generate(); } }

            private static Func<TSource, TTarget> Generate()
            {
                var parameter = Expression.Parameter(typeof(TSource));
                var convert = Expression.ConvertChecked(parameter, typeof(TTarget));
                return Expression.Lambda<Func<TSource, TTarget>>(convert, parameter).Compile();
            }
        }
    }
}
