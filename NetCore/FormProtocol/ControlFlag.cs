using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCore.FormProtocol
{
    [Flags]
    public enum ControlFlag
    {
        None = 1 << 0,
        Label = 1 << 1,
        Form = 1 << 255
    }
}
