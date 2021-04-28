using System;
using PlayerList.Utilities;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace PlayerList.Components
{
    class EnableDisableListener : MonoBehaviour
    {
        [method: HideFromIl2Cpp]
        public event Action OnEnableEvent;
        [method: HideFromIl2Cpp]
        public event Action OnDisableEvent;
        public EnableDisableListener(IntPtr obj0) : base(obj0) { }

        public void OnEnable() => OnEnableEvent?.SafeInvoke();
        public void OnDisable() => OnDisableEvent?.SafeInvoke();
    }
}
