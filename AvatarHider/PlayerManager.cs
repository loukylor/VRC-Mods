using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AvatarHider.DataTypes;
using AvatarHider.Utilities;
using Harmony;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.Management;

namespace AvatarHider
{
    public static class PlayerManager
    {
        public static Dictionary<int, AvatarHiderPlayer> players = new Dictionary<int, AvatarHiderPlayer>();
        public static Dictionary<int, AvatarHiderPlayer> filteredPlayers = new Dictionary<int, AvatarHiderPlayer>();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr OnPlayerNetDecodeDelegate(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer);
        private static readonly List<OnPlayerNetDecodeDelegate> dontGarbageCollectDelegates = new List<OnPlayerNetDecodeDelegate>();

        public static void Init()
        {
            NetworkEvents.OnPlayerJoined += OnPlayerJoin;
            NetworkEvents.OnPlayerLeft += OnPlayerLeave;
            NetworkEvents.OnFriended += OnFriend;
            NetworkEvents.OnUnfriended += OnUnfriend;
            NetworkEvents.OnAvatarChanged += OnAvatarChanged;
            NetworkEvents.OnPlayerModerationSent += OnPlayerModerationSent;
            NetworkEvents.OnPlayerModerationRemoved += OnPlayerModerationRemoved;
            
            // Definitely not stolen code from our lord and savior knah (https://github.com/knah/VRCMods/blob/master/AdvancedSafety/AdvancedSafetyMod.cs) because im not a skid
            foreach (MethodInfo method in typeof(PlayerNet).GetMethods().Where(mi => mi.GetParameters().Length == 3 && mi.Name.StartsWith("Method_Public_Virtual_Final_New_Void_ValueTypePublicSealed")))
            {
                unsafe
                {
                    var originalMethodPointer = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method).GetValue(null);

                    OnPlayerNetDecodeDelegate originalDecodeDelegate = null;

                    OnPlayerNetDecodeDelegate replacement = (instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer) => OnPlayerNetPatch(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer, originalDecodeDelegate);

                    dontGarbageCollectDelegates.Add(replacement); // Add to list to prevent from being garbage collected

                    MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPointer), Marshal.GetFunctionPointerForDelegate(replacement));

                    originalDecodeDelegate = Marshal.GetDelegateForFunctionPointer<OnPlayerNetDecodeDelegate>(originalMethodPointer);
                }
            }
        }
        public static void OnSceneWasLoaded()
        {
            players.Clear();
            filteredPlayers.Clear();
        }

        private static void OnAvatarChanged(VRCAvatarManager manager, GameObject gameObject)
        {
            if (manager.field_Private_VRCPlayer_0 == null) return;
            
            int photonId = manager.field_Private_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0;

            if (!players.ContainsKey(photonId)) return;

            players[photonId].SetAvatar(gameObject);
            if (filteredPlayers.ContainsKey(photonId))
                RefreshManager.RefreshPlayer(players[photonId], Player.prop_Player_0.transform.position);
            else
                if (Config.IncludeHiddenAvatars.Value && players[photonId].isHidden)
                players[photonId].SetInActive();
        }
        private static IntPtr OnPlayerNetPatch(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer, OnPlayerNetDecodeDelegate originalDecodeDelegate)
        {
            if (instancePointer == IntPtr.Zero)
                return originalDecodeDelegate(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer);

            PlayerNet playerNet = new Il2CppSystem.Object(instancePointer).TryCast<PlayerNet>();
            if (playerNet == null)
                return originalDecodeDelegate(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer);

            IntPtr result = originalDecodeDelegate(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer);
            if (result == IntPtr.Zero)
                return result;

            try
            {
                players[playerNet.field_Private_PhotonView_0.field_Private_Int32_0].position = playerNet.transform.position;
            }
            catch { }

            return result;
        }
        private static void OnPlayerJoin(Player player)
        {
            if (player == null || player.prop_APIUser_0.id == APIUser.CurrentUser.id) return;

            int photonId = player.prop_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0;

            if (players.ContainsKey(photonId)) return;

            AvatarHiderPlayer playerProp = new AvatarHiderPlayer()
            {
                active = true,
                photonId = photonId,
                userId = player.prop_APIUser_0.id,
                position = player.transform.position,
                player = player,
                avatar = player.prop_VRCPlayer_0.prop_VRCAvatarManager_0.prop_GameObject_0,
                isFriend = APIUser.IsFriendsWith(player.prop_APIUser_0.id),
                isShown = VRCUtils.IsAvatarExplcitlyShown(player.prop_APIUser_0),
                isHidden = VRCUtils.IsAvatarExplcitlyHidden(player.prop_APIUser_0)
            };

            players.Add(playerProp.photonId, playerProp);
            HideOrShowAvatar(playerProp);
            RefreshFilteredList();
        }
        private static void OnPlayerLeave(Player player)
        {
            int photonId = player.prop_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0;

            players.Remove(photonId);
            filteredPlayers.Remove(photonId);
        }

        private static void OnFriend(APIUser apiUser)
        {
            foreach (AvatarHiderPlayer playerProp in players.Values)
            {
                if (playerProp.userId == apiUser.id)
                {
                    playerProp.isFriend = true;
                    HideOrShowAvatar(playerProp);
                    RefreshFilteredList();
                }
            }
        }
        private static void OnUnfriend(string userId)
        {
            foreach (AvatarHiderPlayer playerProp in players.Values)
            {
                if (playerProp.userId == userId)
                {
                    playerProp.isFriend = false;
                    RefreshFilteredList();
                }
            }
        }
        private static void OnPlayerModerationSent(string userId, ApiPlayerModeration.ModerationType moderationType)
        {
            if (moderationType == ApiPlayerModeration.ModerationType.ShowAvatar)
            {
                foreach (AvatarHiderPlayer playerProp in players.Values)
                {
                    if (playerProp.userId == userId)
                    {
                        playerProp.isShown = true;
                        HideOrShowAvatar(playerProp);
                        RefreshFilteredList();
                    }
                }
            }
            else if (moderationType == ApiPlayerModeration.ModerationType.HideAvatar)
            {
                foreach (AvatarHiderPlayer playerProp in players.Values)
                {
                    if (playerProp.userId == userId)
                    {
                        playerProp.isHidden = true;
                        HideOrShowAvatar(playerProp);
                        RefreshFilteredList();
                    }
                }
            }
        }
        private static void OnPlayerModerationRemoved(string userId, ApiPlayerModeration.ModerationType moderationType)
        {
            if (moderationType == ApiPlayerModeration.ModerationType.ShowAvatar)
            {
                foreach (AvatarHiderPlayer playerProp in players.Values)
                {
                    if (playerProp.userId == userId)
                    {
                        playerProp.isShown = false;
                        RefreshFilteredList();
                    }
                }
            }
            else if (moderationType == ApiPlayerModeration.ModerationType.HideAvatar)
            {
                foreach (AvatarHiderPlayer playerProp in players.Values)
                {
                    if (playerProp.userId == userId)
                    {
                        playerProp.isHidden = false;
                        RefreshFilteredList();
                    }
                }
            }
        }

        public static List<AvatarHiderPlayer> RefreshFilteredList()
        {
            ExcludeFlags excludeFlags = ExcludeFlags.None;
            if (Config.IgnoreFriends.Value)
                excludeFlags |= ExcludeFlags.Friends;
            if (Config.ExcludeShownAvatars.Value)
                excludeFlags |= ExcludeFlags.Shown;
            if (Config.IncludeHiddenAvatars.Value)
                excludeFlags |= ExcludeFlags.Hidden;

            List<AvatarHiderPlayer> removedPlayers = new List<AvatarHiderPlayer>();
            filteredPlayers = players.ToDictionary(entry => entry.Key, entry => entry.Value);
            foreach (KeyValuePair<int, AvatarHiderPlayer> item in players)
            {
                if (((excludeFlags.HasFlag(ExcludeFlags.Friends) && item.Value.isFriend) ||
                     (excludeFlags.HasFlag(ExcludeFlags.Shown) && item.Value.isShown)) &&
                     !(excludeFlags.HasFlag(ExcludeFlags.Hidden) && item.Value.isHidden))
                { 
                    filteredPlayers.Remove(item.Key);
                    removedPlayers.Add(item.Value);
                }
            }
            return removedPlayers;
        }

        public static void HideOrShowAvatar(AvatarHiderPlayer playerProp)
        {
            if (Config.IncludeHiddenAvatars.Value && playerProp.isHidden)
                playerProp.SetInActive();
            else if ((Config.IgnoreFriends.Value && playerProp.isFriend) ||
                (Config.ExcludeShownAvatars.Value && playerProp.isShown))
                playerProp.SetActive();
        }
    }
}
