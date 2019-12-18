using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public static class Constants
    {
        public const int MessageFileMaxSize = 1024;
        public static Encoding Encoder { get { return Encoding.UTF8; } }
    }
}
