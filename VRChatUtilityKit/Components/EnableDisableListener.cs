using MelonLoader;
using System;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using VRChatUtilityKit.Utilities;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit.Components 
{
    /// <summary>
    /// A simple enable/disable listener component
    /// </summary>
    [RegisterTypeInIl2Cpp]
    public class EnableDisableListener : MonoBehaviour
    {
        /// <summary>
        /// Calls when the component is enabled.
        /// </summary>
        [method: HideFromIl2Cpp]
        public event Action OnEnableEvent;
        /// <summary>
        /// Calls when the component is disabled.
        /// </summary>
        [method: HideFromIl2Cpp]
        public event Action OnDisableEvent;
        public EnableDisableListener(IntPtr obj0) : base(obj0) { }

        internal void OnEnable() => OnEnableEvent?.DelegateSafeInvoke();
        internal void OnDisable() => OnDisableEvent?.DelegateSafeInvoke();
    }
}
