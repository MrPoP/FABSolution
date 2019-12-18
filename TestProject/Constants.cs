using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    [Flags]
    public enum ThreadType
    {
        Invocator,
        MultiThreading,
        LongRunning
    }
    [Flags]
    public enum ThreadStatus
    {
        Active = 1,
        Dead = 2
    }
}
