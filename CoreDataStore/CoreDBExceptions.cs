using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreDataStore
{
    public class DuplicatedUniqueValue : Exception
    {
        public DuplicatedUniqueValue(string message, params object[] args)
            : base(string.Format(message, args)) { }
    }
    public class WrongParameterCodeType : Exception
    {
        public WrongParameterCodeType(string message, params object[] args)
            : base(string.Format(message, args)) { }
    }
}
