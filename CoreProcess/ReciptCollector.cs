using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreProcess.Structure;

namespace CoreProcess
{
    public static class ReciptCollector
    {//need to handle gathering data from server :D
        static Dictionary<DepartmentFlag, Dictionary<Product, Dictionary<DepartmentFlag, List<RawItem>>>> Receipts =
            new Dictionary<DepartmentFlag, Dictionary<Product, Dictionary<DepartmentFlag, List<RawItem>>>>();

        public static IEnumerable<Structure.Product> SelectProducts(Cheque cheque, DepartmentFlag flag = DepartmentFlag.None)
        {
            if (flag != DepartmentFlag.None)
            {
                if (Receipts.ContainsKey(flag))
                {
                    foreach (var product in Receipts[flag].Select(x => x.Key).Where(p => cheque.Contains(p)))
                        yield return product;
                }
                yield break;
            }
            else
            {
                foreach (var product in Receipts.Values.SelectMany(x => x.Keys).Where(p => cheque.Contains(p)))
                    yield return product;
                yield break;
            }
        }

        public static IEnumerable<Structure.RawItem> SelectRawItems(Product Product, DepartmentFlag flag = DepartmentFlag.None)
        {
            if (flag != DepartmentFlag.None)
            {
                foreach (var rawitem in Receipts.Values.Where(p => p.Select(x => x.Key).Contains(Product)).SelectMany(x => x.Values)
                    .Where(p => p.Select(x => x.Key).Contains(flag)).SelectMany(x => x.Values).SelectMany(x => x))
                {
                    yield return rawitem;
                }
                yield break;
            }
            else
            {
                foreach (var rawitem in Receipts.Values.Where(p => p.Select(x => x.Key).Contains(Product)).SelectMany(x => x.Values)
                    .SelectMany(x => x.Values).SelectMany(x => x))
                {
                    yield return rawitem;
                }
                yield break;
            }
        }
    }
}
