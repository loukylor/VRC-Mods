using System;
using MelonLoader;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;

[assembly: MelonInfo(typeof(VRChatUtilityKit.VRChatUtilityKitMod), "VRChatUtilityKit", "1.0.3", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonPriority(-100)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit
{
    public class VRChatUtilityKitMod : MelonMod
    {
        internal static VRChatUtilityKitMod Instance { get; private set; }

        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Initializing...");
            Instance = this;
            UiManager.Init();
            VRCUtils.Init();
            VRCUtils.OnUiManagerInit += OnUiManagerInit;
        }

        internal void OnUiManagerInit()
        {
            MelonLogger.Msg("Initializing UI...");
            UiManager.UiInit();
            NetworkEvents.NetworkInit();
            VRCUtils.UiInit();
        }

        public override void OnUpdate()
        {
            if (AsyncUtils._toMainThreadQueue.TryDequeue(out Action result))
                result.Invoke();
        }
    }
}
