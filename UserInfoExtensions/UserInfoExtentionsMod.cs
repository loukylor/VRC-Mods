using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UIExpansionKit.API;
using UserInfoExtentions.Modules;
using VRC.UI;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;

[assembly: MelonInfo(typeof(UserInfoExtensions.UserInfoExtensionsMod), "UserInfoExtensions", "2.5.4", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace UserInfoExtensions
{
    public class UserInfoExtensionsMod : MelonMod
    {
        public static ICustomLayoutedMenu userDetailsMenu;
        public static ICustomShowableLayoutedMenu menu;

        internal static List<ModuleBase> modules = new List<ModuleBase>();

        public static UserInfoExtensionsMod Instance { private set; get; }

        public override void OnApplicationStart()
        {            
            Instance = this;
            CacheManager.Init();

            foreach (MethodInfo method in typeof(MenuController).GetMethods().Where(mi => mi.Name.StartsWith("Method_Public_Void_APIUser_") && !mi.Name.Contains("_PDM_")))
                HarmonyInstance.Patch(method, postfix: new HarmonyMethod(typeof(UserInfoExtensionsMod).GetMethod("OnUserInfoOpen", BindingFlags.Static | BindingFlags.Public)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(PageUserInfo), "Back"), postfix: new HarmonyMethod(typeof(UserInfoExtensionsMod).GetMethod("OnUserInfoClose", BindingFlags.Static | BindingFlags.Public)));
            LayoutDescription popupLayout = new LayoutDescription
            {
                RowHeight = 80,
                NumColumns = 3,
                NumRows = 5
            };
            menu = ExpansionKitApi.CreateCustomFullMenuPopup(popupLayout);
            userDetailsMenu = ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserDetailsMenu);

            menu.AddLabel("General Things");
            menu.AddSpacer();
            menu.AddSimpleButton("Back", () => menu.Hide());
            userDetailsMenu.AddSimpleButton("UserInfoExtensions", () => 
            {
                HideAllPopups();
                menu.Show();
                foreach (ModuleBase module in modules)
                    module.OnUIXMenuOpen();
            });

            AddModule(new QuickMenuFromSocial());
            AddModule(new GetAvatarAuthor());
            AddModule(new BioButtons());
            AddModule(new UserInformation());

            VRCUtils.OnUiManagerInit += OnUiManagerInit;

            MelonLogger.Msg("Initialized!");
        }
        public void OnUiManagerInit()
        {
            foreach (ModuleBase module in modules)
                module.UiInit();

            MelonLogger.Msg("UI Initialized!");
        }
        public override void OnPreferencesSaved()
        {
            foreach (ModuleBase module in modules)
                module.OnPreferencesSaved();
        }
        public static void OnUserInfoOpen()
        {
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
            UiManager.ClosePopup();
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