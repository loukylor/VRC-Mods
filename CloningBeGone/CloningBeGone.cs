using MelonLoader;
using UnityEngine;
using VRC.Core;

[assembly: MelonInfo(typeof(CloningBeGone.CloningBeGoneMod), "CloningBeGone", "1.1.0", "loukylor (https://github.com/loukylor/CloningBeGone)")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace CloningBeGone
{
    class CloningBeGoneMod : MelonMod
    {
        public static bool PublicCloningSetting;
        public static bool FriendsPlusCloningSetting;
        public static bool FriendsCloningSetting;
        public static bool InvitePlusCloningSetting;
        public static bool InviteCloningSetting;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("CloningBeGone", "CloningBeGone Settings");
            MelonPreferences.CreateEntry("CloningBeGone", nameof(PublicCloningSetting), false, "Cloning setting in public instances");
            MelonPreferences.CreateEntry("CloningBeGone", nameof(FriendsPlusCloningSetting), false, "Cloning setting in friends+ instances");
            MelonPreferences.CreateEntry("CloningBeGone", nameof(FriendsCloningSetting), false, "Cloning setting in friend only instances");
            MelonPreferences.CreateEntry("CloningBeGone", nameof(InvitePlusCloningSetting), false, "Cloning setting in invite+ instances");
            MelonPreferences.CreateEntry("CloningBeGone", nameof(InviteCloningSetting), false, "Cloning setting in invite only instances");

            Harmony.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), new Harmony.HarmonyMethod(typeof(CloningBeGoneMod).GetMethod("OnInstanceChange")));
        }
        public override void OnPreferencesSaved()
        {
            PublicCloningSetting = MelonPreferences.GetEntryValue<bool>("CloningBeGone", nameof(PublicCloningSetting));
            FriendsPlusCloningSetting = MelonPreferences.GetEntryValue<bool>("CloningBeGone", nameof(FriendsPlusCloningSetting));
            FriendsCloningSetting = MelonPreferences.GetEntryValue<bool>("CloningBeGone", nameof(FriendsCloningSetting));
            InvitePlusCloningSetting = MelonPreferences.GetEntryValue<bool>("CloningBeGone", nameof(InvitePlusCloningSetting));
            InviteCloningSetting = MelonPreferences.GetEntryValue<bool>("CloningBeGone", nameof(InviteCloningSetting));

            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 != null) CheckAccessType(RoomManager.field_Internal_Static_ApiWorldInstance_0.InstanceType);
        }
        public static void OnInstanceChange(ApiWorldInstance __1)
        {
            if (__1 != null) CheckAccessType(__1.InstanceType);
        }
        public static void CheckAccessType(ApiWorldInstance.AccessType accessType)
        {
            switch (accessType)
            {
                case ApiWorldInstance.AccessType.Public:
                    SetCloning(PublicCloningSetting);
                    break;
                case ApiWorldInstance.AccessType.FriendsOfGuests:
                    SetCloning(FriendsPlusCloningSetting);
                    break;
                case ApiWorldInstance.AccessType.FriendsOnly:
                    SetCloning(FriendsCloningSetting);
                    break;
                case ApiWorldInstance.AccessType.InvitePlus:
                    SetCloning(InvitePlusCloningSetting);
                    break;
                case ApiWorldInstance.AccessType.InviteOnly:
                    SetCloning(InviteCloningSetting);
                    break;
            }
        }
        public static void SetCloning(bool value) => GameObject.Find("UserInterface/MenuContent/Screens/Settings/OtherOptionsPanel/AllowAvatarCopyingToggle").GetComponent<UiSettingConfig>().SetEnable(value);
    }
}
