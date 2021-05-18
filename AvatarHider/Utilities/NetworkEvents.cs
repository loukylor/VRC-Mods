using System;
using System.Linq;
using System.Reflection;
using Harmony;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.Management;

namespace AvatarHider.Utilities
{
    // Literally copied from knah's JoinNotifier (https://github.com/knah/VRCMods/tree/master/JoinNotifier)
    class NetworkEvents
    {
        public static event Action<Player> OnPlayerLeft;
        public static event Action<Player> OnPlayerJoined;
        public static event Action<APIUser> OnFriended;
        public static event Action<string> OnUnfriended;
        public static event Action<VRCAvatarManager, GameObject> OnAvatarChanged;
        public static event Action<string, ApiPlayerModeration.ModerationType> OnPlayerModerationSent;
        public static event Action<string, ApiPlayerModeration.ModerationType> OnPlayerModerationRemoved;

        private static void OnFriend(APIUser __0)
        {
            OnFriended?.Invoke(__0);
        }
        private static void OnUnfriend(string __0)
        {
            OnUnfriended?.Invoke(__0);
        }
        private static void OnAvatarChange(VRCAvatarManager __instance)
        {
            OnAvatarChanged?.Invoke(__instance, __instance.prop_GameObject_0);
        }
        private static void OnPlayerModerationSend(string __0, ApiPlayerModeration.ModerationType __1)
        {
            OnPlayerModerationSent?.Invoke(__0, __1);
        }
        private static void OnPlayerModerationRemove(string __0, ApiPlayerModeration.ModerationType __1)
        {
            OnPlayerModerationRemoved?.Invoke(__0, __1);
        }

        public static void NetworkInit()
        {
            var field0 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var field1 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;

            field0.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => { if (player != null) OnPlayerJoined?.Invoke(player); }));
            field1.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => { if (player != null) OnPlayerLeft?.Invoke(player); }));

            AvatarHiderMod.Instance.Harmony.Patch(typeof(APIUser).GetMethod("LocalAddFriend"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnFriend), BindingFlags.NonPublic | BindingFlags.Static)));
            AvatarHiderMod.Instance.Harmony.Patch(typeof(APIUser).GetMethod("UnfriendUser"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnUnfriend), BindingFlags.NonPublic | BindingFlags.Static)));
            AvatarHiderMod.Instance.Harmony.Patch(typeof(VRCAvatarManager).GetMethod("Method_Private_Boolean_GameObject_String_Single_String_PDM_0"), null, new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnAvatarChange), BindingFlags.NonPublic | BindingFlags.Static)));
            AvatarHiderMod.Instance.Harmony.Patch(typeof(ModerationManager).GetMethod("Method_Private_Void_String_ModerationType_Action_1_ApiPlayerModeration_Action_1_String_0"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnPlayerModerationSend), BindingFlags.NonPublic | BindingFlags.Static)));
            AvatarHiderMod.Instance.Harmony.Patch(typeof(ModerationManager).GetMethod("Method_Private_Void_String_ModerationType_0"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnPlayerModerationRemove), BindingFlags.NonPublic | BindingFlags.Static)));
            
            foreach (Type type in typeof(ModerationManager).Assembly.GetTypes())
            {
                int counter = 0;
                foreach (PropertyInfo pi in type.GetProperties())
                    if (pi.Name.Contains("_Dictionary_2_String_APIUser_"))
                        counter += 1;
                if (counter == 3)
                {
                    AvatarHiderMod.Instance.Harmony.Patch(type.GetMethod("Method_Private_Void_String_0"), new HarmonyMethod(typeof(NetworkEvents).GetMethod(nameof(OnUnfriend), BindingFlags.NonPublic | BindingFlags.Static)));
                    break;
                }
            }
        }
    }
}
