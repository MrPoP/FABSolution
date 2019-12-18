using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.Collections.Generic
{
    public interface IBag
    {
        int Length { get; }
        void Add(params object[] param);
        void Remove(int index);
        int[] IndexsOf(params object[] param);
        object this[int index, int place] { get; set; }
    }
    internal interface IBagInternal : IBag
    {
        string ToString(StringBuilder sb);
        int GetHashCode(IEqualityComparer comparer);
    }
}
