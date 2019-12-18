using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public abstract class ICurrency
    {
        double[] values = { 0.25, 0.5, 1.0, 5.0, 10.0, 20.0, 50.0, 100.0, 200.0 };

        public double Value
        {
            get
            {
                if (_unit == MoneyUnits.None)
                    return 0.0;
                else if ((byte)_unit >= 1 || (byte)_unit <= 9)
                    return Math.Round(values[(byte)_unit - 1] * _count, 2);
                else
                    return 0.0;
            }
        }
        public MoneyUnits _unit = MoneyUnits.None;
        public byte _count = 0;
        public abstract double this[byte count, MoneyUnits unit] { get; }
    }
}
