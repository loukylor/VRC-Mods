using System;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UserInfoExtensions;
using UserInfoExtentions.Utilities;
using VRC;

namespace UserInfoExtentions.Modules
{
    public class QuickMenuFromSocial
    {
        public static MelonPreferences_Entry<bool> QuickMenuFromSocialButton;

        public static GameObject quickMenuFromSocialButtonGameObject;

        public static MethodBase closeMenu;
        public static MethodBase openQuickMenu;

        public static void Init()
        {
            QuickMenuFromSocialButton = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(QuickMenuFromSocialButton), false, "Show \"To Quick Menu\" button");
            UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("To Quick Menu", ToQuickMenu, new Action<GameObject>((gameObject) => { quickMenuFromSocialButtonGameObject = gameObject; gameObject.SetActive(QuickMenuFromSocialButton.Value); }));
            UserInfoExtensionsMod.menu.AddSimpleButton("To Quick Menu", ToQuickMenu);

            closeMenu = typeof(VRCUiManager).GetMethods()
                            .Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && !mb.Name.Contains("_PDM_")).OrderBy(mb => ((UnhollowerBaseLib.Attributes.CallerCountAttribute)Attribute.GetCustomAttribute(mb, typeof(UnhollowerBaseLib.Attributes.CallerCountAttribute))).Count).Last();
            openQuickMenu = typeof(QuickMenu).GetMethods()
                            .Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && mb.Name.Length <= 29 && mb.GetParameters().Any(pi => pi.HasDefaultValue == false)).First();
        }
        public static void OnPreferencesSaved()
        {
            quickMenuFromSocialButtonGameObject?.SetActive(QuickMenuFromSocialButton.Value);
        }
        public static void ToQuickMenu()
        {
            UserInfoExtensionsMod.HideAllPopups();

            foreach (Player player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player.field_Private_APIUser_0 == null) continue;
                if (player.field_Private_APIUser_0.id == VRCUtils.ActiveUser.id)
                {
                    closeMenu.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[] { true, false }); //Closes Big Menu
                    openQuickMenu.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { true }); //Opens Quick Menu

                    if (VRCUiCursorManager.Method_Public_Static_VRCUiCursor_0().gameObject.activeSelf) VRCUiCursorManager.Method_Public_Static_VRCUiCursor_0().Method_Public_Void_VRCPlayer_PDM_0(player.field_Internal_VRCPlayer_0);

                    QuickMenu.prop_QuickMenu_0.Method_Public_Void_Player_0(player); //Does the rest lmao

                    return;
                }
            }
            VRCUtils.OpenPopupV2("Notice:", "You cannot show this user on the Quick Menu because they are not in the same instance", "Close", new Action(VRCUtils.ClosePopup));
        }
    }
}
