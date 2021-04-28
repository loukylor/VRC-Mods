using System;
using System.Linq;
using System.Reflection;
using Harmony;
using UnityEngine;
using VRC;
using VRC.Core;

namespace PlayerList.Utilities
{
    // Literally copied from knah's JoinNotifier (https://github.com/knah/VRCMods/tree/master/JoinNotifier)
    class NetworkEvents
    {
        public static event Action<Player> OnPlayerLeft;
        public static event Action<Player> OnPlayerJoined;
        public static event Action<ApiWorld, ApiWorldInstance> OnInstanceChanged;
        public static event Action<Photon.Realtime.Player> OnMasterChanged;
        public static event Action<ApiAvatar, VRCAvatarManager> OnAvatarChanged;
        public static event Action<VRCPlayer, GameObject> OnAvatarInstantiated;

        private static void OnInstanceChange(ApiWorld __0, ApiWorldInstance __1)
        {
            if (__0 == null || __1 == null) return;
            
            OnInstanceChanged?.SafeInvoke(__0, __1);
        }
        private static void OnMasterChange(Photon.Realtime.Player __0)
        {
            if (__0 == null) return;

            OnMasterChanged?.SafeInvoke(__0);
        }
        private static void OnAvatarChange(ApiAvatar __0, VRCAvatarManager __instance)
        {
            if (__0 == null || __instance == null) return;

            OnAvatarChanged?.SafeInvoke(__0, __instance);
        }
        private static void OnAvatarInstantiate(VRCPlayer __instance, GameObject __0, bool __2)
        {
            if (__instance == null || __0 == null) return;

            if (__2)
                OnAvatarInstantiated?.SafeInvoke(__instance, __0);
        }

        public static void NetworkInit()
        {
            if (NetworkManager.field_Internal_Static_NetworkManager_0 == null) return;

            var field0 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var field1 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;
            PlayerListMod.Instance.Harmony.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnInstanceChange), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.Harmony.Patch(typeof(NetworkManager).GetMethod("OnMasterClientSwitched"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnMasterChange), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.Harmony.Patch(typeof(VRCAvatarManager).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Boolean_ApiAvatar_String_Single_")), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarChange), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.Harmony.Patch(typeof(VRCPlayer).GetMethods().First(mi => mi.Name.StartsWith("Method_Private_Void_GameObject_VRC_AvatarDescriptor_Boolean_") && !Xref.CheckMethod(mi, "Avatar is Ready, Initializing")), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarInstantiate), BindingFlags.NonPublic | BindingFlags.Static)));

            field0.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => { if (player != null) OnPlayerJoined?.SafeInvoke(player); }));
            field1.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => { if (player != null) OnPlayerLeft?.SafeInvoke(player); }));
        }
    }
}
