using MelonLoader;
using PlayerList.Config;
using PlayerList.Components;
using PlayerList.Entries;
using PlayerList.UI;
using PlayerList.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine;

[assembly: MelonInfo(typeof(PlayerList.PlayerListMod), "PlayerList", "1.4.4", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies(new string[1] { "UIExpansionKit" })]

namespace PlayerList
{
    public class PlayerListMod : MelonMod
    {
        public static PlayerListMod Instance { get; private set; }

        public override void OnApplicationStart()
        {
            Instance = this;
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            PlayerListConfig.RegisterSettings();
            UIManager.Init();
            EntryManager.Init();
            ListPositionManager.Init();
            MenuManager.Init();
            EntrySortManager.Init();
            PlayerEntry.EntryInit();
            LocalPlayerEntry.EntryInit();
        }
        public override void VRChat_OnUiManagerInit()
        {
            // Initialize Constants util
            Constants.UIInit();
            UIManager.UIInit();

            // TODO: Add opacity options, maybe color too, (maybe even for each stage of ping and fps??)
            // TODO: Make is so the vector 2 acutlaly uses the custom mapper when it gets fixed
            // TODO: add load percentage??
            // TODO: add indicator for those in hearing distance
            // TODO: Crash indication

            MenuManager.LoadAssetBundle();

            // Initialize submenu for the list 
            MenuManager.CreateMainSubMenu();

            // This is kinda a mess but whatever
            MenuManager.AddMenuListeners();
            MenuManager.CreateSortPages();
            MenuManager.CreateSubMenus();
            EntryManager.AddGeneralInfoEntries();
            MenuManager.CreateGeneralInfoSubMenus();
            MenuManager.AdjustSubMenus();

            // Initialize on network events
            NetworkEvents.NetworkInit();

            PlayerListConfig.OnConfigChange(false);

            MelonLogger.Msg("Initialized!");
        }

        public override void OnUpdate()
        {
            EntryManager.OnUpdate();
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.F1)) MenuManager.ToggleMenu();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != -1) return;

            MenuManager.OnSceneWasLoaded();
            Constants.OnSceneWasLoaded();
            EntryManager.OnSceneWasLoaded();
        }
    }
}
