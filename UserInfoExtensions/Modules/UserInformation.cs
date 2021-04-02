using System;
using UnityEngine;
using UnityEngine.UI;
using UserInfoExtensions;
using UserInfoExtentions.Utilities;

namespace UserInfoExtentions.Modules
{
    class UserInformation : ModuleBase
    {
        private static Text userNameLabel;
        private static Text platformLabel;
        private static Text lastLoginLabel;
        public override void Init()
        {
            UserInfoExtensionsMod.menu.AddLabel("General User Info");
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => userNameLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => platformLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            UserInfoExtensionsMod.menu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => lastLoginLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
        }
        public override void OnUIEMenuOpen()
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
                lastLoginLabel.text = "Last Login:\n" + lastLogin.ToString("G");

            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is FormatException)
                    lastLoginLabel.text = "Last Login:\nUnknown";
            }
        }
    }
}
