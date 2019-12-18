using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Exceptions;

namespace CoreProcess
{
    public static class UnitConverter
    {//need to handle gathering data from server :D
        static Dictionary<string, Dictionary<Units, Func<Units, double>>> Logical = 
            new Dictionary<string, Dictionary<Units, Func<Units, double>>>();

        public static double Convert(Structure.RawItem item, Units to)
        {
            if (Logical.ContainsKey(item.Name))
            {
                if (Logical[item.Name].ContainsKey(item.Unit))
                {
                    return Logical[item.Name][item.Unit].Invoke(to);
                }
                else
                {
                    throw new StockItemNotExistedException("[UnitConverter] failed to find unit {0} => [from] for item {1}", item.Unit.ToString(), item.Name);
                }
            }
            else
            {
                throw new StockItemNotExistedException("[UnitConverter] failed to find item {0}", item.Name);
            }
        }
    }
}
