using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore
{
    public interface IFABChannelGroupTaskCompletionSource : IEnumerator<Task>, IDisposable, IEnumerator
    {
        Task Find(IFABChannel channel);
        bool IsPartialFailure();
        bool IsPartialSucess();
        bool IsSucess();

        FABChannelGroupException Cause { get; }

        IFABChannelGroup Group { get; }
    }
}
