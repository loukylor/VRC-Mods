using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.UI;

namespace PlayerList.Utilities
{
    // Literally copied from knah's JoinNotifier (https://github.com/knah/VRCMods/tree/master/JoinNotifier)
    class NetworkEvents
    {
        public static event Action<APIUser> OnFriended;
        public static event Action<string> OnUnFriended;
        public static event Action<Player> OnPlayerLeft;
        public static event Action<Player> OnPlayerJoined;
        public static event Action<ApiWorld, ApiWorldInstance> OnInstanceChanged;
        public static event Action<Photon.Realtime.Player> OnMasterChanged;
        public static event Action<VRCAvatarManager, GameObject> OnAvatarInstantiated;
        public static event Action<AvatarLoadingBar, float, long> OnAvatarDownloadProgressed;
        public static event Action<VRCPlayer, int> OnSetupFlagsReceived;

        private static void OnFriend(APIUser __0)
        {
            if (__0 == null) return;

            OnFriended?.SafeInvoke(__0);
        }
        private static void OnUnfriend(string __0)
        {
            if (string.IsNullOrWhiteSpace(__0)) return;

            OnUnFriended?.SafeInvoke(__0);
        }
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
        private static void OnAvatarInstantiate(VRCAvatarManager __instance, GameObject __0, ref Il2CppSystem.Delegate __2)
        {
            if (__2 == null) return;

            __2 = __2.CombineImpl((Il2CppSystem.Action<bool>)new Action<bool>((flag) =>
            {
                if (__instance == null || __0 == null || !flag)
                    return;

                OnAvatarInstantiated?.SafeInvoke(__instance, __0);
            }));
        }
        private static void OnAvatarDownloadProgress(AvatarLoadingBar __instance, float __0, long __1)
        {
            if (__instance == null) return;

            OnAvatarDownloadProgressed?.SafeInvoke(__instance, __0, __1);
        }
        private static void OnSetupFlagsReceive(VRCPlayer __instance, object __result)
        {
            if (__instance == null) return;
            
            OnSetupFlagsReceived?.SafeInvoke(__instance, (int)__result);
        }

        public static void NetworkInit()
        {
            if (NetworkManager.field_Internal_Static_NetworkManager_0 == null) return;

            PlayerListMod.Instance.HarmonyInstance.Patch(typeof(VRCAvatarManager).GetMethods().First(mb => mb.Name.StartsWith("Method_Private_Void_ApiAvatar_GameObject_Action_1_Boolean_")), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarInstantiate), BindingFlags.NonPublic | BindingFlags.Static)));

            PlayerListMod.Instance.HarmonyInstance.Patch(typeof(APIUser).GetMethod("LocalAddFriend"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnFriend), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.HarmonyInstance.Patch(typeof(APIUser).GetMethod("UnfriendUser"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnUnfriend), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.HarmonyInstance.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnInstanceChange), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.HarmonyInstance.Patch(typeof(NetworkManager).GetMethod("OnMasterClientSwitched"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnMasterChange), BindingFlags.NonPublic | BindingFlags.Static)));

            foreach (MethodInfo method in typeof(AvatarLoadingBar).GetMethods().Where(mb => mb.Name.Contains("Method_Public_Void_Single_Int64_PDM_")))
                PlayerListMod.Instance.HarmonyInstance.Patch(method, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarDownloadProgress), BindingFlags.NonPublic | BindingFlags.Static)));

            PlayerListMod.Instance.HarmonyInstance.Patch(typeof(FriendsListManager).GetMethod("Method_Private_Void_String_0"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnUnfriend), BindingFlags.NonPublic | BindingFlags.Static)));

            MethodInfo onSetupFlagsReceivedMethod = typeof(VRCPlayer).GetMethods().First(mi => mi.ReturnType.IsEnum && mi.GetParameters().Length == 1 && mi.GetParameters()[0].ParameterType == typeof(Il2CppSystem.Collections.Hashtable) && Xref.CheckMethod(mi, "Failed to read showSocialRank for {0}"));
            PlayerListMod.Instance.HarmonyInstance.Patch(onSetupFlagsReceivedMethod, null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnSetupFlagsReceive), BindingFlags.NonPublic | BindingFlags.Static)));

            var field0 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var field1 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;

            field0.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => { if (player != null) OnPlayerJoined?.SafeInvoke(player); }));
            field1.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => { if (player != null) OnPlayerLeft?.SafeInvoke(player); }));
        }
    }
}
