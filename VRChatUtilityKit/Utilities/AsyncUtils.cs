using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// A set of async utilities.
    /// </summary>
    public static class AsyncUtils
    {
        internal static ConcurrentQueue<Action> _toMainThreadQueue = new ConcurrentQueue<Action>();

        // Thank you knah for teaching me this (this is literally his code copied and pasted so)
        /// <summary>
        /// Await this to force the rest of the method body to run on the main thread.
        /// </summary>
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
                _toMainThreadQueue.Enqueue(continuation);
            }
        }

    }
}
