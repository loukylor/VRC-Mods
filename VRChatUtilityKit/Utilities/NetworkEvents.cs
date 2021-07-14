using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.Management;
using VRC.UI;


using MelonLoader;
namespace VRChatUtilityKit.Utilities
{
    // Join/leave events stolen from knah's JoinNotifier (https://github.com/knah/VRCMods/tree/master/JoinNotifier)
    /// <summary>
    /// A set of events pertaining to VRChat's network.
    /// </summary>
    public static class NetworkEvents
    {
        /// <summary>
        /// Calls when a player has left the instance.
        /// This will also call as every player is removed after leaving an instance.
        /// </summary>
        public static event Action<Player> OnPlayerLeft;

        /// <summary>
        /// Calls when a player joins the instance.
        /// The first call of this may contain a null APIUser.
        /// </summary>
        public static event Action<Player> OnPlayerJoined;

        /// <summary>
        /// Calls when the local user leaves an room.
        /// </summary>
        public static event Action OnRoomLeft;

        /// <summary>
        /// Calls when the local user joins an room.
        /// </summary>
        public static event Action OnRoomJoined;

        /// <summary>
        /// Calls when a new friend is added.
        /// Whether they accepted your friend request or you accepted theirs does not matter.
        /// </summary>
        public static event Action<APIUser> OnFriended;

        /// <summary>
        /// Calls when a friend is removed.
        /// Whether you removed them or they removed you does not matter.
        /// </summary>
        public static event Action<string> OnUnfriended;

        /// <summary>
        /// Calls during the loading screen into a new instance.
        /// Does not call if there was a desync while joining the instance.
        /// </summary>
        public static event Action<ApiWorld, ApiWorldInstance> OnInstanceChanged;

        /// <summary>
        /// Calls when the master of the instance changes.
        /// Does not call upon initialy receiving the master after joining the instance.
        /// </summary>
        public static event Action<Photon.Realtime.Player> OnMasterChanged;

        /*
        /// <summary>
        /// Called when a player switches avatars.
        /// </summary>
        public static event Action<VRCPlayer, GameObject> OnCustomAvatarChanged;
        */

        /// <summary>
        /// Calls when the internal value of the avatar in an instance of VRCAvatarManager is changed.
        /// It will call for every GameObject change. This is not limited to, but includes the error bot and platform substitute.
        /// </summary>
        public static event Action<VRCAvatarManager, GameObject> OnAvatarChanged;

        /// <summary>
        /// Calls when an avatar is instantiated
        /// </summary>
        public static event Action<VRCAvatarManager, ApiAvatar, GameObject> OnAvatarInstantiated;

        /// <summary>
        /// Calls when an avatar download is progressing. 
        /// The float is percentage and long is total size.
        /// </summary>
        public static event Action<AvatarLoadingBar, float, long> OnAvatarDownloadProgressed;

        /// <summary>
        /// Calls when the setup flags of a VRCPlayer is received from photon.
        /// The int is actually the enum "VRCPlayer.EnumNPrivateSealedvaViMoVRStShAvUsFa9vUnique" at the time of writing.
        /// </summary>
        public static event Action<VRCPlayer, int> OnSetupFlagsReceived;

        /// <summary>
        /// Calls when the local user changes whether to show their social rank.
        /// </summary>
        public static event Action OnShowSocialRankChanged;

        /// <summary>
        /// Calls when a player moderation is sent from the local user.
        /// </summary>
        public static event Action<string, ApiPlayerModeration.ModerationType> OnPlayerModerationSent;

        /// <summary>
        /// Calls when a player moderation is removed by the local user.
        /// </summary>
        public static event Action<string, ApiPlayerModeration.ModerationType> OnPlayerModerationRemoved;

        private static void OnRoomLeave() => OnRoomLeft?.DelegateSafeInvoke();
        
        private static void OnRoomJoin() => OnRoomJoined?.DelegateSafeInvoke();

        private static void OnFriend(APIUser __0)
        {
            if (__0 == null) return;

            OnFriended?.DelegateSafeInvoke(__0);
        }
        private static void OnUnfriend(string __0)
        {
            if (string.IsNullOrEmpty(__0)) return;

            OnUnfriended?.DelegateSafeInvoke(__0);
        }
        private static void OnInstanceChange(ApiWorld __0, ApiWorldInstance __1)
        {
            if (__0 == null || __1 == null) return;

            OnInstanceChanged?.DelegateSafeInvoke(__0, __1);
        }
        private static void OnMasterChange(Photon.Realtime.Player __0)
        {
            if (__0 == null) return;

            OnMasterChanged?.DelegateSafeInvoke(__0);
        }
        private static void OnAvatarChange(VRCAvatarManager __instance)
        {
            OnAvatarChanged?.DelegateSafeInvoke(__instance, __instance.prop_GameObject_0);
        }
        private static void OnAvatarInstantiate(VRCAvatarManager __instance, ApiAvatar __0, GameObject __1)
        {
            if (__0 == null || __1 == null)
                return;

            OnAvatarInstantiated.DelegateSafeInvoke(__instance, __0, __1);
        }
        private static void OnAvatarDownloadProgress(AvatarLoadingBar __instance, float __0, long __1)
        {
            OnAvatarDownloadProgressed?.DelegateSafeInvoke(__instance, __0, __1);
        }
        private static void OnSetupFlagsReceive(VRCPlayer __instance, object __result)
        {
            if (__result == null)
                return;

            OnSetupFlagsReceived?.DelegateSafeInvoke(__instance, __result);
        }
        private static void OnShowSocialRankChange()
        {
            OnShowSocialRankChanged?.DelegateSafeInvoke();
        }
        private static void OnPlayerModerationSend(string __1, ApiPlayerModeration.ModerationType __2)
        {
            if (__1 == null) return;

            OnPlayerModerationSent?.DelegateSafeInvoke(__1, __2);
        }
        private static void OnPlayerModerationRemove(string __0, ApiPlayerModeration.ModerationType __1)
        {
            if (__0 == null) return;

            OnPlayerModerationRemoved?.DelegateSafeInvoke(__0, __1);
        }

        internal static void NetworkInit()
        {
            var field0 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var field1 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;

            field0.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => { if (player != null) OnPlayerJoined?.DelegateSafeInvoke(player); }));
            field1.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => { if (player != null) OnPlayerLeft?.DelegateSafeInvoke(player); }));

            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(APIUser).GetMethod("LocalAddFriend"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnFriend), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(APIUser).GetMethod("UnfriendUser"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnUnfriend), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnInstanceChange), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(NetworkManager).GetMethod("OnMasterClientSwitched"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnMasterChange), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(NetworkManager).GetMethod("OnLeftRoom"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnRoomLeave), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(NetworkManager).GetMethod("OnJoinedRoom"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnRoomJoin), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(VRCAvatarManager).GetMethods().First(mb => mb.Name.StartsWith("Method_Private_Boolean_GameObject_String_Single_String_")), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarChange), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(VRCAvatarManager).GetMethods().First(mb => mb.Name.StartsWith("Method_Private_Boolean_ApiAvatar_GameObject_")), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarInstantiate), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(ModerationManager).GetMethods().First(mb => mb.Name.StartsWith("Method_Private_ApiPlayerModeration_String_String_ModerationType_")), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnPlayerModerationSend), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(ModerationManager).GetMethod("Method_Private_Void_String_ModerationType_0"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnPlayerModerationRemove), BindingFlags.NonPublic | BindingFlags.Static)));

            foreach (MethodInfo method in typeof(AvatarLoadingBar).GetMethods().Where(mb => mb.Name.Contains("Method_Public_Void_Single_Int64_")))
                VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(method, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarDownloadProgress), BindingFlags.NonPublic | BindingFlags.Static)));

            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(FriendsListManager).GetMethod("Method_Private_Void_String_0"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnUnfriend), BindingFlags.NonPublic | BindingFlags.Static)));

            MethodInfo onSetupFlagsReceivedMethod = typeof(VRCPlayer).GetMethods().First(mi => mi.ReturnType.IsEnum && mi.GetParameters().Length == 1 && mi.GetParameters()[0].ParameterType == typeof(Il2CppSystem.Collections.Hashtable) && XrefUtils.CheckMethod(mi, "Failed to read showSocialRank for {0}"));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(onSetupFlagsReceivedMethod, null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnSetupFlagsReceive), BindingFlags.NonPublic | BindingFlags.Static)));

            MethodInfo OnShowSocialRankChangeMethod = typeof(QuickMenu).GetMethods()
                .First(mi => mi.Name.StartsWith("Method_Public_Void_") && mi.GetParameters().Length == 0 && XrefUtils.CheckUsing(mi, "Method_Public_Static_Void_EnumNPublicSealed", typeof(VRCInputManager))); // && (!mi.Name.EndsWith("1") && !mi.Name.EndsWith("8") && !mi.Name.EndsWith("12") && !mi.Name.EndsWith("17") && !mi.Name.EndsWith("6"))))
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(OnShowSocialRankChangeMethod, null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnShowSocialRankChange), BindingFlags.NonPublic | BindingFlags.Static)));
        }
    }
}
