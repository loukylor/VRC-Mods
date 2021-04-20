using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using MelonLoader;
using UserInfoExtentions.Modules;
using UserInfoExtentions.Utilities;
using VRC.UI;

[assembly: MelonInfo(typeof(UserInfoExtensions.UserInfoExtensionsMod), "UserInfoExtensions", "2.4.1", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace UserInfoExtensions
{
    public class UserInfoExtensionsMod : MelonMod
    {
        public static UIExpansionKit.API.ICustomLayoutedMenu userDetailsMenu;
        public static UIExpansionKit.API.ICustomShowableLayoutedMenu menu;

        internal static List<ModuleBase> modules = new List<ModuleBase>();

        public override void OnApplicationStart()
        {

            VRCUtils.Init();

            Harmony.Patch(AccessTools.Method(typeof(MenuController), "Method_Public_Void_APIUser_0"), postfix: new HarmonyMethod(typeof(UserInfoExtensionsMod).GetMethod("OnUserInfoOpen", BindingFlags.Static | BindingFlags.Public)));
            Harmony.Patch(AccessTools.Method(typeof(PageUserInfo), "Back"), postfix: new HarmonyMethod(typeof(UserInfoExtensionsMod).GetMethod("OnUserInfoClose", BindingFlags.Static | BindingFlags.Public)));
            UIExpansionKit.API.LayoutDescription popupLayout = new UIExpansionKit.API.LayoutDescription
            {
                RowHeight = 80,
                NumColumns = 3,
                NumRows = 5
            };
            menu = UIExpansionKit.API.ExpansionKitApi.CreateCustomFullMenuPopup(popupLayout);
            userDetailsMenu = UIExpansionKit.API.ExpansionKitApi.GetExpandedMenu(UIExpansionKit.API.ExpandedMenu.UserDetailsMenu);

            menu.AddLabel("General Things");
            menu.AddSpacer();
            menu.AddSimpleButton("Back", () => menu.Hide());
            userDetailsMenu.AddSimpleButton("UserInfoExtensions", async () => 
            {
                await AsyncUtils.YieldToMainThread();
                HideAllPopups();
                menu.Show();
                foreach (ModuleBase module in modules)
                    module.OnUIEMenuOpen();
            });

            AddModule(new QuickMenuFromSocial());
            AddModule(new GetAvatarAuthor());
            AddModule(new OpenInWorldMenu());
            AddModule(new BioButtons());
            AddModule(new OpenInBrowser());
            AddModule(new UserInformation());

            MelonLogger.Msg("Initialized!");
        }
        public override void VRChat_OnUiManagerInit()
        {
            VRCUtils.UiInit();
            foreach (ModuleBase module in modules)
                module.UiInit();

            MelonLogger.Msg("UI Initialized!");
        }
        public override async void OnPreferencesSaved()
        {
            await AsyncUtils.YieldToMainThread();
            foreach (ModuleBase module in modules)
                module.OnPreferencesSaved();
        }
        public override void OnUpdate()
        {
            if (AsyncUtils.toMainThreadQueue.TryDequeue(out Action action)) action();
        }
        public static async void OnUserInfoOpen()
        {
            await AsyncUtils.YieldToMainThread();
            foreach (ModuleBase module in modules)
                module.OnUserInfoOpen();
        }
        public static void OnUserInfoClose() => menu.Hide();
        /*
        public static void OnPageOpen(VRCUiPage __0)
        {
            
        }
        */
        public static void HideAllPopups()
        {
            VRCUtils.ClosePopup();
            BioButtons.bioLanguagesPopup?.Close();
            BioButtons.bioLinksPopup?.Close();
            menu.Hide();
        }
        public static void AddModule(ModuleBase module)
        {
            modules.Add(module);
            module.Init();
        }
    }
}