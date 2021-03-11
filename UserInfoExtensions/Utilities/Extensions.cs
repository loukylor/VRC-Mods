using System.Threading.Tasks;

namespace UserInfoExtentions.Utilities
{
    public static class Extensions
    {
        public static Task<TResult> NoAwait<TResult>(this Task<TResult> task)
        {
            task.ContinueWith(tsk =>
            {
                if (tsk.IsFaulted)
                    MelonLoader.MelonLogger.Error($"Free-floating Task failed with exception: {tsk.Exception}");
            });
            return task;
        }
    }
}
