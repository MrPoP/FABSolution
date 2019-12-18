using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCore.FormProtocol
{
    [Flags]
    public enum ContractFlag
    {
        Ping = 1 << 0,
        GotFocus = 1 << 1,
        GotFocusRespond = 1 << 2,
        TextChanged = 1 << 3,
        TextChangedRespond = 1 << 4,
        LostFocus = 1 << 5,
        LostFocusRespond = 1 << 6,
        MouseClick = 1 << 7,
        MouseClickRespond = 1 << 8,
        Validating = 1 << 9,
        ValidatingRespond = 1 << 10,
        Validated = 1 << 11,
        ValidatedRespond = 1 << 12,
    }
}
