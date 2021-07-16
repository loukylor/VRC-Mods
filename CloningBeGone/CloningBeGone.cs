using System.Collections.Generic;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using VRC.Core;
using VRChatUtilityKit.Utilities;

[assembly: MelonInfo(typeof(CloningBeGone.CloningBeGoneMod), "CloningBeGone", "1.2.1", "loukylor (https://github.com/loukylor/CloningBeGone)")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace CloningBeGone
{
    class CloningBeGoneMod : MelonMod
    {
        public static MelonPreferences_Category category;
        public static MelonPreferences_Entry<bool> PublicCloningSetting;
        public static MelonPreferences_Entry<bool> FriendsPlusCloningSetting;
        public static MelonPreferences_Entry<bool> FriendsCloningSetting;
        public static MelonPreferences_Entry<bool> InvitePlusCloningSetting;
        public static MelonPreferences_Entry<bool> InviteCloningSetting;

        public static MelonPreferences_Entry<List<string>> cloningOnAvatars;
        public static MelonPreferences_Entry<List<string>> cloningOffAvatars;

        public static InstanceAccessType currentAccessType;
        public static UiSettingConfig cloningSetting;

        public override void OnApplicationStart()
        {
            category = MelonPreferences.CreateCategory("CloningBeGone", "CloningBeGone Settings");
            PublicCloningSetting = category.CreateEntry(nameof(PublicCloningSetting), false, "Cloning setting in public instances");
            FriendsPlusCloningSetting = category.CreateEntry(nameof(FriendsPlusCloningSetting), false, "Cloning setting in friends+ instances");
            FriendsCloningSetting = category.CreateEntry(nameof(FriendsCloningSetting), false, "Cloning setting in friend only instances");
            InvitePlusCloningSetting = category.CreateEntry(nameof(InvitePlusCloningSetting), false, "Cloning setting in invite+ instances");
            InviteCloningSetting = category.CreateEntry(nameof(InviteCloningSetting), false, "Cloning setting in invite only instances");

            foreach (MelonPreferences_Entry entry in category.Entries)
                entry.OnValueChangedUntyped += OnPrefValueChanged;

            cloningOnAvatars = category.CreateEntry(nameof(cloningOnAvatars), new List<string>(), null, null, true);
            cloningOffAvatars = category.CreateEntry(nameof(cloningOffAvatars), new List<string>(), null, null, true);

            HarmonyInstance.Patch(typeof(NetworkManager).GetMethod("OnJoinedRoom"), new HarmonyMethod(typeof(CloningBeGoneMod).GetMethod(nameof(OnJoinedRoom))));

            NetworkEvents.OnInstanceChanged += OnInstanceChange;
            NetworkEvents.OnAvatarInstantiated += OnAvatarInstantiated;

            if (VRCUtils.IsUIXPresent)
                typeof(UIXManager).GetMethod("Init").Invoke(null, null);
        }
        public static void OnPrefValueChanged()
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 != null) CheckAccessType(RoomManager.field_Internal_Static_ApiWorldInstance_0.type);
        }

        public static void OnJoinedRoom()
        {
            CheckAccessType(currentAccessType);
        }
        public static void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            if (instance != null) currentAccessType = instance.type;
        }

        internal static void OnAvatarInstantiated(VRCAvatarManager manager, ApiAvatar avatar, GameObject gameObject)
        {
            if (!manager.field_Private_VRCPlayer_0.prop_Player_0.prop_APIUser_0.IsSelf)
                return;

            OnAvatarInstantiated(avatar);
        }
        internal static void OnAvatarInstantiated(ApiAvatar avatar)
        {
            if (cloningOnAvatars.Value.Contains(avatar.id))
                SetCloning(true);
            else if (cloningOffAvatars.Value.Contains(avatar.id))
                SetCloning(false);
        }

        public static void CheckAccessType(InstanceAccessType accessType)
        {
            switch (accessType)
            {
                case InstanceAccessType.Public:
                    SetCloning(PublicCloningSetting.Value);
                    break;
                case InstanceAccessType.FriendsOfGuests:
                    SetCloning(FriendsPlusCloningSetting.Value);
                    break;
                case InstanceAccessType.FriendsOnly:
                    SetCloning(FriendsCloningSetting.Value);
                    break;
                case InstanceAccessType.InvitePlus:
                    SetCloning(InvitePlusCloningSetting.Value);
                    break;
                case InstanceAccessType.InviteOnly:
                    SetCloning(InviteCloningSetting.Value);
                    break;
            }
        }
        public static void SetCloning(bool value)
        {
            if (cloningSetting == null)
                cloningSetting = GameObject.Find("UserInterface/MenuContent/Screens/Settings/OtherOptionsPanel/AllowAvatarCopyingToggle").GetComponent<UiSettingConfig>();

            if (Photon.Pun.PhotonNetwork.field_Public_Static_LoadBalancingClient_0.prop_Room_0 != null)
                cloningSetting.SetEnable(value);
        }
    }
}
