using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public interface IFABServerChannel : IFABChannel, IAttributeMap, IComparable<IFABChannel>
    {
    }
}
