using System;
using System.Linq;
using System.Reflection;
using Harmony;
using PlayerList.Utilities;
using UnityEngine;
using VRC;
using VRC.Core;

namespace PlayerList
{
    // Literally copied from knah's JoinNotifier (https://github.com/knah/VRCMods/tree/master/JoinNotifier)
    class NetworkEvents
    {
        public static event Action<Player> OnPlayerLeave;
        public static event Action<Player> OnPlayerJoin;
        public static event Action<ApiWorld, ApiWorldInstance> OnInstanceChange;
        public static event Action<Photon.Realtime.Player> OnMasterChange;
        public static event Action<ApiAvatar, VRCAvatarManager> OnAvatarChanged;
        public static event Action<VRCPlayer, GameObject> OnAvatarInstantiated;

        private static void OnInstanceChangeMethod(ApiWorld __0, ApiWorldInstance __1)
        {
            OnInstanceChange?.Invoke(__0, __1);
        }
        private static void OnMasterChangeMethod(Photon.Realtime.Player __0)
        {
            OnMasterChange?.Invoke(__0);
        }
        private static void OnAvatarChangeMethod(ApiAvatar __0, VRCAvatarManager __instance)
        {
            OnAvatarChanged?.Invoke(__0, __instance);
        }
        private static void OnAvatarInstantiatedMethod(VRCPlayer __instance, GameObject __0, bool __2)
        {
            if (__2)
                OnAvatarInstantiated?.Invoke(__instance, __0);
        }

        public static void NetworkInit()
        {
            if (NetworkManager.field_Internal_Static_NetworkManager_0 == null) return;

            var field0 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var field1 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;
            PlayerListMod.Instance.Harmony.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnInstanceChangeMethod), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.Harmony.Patch(typeof(NetworkManager).GetMethod("OnMasterClientSwitched"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnMasterChangeMethod), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.Harmony.Patch(typeof(VRCAvatarManager).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Boolean_ApiAvatar_String_Single_")), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarChangeMethod), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.Harmony.Patch(typeof(VRCPlayer).GetMethods().First(mi => mi.Name.StartsWith("Method_Private_Void_GameObject_VRC_AvatarDescriptor_Boolean_") && !Xref.CheckMethod(mi, "Avatar is Ready, Initializing")), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarInstantiatedMethod), BindingFlags.NonPublic | BindingFlags.Static)));

            field0.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => OnPlayerJoin?.Invoke(player)));
            field1.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => OnPlayerLeave?.Invoke(player)));
        }
    }
}
