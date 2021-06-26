using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UserInfoExtensions;
using VRC.Core;
using VRChatUtilityKit.Utilities;

namespace UserInfoExtentions.Modules
{
    class UserInformation : ModuleBase
    {
        public static MelonPreferences_Entry<bool> militaryTimeFormat;

        private static Text refreshButtonLabel;
        private static Text userNameLabel;
        private static Text platformLabel;
        private static Text lastLoginLabel;
        private static Text dateJoinedLabel;
        private static Text friendIndexLabel;

        public override void Init()
        {
            militaryTimeFormat = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(militaryTimeFormat), true, "Display time in 24 hour time");

            UserInfoExtensionsMod.menu.AddLabel("General User Info");
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSimpleButton("Pressing this button may be able to find unknown data", new Action(() => RefreshAPIUser()), new Action<GameObject>((gameObject) => refreshButtonLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => userNameLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => platformLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => lastLoginLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => dateJoinedLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => friendIndexLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
        }
        public override void OnUIXMenuOpen()
        {
            if (VRCUtils.ActiveUserInUserInfoMenu.username != null)
                userNameLabel.text = "Username:\n" + VRCUtils.ActiveUserInUserInfoMenu.username;
            else
                userNameLabel.text = "Username:\n" + VRCUtils.ActiveUserInUserInfoMenu.displayName.ToLower();

            switch (VRCUtils.ActiveUserInUserInfoMenu.last_platform)
            {
                case "standalonewindows":
                    platformLabel.text = "Last Platform:\nPC";
                    break;
                case "android":
                    platformLabel.text = "Last Platform:\nQuest";
                    break;
                default:
                    platformLabel.text = "Last Platform:\nUnknown";
                    break;
            }

            try
            { 
                DateTime lastLogin = DateTime.Parse(VRCUtils.ActiveUserInUserInfoMenu.last_login);
                if (militaryTimeFormat.Value)
                    lastLoginLabel.text = "Last Login:\n" + lastLogin.ToString("M/d/yyyy HH:mm");
                else
                    lastLoginLabel.text = "Last Login:\n" + lastLogin.ToString("M/d/yyyy hh:mm tt");
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FormatException)
                    lastLoginLabel.text = "Last Login:\nUnknown";
            }

            try
            {
                if (Utils.ActiveUIEUser == null)
                {
                    dateJoinedLabel.text = "Date Joined:\nUnknown";
                }
                else
                {
                    DateTime dateJoined = DateTime.Parse(Utils.ActiveUIEUser.DateJoined);
                    dateJoinedLabel.text = "Date Joined:\n" + dateJoined.ToString("d");
                }
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FormatException)
                    dateJoinedLabel.text = "Date Joined:\nUnknown";
            }

            if (APIUser.CurrentUser != null)
            {
                if (VRCUtils.ActiveUserInUserInfoMenu.IsSelf)
                {
                    friendIndexLabel.text = "Friend Number:\nIs Yourself";
                }
                else
                {
                    int friendIndex = APIUser.CurrentUser.friendIDs.IndexOf(VRCUtils.ActiveUserInUserInfoMenu.id);
                    if (friendIndex != -1)
                        friendIndexLabel.text = "Friend Number:\n" + (friendIndex + 1).ToString();
                    else
                        friendIndexLabel.text = "Friend Number:\nNot a Friend";
                }
            }
            else
            {
                friendIndexLabel.text = "Friend Number:\nUnknown";
            }
        }

        public void RefreshAPIUser()
        {
            if (!Utils.StartRequestTimer(new Action(() => { if (refreshButtonLabel != null) refreshButtonLabel.text = "Please wait between button presses"; }), 
                new Action(() => { if (refreshButtonLabel != null) refreshButtonLabel.text = "Pressing this button may be able to find unknown data"; })))
                return;

            if (Utils.ActiveUIEUser == null || !Utils.ActiveUIEUser.IsFullAPIUser) // Only bother to refresh if there's actually any invalid data // Still resets timer for consistent behavior on user's end
                APIUser.FetchUser(VRCUtils.ActiveUserInUserInfoMenu.id, new Action<APIUser>((user) => OnUIXMenuOpen()), null);
        }
    }
}
