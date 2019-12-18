using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreDataStore
{
    static class CoreResolver
    {
        static readonly Dictionary<char, byte> FormalEncoding = new Dictionary<char, byte>()
        {
            {'A', 0}, {'B', 1}, {'C', 2}, {'D', 3}, {'E', 4}, {'F', 5}, {'G', 6}, {'H', 7}, {'I', 8}, {'J', 9}, {'K', 10},
            {'L', 11}, {'M', 12}, {'N', 13}, {'O', 14}, {'P', 15}, {'Q', 16}, {'R', 17}, {'S', 18}, {'T', 19}, {'U', 20}, {'V', 21},
            {'W', 22}, {'X', 23}, {'Y', 24}, {'Z', 25}, {'0', 26}, {'1', 27}, {'2', 28}, {'3', 29}, {'4', 30}, {'5', 31}, {'6', 32},
            {'7', 33}, {'8', 34}, {'9', 35}, {'a', 36}, {'b', 37}, {'c', 38}, {'d', 39}, {'e', 40}, {'f', 41}, {'g', 42}, {'h', 43},
            {'i', 44}, {'j', 45}, {'k', 46}, {'l', 47}, {'m', 48}, {'n', 49}, {'o', 50}, {'p', 51}, {'q', 52}, {'r', 53}, {'s', 54},
            {'t', 55}, {'u', 56}, {'v', 57}, {'w', 58}, {'x', 59}, {'y', 60}, {'z', 61},
            {'أ', 62}, {'ب', 63}, {'ت', 64}, {'ث', 65}, {'ج', 66}, {'ح', 67}, {'خ', 68}, {'د', 69}, {'ذ', 70}, {'ر', 71}, {'ز', 72},
            {'س', 73}, {'ش', 74}, {'ص', 75}, {'ض', 76}, {'ط', 77}, {'ظ', 78}, {'ع', 79}, {'غ', 80}, {'ف', 81}, {'ق', 82}, {'ك', 83},
            {'ل', 84}, {'م', 85}, {'ن', 86}, {'ه', 87}, {'و', 88}, {'ي', 89}
        };
        public static Func<CoreStruct, string> TableName = (s) => s.Name;
        public static Func<CoreStruct, bool> Cryptable = (s) => s.Crypted;

        public static Func<CoreProperty, string> ColumName = (c) => c.Name;
        public static Func<CoreProperty, bool> UniqueKey = (c) => c.Unique;
        public static Func<CoreProperty, object, object> AutoIncreasment = (c, cv) => autoIncreasment(c, c.AutoIncreasment, cv);
        static Func<CoreProperty, bool, object, object> autoIncreasment = (c, a, cv) => a ? Increasable(c, cv) : null;

        public static Func<DBColum, bool> UniqueDBKey = (c) => c.UniqueKey;
        public static Func<DBColum, int> ValuesCount = (c) => c.Values.Count;
        public static Func<DBColum, object, bool> CanUse = (dc, o) => ReturnType(dc.DefaultValue.GetType()) == ReturnType(o.GetType());

        public static Func<Type, TypeCode> ReturnType = (o) => Type.GetTypeCode(o);
        public static Func<string, string> RandomizedCryptionKey = (input) => CryptographicKey(input);
        public static Func<long> GetTime;

        public static readonly Func<CoreProperty, bool> DefinedAttribute =
            p => !string.IsNullOrEmpty(p.Name) && !string.IsNullOrWhiteSpace(p.Name) && p.DefaultValue != null &&
                !string.IsNullOrEmpty(p.Table) && !string.IsNullOrWhiteSpace(p.Table);
        static object Increasable(CoreProperty attribute, object currentvalue = null)
        {
            switch (ReturnType(currentvalue.GetType()))
            {
                case TypeCode.DateTime:
                    {
                        return DateTime.Now.ToBinary();
                    }
                case TypeCode.Single:
                    {
                        float current = 0;
                        if (Single.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == float.MaxValue)
                                return float.MaxValue;
                            return current += Single.Epsilon;
                        }
                        return null;
                    }
                case TypeCode.UInt64:
                    {
                        ulong current = 0;
                        if (ulong.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == ulong.MaxValue)
                                return ulong.MaxValue;
                            return ++current;
                        }
                        return null;
                    }
                case TypeCode.UInt32:
                    {
                        uint current = 0;
                        if (uint.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == uint.MaxValue)
                                return uint.MaxValue;
                            return ++current;
                        }
                        return null;
                    }
                case TypeCode.UInt16:
                    {
                        ushort current = 0;
                        if (ushort.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == ushort.MaxValue)
                                return ushort.MaxValue;
                            return ++current;
                        }
                        return null;
                    }
                case TypeCode.Double:
                    {
                        double current = 0;
                        if (double.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == double.MaxValue)
                                return double.MaxValue;
                            return current += double.Epsilon;
                        }
                        return null;
                    }
                case TypeCode.Decimal:
                    {
                        decimal current = 0;
                        if (decimal.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == decimal.MaxValue)
                                return decimal.MaxValue;
                            return ++current;
                        }
                        return null;
                    }
                case TypeCode.Int64:
                    {
                        long current = 0;
                        if (long.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == long.MaxValue)
                                return long.MaxValue;
                            return ++current;
                        }
                        return null;
                    }
                case TypeCode.Int32:
                    {
                        int current = 0;
                        if (int.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == int.MaxValue)
                                return int.MaxValue;
                            return ++current;
                        }
                        return null;
                    }
                case TypeCode.Int16:
                    {
                        short current = 0;
                        if (short.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == short.MaxValue)
                                return short.MaxValue;
                            return ++current;
                        }
                        return null;
                    }
                case TypeCode.Byte:
                    {
                        byte current = 0;
                        if(byte.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == byte.MaxValue)
                                return byte.MaxValue;
                            return ++current;
                        }
                        return null;
                    }
                case TypeCode.SByte:
                    {
                        sbyte current = 0;
                        if (sbyte.TryParse(currentvalue.ToString(), out current))
                        {
                            if (current == sbyte.MaxValue)
                                return sbyte.MaxValue;
                            return ++current;
                        }
                        return null;
                    }
                case TypeCode.Char:
                    {
                        byte charval = (byte)((char)currentvalue);
                        if (charval == byte.MaxValue)
                            return byte.MaxValue;
                        return ++charval;
                    }
                case TypeCode.Boolean:
                    {
                        return (byte)currentvalue == 0 ? true : false;
                    }
                case TypeCode.String:
                    {
                        List<byte> bytes = Encoding.ASCII.GetBytes((string)currentvalue).ToList();
                        bytes.ForEach(p => 
                        {
                            if (p < byte.MaxValue)
                            {
                                ++p;
                            }
                            else if (p == byte.MaxValue)
                                p = byte.MinValue;
                            else
                                ++p;
                        });
                        return Encoding.ASCII.GetString(bytes.ToArray());
                    }
                default:
                    {
                        return null;
                    }
            }
        }
        static string CryptographicKey(string input)
        {
            int RequiredLength = 16 - input.Length;
            if(input.Length > 16)
                RequiredLength = 16 - (input.Length - 16);
            List<byte> keyiv = new List<byte>(16);
            while(RequiredLength > 0)
            {
                int Random = new Random().Next(0, 90);
                while (Random == 0 || !FormalEncoding.ContainsValue((byte)Random))
                    Random = new Random().Next(0, 90);
                keyiv.Add((byte)Random);
            }
            return Encoding.UTF8.GetString(keyiv.ToArray());
        }
    }
}
