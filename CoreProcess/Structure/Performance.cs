using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public class Performance
    {
        public static readonly Performance Maximum = new Performance() { speed = 2f, cleanless = 2f, precision = 2f, quality = 2f, service = 2f };
        public static readonly Performance Minimum = new Performance() { speed = -2f, cleanless = -2f, precision = -2f, quality = -2f, service = -2f };
        public static readonly Performance Ideal = new Performance() { speed = 1f, cleanless = 1f, precision = 1f, quality = 1f, service = 1f };
        public static readonly float MaxValue = 2f, MinValue = -2f;
        private float speed = 1f;
        private float service = 1f;
        private float quality = 1f;
        private float precision = 1f;
        private float cleanless = 1f;

        public float SpeedIndicator { get { return this.speed; } }
        public float ServiceIndicator { get { return this.service; } }
        public float QualityIndicator { get { return this.quality; } }
        public float PrecisionIndicator { get { return this.precision; } }
        public float CleanlessIndicator { get { return this.cleanless; } }

        public void AddSpeed(float input)
        {
            if (MaxValue < speed + input)
            {
                speed = MaxValue;
                return;
            }
            speed += input;
        }
        public void RemoveSpeed(float input)
        {
            if (MinValue > cleanless - input)
            {
                cleanless = MinValue;
                return;
            }
            speed -= input;
        }

        public void AddService(float input)
        {
            if (MaxValue < service + input)
            {
                service = MaxValue;
                return;
            }
            service += input;
        }
        public void RemoveService(float input)
        {
            if (MinValue > service - input)
            {
                service = MinValue;
                return;
            }
            service -= input;
        }

        public void AddQuality(float input)
        {
            if (MaxValue < quality + input)
            {
                quality = MaxValue;
                return;
            }
            quality += input;
        }
        public void RemoveQualiy(float input)
        {
            if (MinValue > quality - input)
            {
                quality = MinValue;
                return;
            }
            quality -= input;
        }

        public void AddPrecision(float input)
        {
            if (MaxValue < precision + input)
            {
                precision = MaxValue;
                return;
            }
            precision += input;
        }
        public void RemovePrecision(float input)
        {
            if (MinValue > precision - input)
            {
                precision = MinValue;
                return;
            }
            precision -= input;
        }

        public void AddCleanless(float input)
        {
            if (MaxValue < cleanless + input)
            {
                cleanless = MaxValue;
                return;
            }
            cleanless += input;
        }
        public void RemoveCleanless(float input)
        {
            if (MinValue > cleanless - input)
            {
                cleanless = MinValue;
                return;
            }
            cleanless -= input;
        }

        public static implicit operator PerformanceReport(Performance performance)
        {
            return PerformanceReport.Create(performance.speed, performance.service, performance.quality, performance.precision, performance.cleanless);
        }
        public static implicit operator Performance(PerformanceReport performance)
        {
            return new Performance() 
            { 
                speed = performance.Speed, 
                service = performance.Service,
                quality = performance.Quality,
                precision = performance.Precision,
                cleanless = performance.Cleanless 
            };
        }
    }
}
