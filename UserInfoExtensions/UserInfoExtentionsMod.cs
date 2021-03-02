using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using MelonLoader;
using UserInfoExtentions;
using UserInfoExtentions.Modules;
using VRC.UI;

[assembly: MelonInfo(typeof(UserInfoExtensions.UserInfoExtensionsMod), "UserInfoExtensions", "2.3.0", "loukylor", "https://github.com/loukylor/UserInfoExtensions")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace UserInfoExtensions
{
    public class UserInfoExtensionsMod : MelonMod
    {
        public static UIExpansionKit.API.ICustomLayoutedMenu userDetailsMenu;
        public static UIExpansionKit.API.ICustomShowableLayoutedMenu menu;

        private static IEnumerable<Type> modules;
        private static Dictionary<string, List<MethodInfo>> moduleMethods = new Dictionary<string, List<MethodInfo>>();

        public override void OnApplicationStart()
        {
            UserInfoExtensionsSettings.RegisterSettings();

            Utilities.TryExecuteMethod(typeof(Utilities).GetMethod("Init"));

            Harmony.Patch(AccessTools.Method(typeof(MenuController), "Method_Public_Void_APIUser_0"), postfix: new HarmonyMethod(typeof(UserInfoExtensionsMod).GetMethod("OnUserInfoOpen", BindingFlags.Static | BindingFlags.Public)));
            Harmony.Patch(AccessTools.Method(typeof(PageUserInfo), "Back"), postfix: new HarmonyMethod(typeof(UserInfoExtensionsMod).GetMethod("OnUserInfoClose", BindingFlags.Static | BindingFlags.Public)));
            Harmony.Patch(typeof(VRCUiManager).GetMethod("Method_Public_VRCUiPage_VRCUiPage_0"), new HarmonyMethod(typeof(UserInfoExtensionsMod).GetMethod("OnPageOpen")));
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

            modules = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && t.Namespace == "UserInfoExtentions.Modules");

            RunModuleMethod("Init");

            MelonLogger.Msg("Initialized!");
        }
        public override void VRChat_OnUiManagerInit()
        {
            Utilities.TryExecuteMethod(typeof(Utilities).GetMethod("UiInit"));
            RunModuleMethod("UiInit");

            MelonLogger.Msg("UI Initialized!");
        }
        public override void OnPreferencesSaved() => UserInfoExtensionsSettings.OnModSettingsApplied();
        public override void OnUpdate()
        {
            if (Utilities.ToMainThreadQueue.TryDequeue(out Action action)) action();
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
                        Utilities.TryExecuteMethod(moduleMethod, parameters: parameters);
                    }
                }
                return;
            }
            else
                foreach (MethodInfo moduleMethod in moduleMethods[methodName])
                    Utilities.TryExecuteMethod(moduleMethod, parameters: parameters);
        }
        public static void OnUserInfoOpen() => RunModuleMethod("OnUserInfoOpen");
        public static void OnUserInfoClose() => menu.Hide();
        public static void OnPageOpen(VRCUiPage __0) => RunModuleMethod("OnPageOpen", new object[] { __0 });
        public static void HideAllPopups()
        {
            Utilities.ClosePopup();
            BioButtons.bioLanguagesPopup.Close();
            BioButtons.bioLinksPopup.Close();
            menu.Hide();
        }
    }
}