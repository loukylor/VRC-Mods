using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UserInfoExtensions;
using UserInfoExtentions.Utilities;
using VRC.Core;

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
        public override void Init()
        {
            militaryTimeFormat = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(militaryTimeFormat), true, "Display time in 24 hour time");

            UserInfoExtensionsMod.menu.AddLabel("General User Info");
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSimpleButton("Pressing this button may be able to find unknown data", new Action(() => RefreshAPIUser()), new Action<GameObject>((gameObject) => refreshButtonLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => userNameLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => platformLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => lastLoginLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => dateJoinedLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
        }
        public override void OnUIXMenuOpen()
        {
            userNameLabel.text = "Username:\n" + VRCUtils.ActiveUser.username;
            
            switch (VRCUtils.ActiveUser.last_platform)
            {
                case "standalonewindows":
                    platformLabel.text = "Platform:\nPC";
                    break;
                case "android":
                    platformLabel.text = "Platform:\nQuest";
                    break;
                default:
                    platformLabel.text = "Platform:\nUnknown";
                    break;
            }

            try
            { 
                DateTime lastLogin = DateTime.Parse(VRCUtils.ActiveUser.last_login);
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
                DateTime dateJoined = DateTime.Parse(VRCUtils.ActiveUIEUser.DateJoined);
                dateJoinedLabel.text = "Date Joined:\n" + dateJoined.ToString("d");
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FormatException)
                    dateJoinedLabel.text = "Date Joined:\nUnknown";
            }
        }

        public void RefreshAPIUser()
        {
            if (!VRCUtils.StartRequestTimer(new Action(() => { if (refreshButtonLabel != null) refreshButtonLabel.text = "Please wait between button presses"; }), 
                new Action(() => { if (refreshButtonLabel != null) refreshButtonLabel.text = "Pressing this button may be able to find unknown data"; })))
                return;

            if (!VRCUtils.ActiveUIEUser.IsFullAPIUser) // Only bother to refresh if there's actually any invalid data // Still resets timer for consistent behavior on user's end
                APIUser.FetchUser(VRCUtils.ActiveUser.id, new Action<APIUser>((user) => OnUIXMenuOpen()), null);
        }
    }
}
