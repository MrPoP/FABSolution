using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreProcess.Exceptions
{
    public class StockItemNotExistedException : Exception
    {
        public StockItemNotExistedException(string message)
            : base(message)
        {
        }
        public StockItemNotExistedException(string message, params object[] parameters)
            : base(string.Format(message, parameters))
        {
        }
    }
}
