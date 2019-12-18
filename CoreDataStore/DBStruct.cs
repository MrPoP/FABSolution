using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreDataStore
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DBTableStruct
    {
        public string Name;
        public string CryptoGraphieKey;
        public ConcurrentDictionary<string, DBColum> Colums;
        public static DBTableStruct Create(string name, bool Crypted = false)
        {
            var table = new DBTableStruct()
            {
                Name = name,
                CryptoGraphieKey = string.Empty,
                Colums = new ConcurrentDictionary<string, DBColum>()
            };
            if (Crypted)
                table.CryptoGraphieKey = CoreResolver.RandomizedCryptionKey(name);
            return table;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DBColum
    {
        public string Name;
        public object UpdatedValue;
        public object DefaultValue;
        public bool AutoIncreasment;
        public bool UniqueKey;
        public List<DBColumOrderRecord> Records;
        public List<object> Values;
        public static DBColum Create(string name, object defaultvalue, bool autoincreasment, bool uniquekey)
        {
            return new DBColum()
            {
                Name = name,
                DefaultValue = defaultvalue,
                AutoIncreasment = autoincreasment,
                UniqueKey = uniquekey,
                Records = new List<DBColumOrderRecord>(),
                Values =new List<object>()
            };
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DBColumOrderRecord
    {
        public DBColumOrderRecordType Type;
        public long Time;
        public string Colum;
        public object oldValue;
        public object newValue;
        public static DBColumOrderRecord Create(DBColumOrderRecordType type, long time, DBColum colum, object newvalue)
        {
            return new DBColumOrderRecord()
            {
                Type = type,
                Time = time,
                Colum = colum.Name,
                oldValue = colum.UpdatedValue,
                newValue = newvalue
            };
        }
        public static DBColumOrderRecord Create(DBColumOrderRecordType type, long time, string colum, object oldvalue, object newvalue)
        {
            return new DBColumOrderRecord()
            {
                Type = type,
                Time = time,
                Colum = colum,
                oldValue = oldvalue,
                newValue = newvalue
            };
        }
    }
}
