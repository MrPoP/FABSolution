using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Structure
{
    public class RawItem
    {
        private double stock = 0.0;
        private Units unit = Units.None;
        private string name = string.Empty;
        private RawCountFlag flag = RawCountFlag.Daily;

        public double Stock { get { return Math.Round(this.stock, 3); } }
        public Units Unit { get { return this.unit; } }
        public string Name { get { return this.name; } }
        public RawCountFlag CountFlag { get { return this.flag; } }

        public RawItem(string itemname, Units countunit, double instock = 0.0, RawCountFlag _flag = RawCountFlag.Daily)
        {
            name = itemname;
            unit = countunit;
            flag = _flag;
            if (instock > 0.0)
            {
                if (instock > 1000.0)
                {
                    switch (countunit)
                    {
                        case Units.Gram:
                            {
                                instock /= 1000;
                                unit = Units.Kilogram;
                                break;
                            }
                        case Units.Millilitre:
                            {
                                instock /= 1000;
                                unit = Units.Litre;
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
            stock = instock;
        }

        public void EditCount(double newstock = 0.0)
        {
            if (newstock < 0.100)
            {
                switch (Unit)
                {
                    case Units.Kilogram:
                        {
                            newstock *= 1000;
                            unit = Units.Gram;
                            break;
                        }
                    case Units.Litre:
                        {
                            newstock *= 1000;
                            unit = Units.Millilitre;
                            break;
                        }
                    default:
                        break;
                }
            }
            if (newstock > 1000.0)
            {
                switch (Unit)
                {
                    case Units.Gram:
                        {
                            newstock /= 1000;
                            unit = Units.Kilogram;
                            break;
                        }
                    case Units.Millilitre:
                        {
                            newstock /= 1000;
                            unit = Units.Litre;
                            break;
                        }
                    default:
                        break;
                }
            }
            stock = newstock;
        }
        public void EditFlag(RawCountFlag _flag)
        {
            flag = _flag;
        }
        public void SetData(Units countunit, double instock = 0.0)
        {
            unit = countunit;
            stock = instock;
        }

        ~RawItem()
        {
            stock = 0.0;
            unit = Units.None;
            name = string.Empty;
            flag = RawCountFlag.Daily;
        }

        //growing
        public static double operator <(RawItem item1, RawItem item2)
        {
            return item1.stock + item2.UnitConversion(item1.unit);
        }
        //shortage
        public static double operator >(RawItem item1, RawItem item2)
        {
            return item1.stock - item2.UnitConversion(item1.unit);
        }
    }
}
