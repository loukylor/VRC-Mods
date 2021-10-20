using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// A class that, when inherited, has methods that call at the same time as the MelonMod methods.
    /// Primarily to make OnApplicationStart, UIInit, etc. easier to subscribe to.
    /// Overrides for any Update methods are unavailable for performance reasons.
    /// </summary>
    public class MelonLoaderEvents
    {
        public virtual void OnApplicationStart() { }
        public virtual void OnUiManagerInit() { }
        public virtual void OnSceneWasLoaded(int buildIndex, string sceneName) { }
        public virtual void OnSceneWasInitialized(int buildIndex, string sceneName) { }
        public virtual void OnSceneWasUnloaded(int buildIndex, string sceneName) { }
        public virtual void OnApplicationQuit() { }

        public MelonLoaderEvents() { }
    }

    /// <summary>
    /// An attribute that effectively acts the same way as MelonLoaderEvents, except as an attribute and not inherited class.
    /// Mainly meant so static classes don't need to be instance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MelonLoaderEventsAttribute : Attribute
    {
        public MelonLoaderEventsAttribute() { }
    }
}
