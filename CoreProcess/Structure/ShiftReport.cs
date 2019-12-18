using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct ShiftReport
    {
        public static ShiftReport Empty = default(ShiftReport);

        public byte Flag;
        public int DayID;
        public int CheckIN_Hour;
        public int CheckIN_Minute;
        public int CheckOUT_Hour;
        public int CheckOUT_Minute;
        public int MonthID;
        public float Speed;
        public float Service;
        public float Quality;
        public float Precision;
        public float Cleanless;

        public static ShiftReport Create(byte _Flag, int _DayID, int _CheckIN_Hour, int _CheckIN_Minute, int _CheckOUT_Hour,
            int _CheckOUT_Minute, int _MonthID, float _Speed, float _Service, float _Quality, float _Precision, float _Cleanless)
        {
            return new ShiftReport()
            {
                Flag = _Flag,
                DayID = _DayID,
                CheckIN_Hour = _CheckIN_Hour,
                CheckIN_Minute = _CheckIN_Minute,
                CheckOUT_Hour = _CheckOUT_Hour,
                CheckOUT_Minute = _CheckOUT_Minute,
                MonthID = _MonthID,
                Speed = _Speed,
                Service = _Service,
                Quality = _Quality,
                Precision = _Precision,
                Cleanless = _Cleanless
            };
        }
    }
}
