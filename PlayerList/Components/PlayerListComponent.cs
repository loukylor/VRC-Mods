using System;
using UnityEngine;

namespace PlayerList.Components
{
    class PlayerListComponent : MonoBehaviour
    {
        public PlayerListComponent(IntPtr obj0) : base(obj0) { }

        public void OnEnable() => PlayerListMod.RefreshLayout();
    }
}
