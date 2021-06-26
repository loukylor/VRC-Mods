using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UserInfoExtensions;
using VRC;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;

namespace UserInfoExtentions.Modules
{
    public class QuickMenuFromSocial : ModuleBase
    {
        public static MelonPreferences_Entry<bool> QuickMenuFromSocialButton;

        public static GameObject quickMenuFromSocialButtonGameObject;

        public override void Init()
        {
            QuickMenuFromSocialButton = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(QuickMenuFromSocialButton), false, "Show \"To Quick Menu\" button");
            UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("To Quick Menu", ToQuickMenu, new Action<GameObject>((gameObject) => { quickMenuFromSocialButtonGameObject = gameObject; gameObject.SetActive(QuickMenuFromSocialButton.Value); }));
            UserInfoExtensionsMod.menu.AddSimpleButton("To Quick Menu", ToQuickMenu);
        }
        public override void OnPreferencesSaved()
        {
            quickMenuFromSocialButtonGameObject?.SetActive(QuickMenuFromSocialButton.Value);
        }
        public static void ToQuickMenu()
        {
            UserInfoExtensionsMod.HideAllPopups();

            foreach (Player player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player.prop_APIUser_0 == null) continue;
                if (player.prop_APIUser_0.id == VRCUtils.ActiveUserInUserInfoMenu.id)
                {
                    UiManager.CloseBigMenu();
                    UiManager.OpenQuickMenu();

                    UiManager.OpenUserInQuickMenu(player);

                    return;
                }
            }
            UiManager.OpenSmallPopup("Notice:", "You cannot show this user on the Quick Menu because they are not in the same instance", "Close", new Action(UiManager.ClosePopup));
        }
    }
}
