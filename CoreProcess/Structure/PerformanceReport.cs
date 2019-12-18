using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public struct PerformanceReport
    {
        public static PerformanceReport Empty = default(PerformanceReport);

        public float Speed;
        public float Service;
        public float Quality;
        public float Precision;
        public float Cleanless;

        public static PerformanceReport Create(float _Speed, float _Service, float _Quality, float _Precision, float _Cleanless)
        {
            return new PerformanceReport()
            {
                Speed = _Speed,
                Service = _Service,
                Quality = _Quality,
                Precision = _Precision,
                Cleanless = _Cleanless
            };
        }
    }
}
