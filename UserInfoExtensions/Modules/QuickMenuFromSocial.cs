using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UserInfoExtensions;
using UserInfoExtentions.Utilities;
using VRC;

namespace UserInfoExtentions.Modules
{
    public class QuickMenuFromSocial : ModuleBase
    {
        public static MelonPreferences_Entry<bool> QuickMenuFromSocialButton;

        public static GameObject quickMenuFromSocialButtonGameObject;

        private static MethodInfo closeMenu;
        private static MethodInfo openQuickMenu;
        private static MethodInfo setMenuIndex;

        public override void Init()
        {
            QuickMenuFromSocialButton = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(QuickMenuFromSocialButton), false, "Show \"To Quick Menu\" button");
            UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("To Quick Menu", ToQuickMenu, new Action<GameObject>((gameObject) => { quickMenuFromSocialButtonGameObject = gameObject; gameObject.SetActive(QuickMenuFromSocialButton.Value); }));
            UserInfoExtensionsMod.menu.AddSimpleButton("To Quick Menu", ToQuickMenu);

            closeMenu = typeof(VRCUiManager).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && !mb.Name.Contains("_PDM_"))
                .OrderBy(mb => UnhollowerSupport.GetIl2CppMethodCallerCount(mb)).Last();

            foreach (MethodInfo method in typeof(QuickMenu).GetMethods().Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && mb.Name.Length <= 29 && !mb.Name.Contains("PDM")))
            {
                MethodBase[] possibleMethods = null;
                try
                {
                    possibleMethods = XrefScanner.UsedBy(method)
                        .Where(instance => instance.Type == XrefType.Method && instance.TryResolve() != null && instance.TryResolve().Name.StartsWith("Method_Public_Void_") && !instance.TryResolve().Name.Contains("_PDM_") && instance.TryResolve().GetParameters().Length == 0)
                        .Select(instance => instance.TryResolve())
                        .ToArray();
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                if (possibleMethods.Length == 0)
                    continue;

                foreach (MethodInfo possibleMethod in possibleMethods)
                { 
                    if (XrefScanner.UsedBy(possibleMethod).Any(instance => instance.Type == XrefType.Method && instance.TryResolve() != null && instance.TryResolve().Name.Contains("OpenQuickMenu")))
                    {
                        openQuickMenu = method;
                        break;
                    }
                }

                if (openQuickMenu != null)
                    break;
            }

            if (openQuickMenu == null)
                throw new InvalidOperationException("The fact that this fucking fat ass of an xref broke is going to drive me insane");

            List<Type> quickMenuNestedEnums = typeof(QuickMenu).GetNestedTypes().Where(type => type.IsEnum).ToList();
            PropertyInfo quickMenuEnumProperty = typeof(QuickMenu).GetProperties()
                .First(pi => pi.PropertyType.IsEnum && quickMenuNestedEnums.Contains(pi.PropertyType));
            setMenuIndex = typeof(QuickMenu).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Enum") && !mb.Name.Contains("_PDM_") && mb.GetParameters()[0].ParameterType == quickMenuEnumProperty.PropertyType);
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
                if (player.prop_APIUser_0.id == VRCUtils.ActiveUser.id)
                {
                    closeMenu.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[] { true, false }); //Closes Big Menu
                    openQuickMenu.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { true }); //Opens Quick Menu

                    QuickMenu.prop_QuickMenu_0.field_Private_Player_0 = player;
                    QuickMenu.prop_QuickMenu_0.prop_APIUser_0 = player.prop_APIUser_0;
                    setMenuIndex.Invoke(QuickMenu.prop_QuickMenu_0, new object[1] { 3 });

                    return;
                }
            }
            VRCUtils.OpenPopupV2("Notice:", "You cannot show this user on the Quick Menu because they are not in the same instance", "Close", new Action(VRCUtils.ClosePopup));
        }
    }
}
