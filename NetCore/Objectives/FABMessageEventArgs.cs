using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCore
{
    public class FABMessageEventArgs
    {
        readonly FABMessage message;

        public FABMessageEventArgs(FABMessage message)
        {
            this.message = message;
        }
    }
}
