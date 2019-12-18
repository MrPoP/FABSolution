using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Structure;

namespace CoreProcess
{
    public static class Extensions
    {
        public static string nameof<T>(this T element)
        {
            return element.ToString();
        }

        static double[] values = { 0.25, 0.5, 1.0, 5.0, 10.0, 20.0, 50.0, 100.0, 200.0 };

        public static double Value<T>(this T element) where T : Tuple<MoneyUnits, byte>
        {
            return Math.Round(values[(byte)(element as Tuple<MoneyUnits, byte>).Item1 - 1] * (element as Tuple<MoneyUnits, byte>).Item2, 2);
        }

        public static double UnitConversion<T>(this T rawitem, Units ConvertTo) where T : Structure.RawItem
        {
            return UnitConverter.Convert(rawitem, ConvertTo);
        }

        public static IEnumerable<RawItem> GetRecipe<T>(this T product) where T : Structure.Product
        {
            foreach (RawItem Item in ReciptCollector.SelectRawItems(product))
            {
                yield return Item;
            }
            yield break;
        }
        public static IEnumerable<Product> GetProducts<T>(this T cheque, DepartmentFlag flag) where T : Structure.Cheque
        {
            foreach (Product Item in ReciptCollector.SelectProducts(cheque, flag))
            {
                yield return Item;
            }
            yield break;
        }
        public static IEnumerable<RawItem> GetRecipe<T>(this T product, DepartmentFlag flag) where T : Product
        {
            foreach (RawItem Item in ReciptCollector.SelectRawItems(product, flag))
            {
                yield return Item;
            }
            yield break;
        }
    }
}
