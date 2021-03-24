using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using MelonLoader;
using UserInfoExtentions.Modules;
using UserInfoExtentions.Utilities;
using VRC.UI;

[assembly: MelonInfo(typeof(UserInfoExtensions.UserInfoExtensionsMod), "UserInfoExtensions", "2.3.6", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace UserInfoExtensions
{
    // TODO: You have shitty formatting
    // TODO: Make button toggles not require a restart
    // TODO: Make to quick menu actually select the person
    public class UserInfoExtensionsMod : MelonMod
    {
        public static UIExpansionKit.API.ICustomLayoutedMenu userDetailsMenu;
        public static UIExpansionKit.API.ICustomShowableLayoutedMenu menu;

        internal static Type[] modules = new Type[] { typeof(QuickMenuFromSocial), typeof(GetAvatarAuthor), typeof(OpenInWorldMenu), typeof(BioButtons), typeof(OpenInBrowser) };
        private static Dictionary<string, List<MethodInfo>> moduleMethods = new Dictionary<string, List<MethodInfo>>();

        public override void OnApplicationStart()
        {
            VRCUtils.TryExecuteMethod(typeof(VRCUtils).GetMethod("Init"));

            Harmony.Patch(AccessTools.Method(typeof(MenuController), "Method_Public_Void_APIUser_0"), postfix: new HarmonyMethod(typeof(UserInfoExtensionsMod).GetMethod("OnUserInfoOpen", BindingFlags.Static | BindingFlags.Public)));
            Harmony.Patch(AccessTools.Method(typeof(PageUserInfo), "Back"), postfix: new HarmonyMethod(typeof(UserInfoExtensionsMod).GetMethod("OnUserInfoClose", BindingFlags.Static | BindingFlags.Public)));
            UIExpansionKit.API.LayoutDescription popupLayout = new UIExpansionKit.API.LayoutDescription
            {
                RowHeight = 80,
                NumColumns = 3,
                NumRows = 6
            };
            menu = UIExpansionKit.API.ExpansionKitApi.CreateCustomFullMenuPopup(popupLayout);
            userDetailsMenu = UIExpansionKit.API.ExpansionKitApi.GetExpandedMenu(UIExpansionKit.API.ExpandedMenu.UserDetailsMenu);

            menu.AddLabel("General Things");
            menu.AddSpacer();
            menu.AddSimpleButton("Back", () => menu.Hide());
            userDetailsMenu.AddSimpleButton("UserInfoExtensions", () => { HideAllPopups(); menu.Show(); });

            RunModuleMethod("Init");

            MelonLogger.Msg("Initialized!");
        }
        public override void VRChat_OnUiManagerInit()
        {
            VRCUtils.TryExecuteMethod(typeof(VRCUtils).GetMethod("UiInit"));
            RunModuleMethod("UiInit");

            MelonLogger.Msg("UI Initialized!");
        }
        public override void OnPreferencesSaved() => RunModuleMethod("OnPreferencesSaved");
        public override void OnUpdate()
        {
            if (AsyncUtils.toMainThreadQueue.TryDequeue(out Action action)) action();
        }
        public static void RunModuleMethod(string methodName, object[] parameters = null)
        {
            if (!moduleMethods.ContainsKey(methodName))
            {
                moduleMethods.Add(methodName, new List<MethodInfo>());
                foreach (Type module in modules)
                {
                    MethodInfo moduleMethod = module.GetMethod(methodName);
                    if (moduleMethod != null)
                    {
                        moduleMethods[methodName].Add(moduleMethod);
                        VRCUtils.TryExecuteMethod(moduleMethod, parameters: parameters);
                    }
                }
                return;
            }
            else
            { 
                foreach (MethodInfo moduleMethod in moduleMethods[methodName])
                    VRCUtils.TryExecuteMethod(moduleMethod, parameters: parameters);
            }
        }
        public static void OnUserInfoOpen() => RunModuleMethod("OnUserInfoOpen");
        public static void OnUserInfoClose() => menu.Hide();
        public static void OnPageOpen(VRCUiPage __0) => RunModuleMethod("OnPageOpen", new object[] { __0 });
        public static void HideAllPopups()
        {
            VRCUtils.ClosePopup();
            BioButtons.bioLanguagesPopup.Close();
            BioButtons.bioLinksPopup.Close();
            menu.Hide();
        }
    }
}