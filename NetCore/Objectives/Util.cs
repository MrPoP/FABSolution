using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NetCore.Message
{
    internal static class Util
    {
        public static void SafeSetSuccess(TaskCompletionSource promise)
        {
            if (promise != TaskCompletionSource.Void && !promise.TryComplete())
            {
                throw new FABChannelPipelineException("Failed to mark a promise as success because it is done already: {0}", promise);
            }
        }
        public static void SafeSetFailure(TaskCompletionSource promise, Exception cause)
        {
            if (promise != TaskCompletionSource.Void && !promise.TrySetException(cause))
            {
                throw new FABChannelPipelineException("Failed to mark a promise as failure because it's done already: {0} , {1}", promise, cause);
            }
        }
        public static void CloseSafe(this IFABChannel channel)
        {
            CompleteChannelCloseTaskSafely(channel, channel.CloseAsync());
        }

        public static void CloseSafe(this IFABChannelUnsafe u)
        {
            CompleteChannelCloseTaskSafely(u, u.CloseAsync());
        }

        internal static async void CompleteChannelCloseTaskSafely(object channelObject, Task closeTask)
        {
            try
            {
                await closeTask;
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                throw new FABChannelPipelineException("Failed to close channel " + channelObject + " cleanly {0}.", ex);
            }
        }
    }
}
