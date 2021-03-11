using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;


namespace UserInfoExtentions.Utilities
{
    class AsyncUtils
    {
        public static ConcurrentQueue<Action> toMainThreadQueue = new ConcurrentQueue<Action>();

        // Thank you knah for teaching me this (this is literally his code copied and pasted so)
        public static MainThreadAwaitable YieldToMainThread()
        {
            return new MainThreadAwaitable();
        }

        public struct MainThreadAwaitable : INotifyCompletion
        {
            public bool IsCompleted => false;

            public MainThreadAwaitable GetAwaiter() => this;

            public void GetResult() { }

            public void OnCompleted(Action continuation)
            {
                toMainThreadQueue.Enqueue(continuation);
            }
        }

    }
}
