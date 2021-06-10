using System;
using MelonLoader;
using UnityEngine;
using UserInfoExtensions;
using UserInfoExtentions.Utilities;

namespace UserInfoExtentions.Modules
{
    public class OpenInBrowser : ModuleBase
    {
        public static MelonPreferences_Entry<bool> OpenUserInBrowserButton;
        public static GameObject openUserInBrowserButtonGameObject;

        public override void Init()
        {
            OpenUserInBrowserButton = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(OpenUserInBrowserButton), false, "Show \"Open User in Browser\" button");

            UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Open User in Browser", OpenUserInBrowser, new Action<GameObject>((gameObject) => { openUserInBrowserButtonGameObject = gameObject; gameObject.SetActive(OpenUserInBrowserButton.Value); }));

            UserInfoExtensionsMod.menu.AddLabel("Website Related Things");
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSimpleButton("Open User in Browser", OpenUserInBrowser);
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSpacer();
        }
        public override void OnPreferencesSaved()
        {
            openUserInBrowserButtonGameObject?.SetActive(OpenUserInBrowserButton.Value);
        }

        public static void OpenUserInBrowser()
        {
            UserInfoExtensionsMod.HideAllPopups();

            System.Diagnostics.Process.Start("https://vrchat.com/home/user/" + VRCUtils.ActiveUser.id);
            VRCUtils.OpenPopupV2("Notice:", "User has been opened in the default browser", "Close", new Action(VRCUtils.ClosePopup));
        }
    }
}
