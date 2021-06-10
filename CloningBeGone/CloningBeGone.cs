using HarmonyLib;
using MelonLoader;
using UnityEngine;
using VRC.Core;

[assembly: MelonInfo(typeof(CloningBeGone.CloningBeGoneMod), "CloningBeGone", "1.1.3", "loukylor (https://github.com/loukylor/CloningBeGone)")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace CloningBeGone
{
    class CloningBeGoneMod : MelonMod
    {
        public static MelonPreferences_Entry<bool> PublicCloningSetting;
        public static MelonPreferences_Entry<bool> FriendsPlusCloningSetting;
        public static MelonPreferences_Entry<bool> FriendsCloningSetting;
        public static MelonPreferences_Entry<bool> InvitePlusCloningSetting;
        public static MelonPreferences_Entry<bool> InviteCloningSetting;

        public static ApiWorldInstance.AccessType currentAccessType;
        public static UiSettingConfig cloningSetting;
        public static bool isClosing = false;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("CloningBeGone", "CloningBeGone Settings");
            PublicCloningSetting = MelonPreferences.CreateEntry("CloningBeGone", nameof(PublicCloningSetting), false, "Cloning setting in public instances");
            FriendsPlusCloningSetting = MelonPreferences.CreateEntry("CloningBeGone", nameof(FriendsPlusCloningSetting), false, "Cloning setting in friends+ instances");
            FriendsCloningSetting = MelonPreferences.CreateEntry("CloningBeGone", nameof(FriendsCloningSetting), false, "Cloning setting in friend only instances");
            InvitePlusCloningSetting = MelonPreferences.CreateEntry("CloningBeGone", nameof(InvitePlusCloningSetting), false, "Cloning setting in invite+ instances");
            InviteCloningSetting = MelonPreferences.CreateEntry("CloningBeGone", nameof(InviteCloningSetting), false, "Cloning setting in invite only instances");

            HarmonyInstance.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), new HarmonyMethod(typeof(CloningBeGoneMod).GetMethod(nameof(OnInstanceChange))));
            HarmonyInstance.Patch(typeof(NetworkManager).GetMethod("OnJoinedRoom"), new HarmonyMethod(typeof(CloningBeGoneMod).GetMethod(nameof(OnJoinedRoom))));
        }
        public override void OnPreferencesSaved()
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 != null) CheckAccessType(RoomManager.field_Internal_Static_ApiWorldInstance_0.InstanceType);
        }

        public static void OnJoinedRoom()
        {
            CheckAccessType(currentAccessType);
        }
        public static void OnInstanceChange(ApiWorldInstance __1)
        {
            if (__1 != null) currentAccessType = __1.InstanceType;
        }
        public override void OnApplicationQuit()
        {
            isClosing = true;
        }

        public static void CheckAccessType(ApiWorldInstance.AccessType accessType)
        {
            switch (accessType)
            {
                case ApiWorldInstance.AccessType.Public:
                    SetCloning(PublicCloningSetting.Value);
                    break;
                case ApiWorldInstance.AccessType.FriendsOfGuests:
                    SetCloning(FriendsPlusCloningSetting.Value);
                    break;
                case ApiWorldInstance.AccessType.FriendsOnly:
                    SetCloning(FriendsCloningSetting.Value);
                    break;
                case ApiWorldInstance.AccessType.InvitePlus:
                    SetCloning(InvitePlusCloningSetting.Value);
                    break;
                case ApiWorldInstance.AccessType.InviteOnly:
                    SetCloning(InviteCloningSetting.Value);
                    break;
            }
        }
        public static void SetCloning(bool value)
        {
            if (cloningSetting == null)
                cloningSetting = GameObject.Find("UserInterface/MenuContent/Screens/Settings/OtherOptionsPanel/AllowAvatarCopyingToggle").GetComponent<UiSettingConfig>();

            if (Photon.Pun.PhotonNetwork.field_Public_Static_LoadBalancingClient_0.prop_Room_0 != null && !isClosing)
                cloningSetting.SetEnable(value);
        }
    }
}
