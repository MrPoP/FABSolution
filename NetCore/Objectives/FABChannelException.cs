using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public class FABChannelException : Exception
    {
        public FABChannelException()
        {
        }

        public FABChannelException(Exception innerException) : base(null, innerException)
        {
        }

        public FABChannelException(string message) : base(message)
        {
        }

        public FABChannelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
