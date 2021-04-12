using MelonLoader;
using PlayerList.Config;
using PlayerList.Components;
using PlayerList.Entries;
using PlayerList.UI;
using PlayerList.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine;

[assembly: MelonInfo(typeof(PlayerList.PlayerListMod), "PlayerList", "1.3.1", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

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
        }
        public override void VRChat_OnUiManagerInit()
        {
            // Initialize Constants util
            Constants.UIInit();

            MenuManager.LoadAssetBundle();

            // Initialize submenu for the list 
            MenuManager.CreateMainSubMenu();

            // TODO: Add opacity options, maybe color too, (maybe even for each stage of ping and fps??)
            // TODO: Add option to sort playerlist

            // This is kinda a mess but whatever
            MenuManager.AddMenuListeners();
            MenuManager.CreateSubMenus();
            PlayerEntry.EntryInit();
            LocalPlayerEntry.EntryInit();
            EntryManager.AddGeneralInfoEntries();
            MenuManager.CreateGeneralInfoSubMenus();
            MenuManager.AdjustSubMenus();

            // Initialize on leave and join events
            NetworkHooks.NetworkInit();

            PlayerListConfig.OnConfigChanged();
            
            MelonLogger.Msg("Initialized!");
        }

        public override void OnUpdate()
        {
            EntryManager.OnUpdate();
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.F1)) MenuManager.ToggleMenu();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex == -1)
            {
                if (Constants.quickMenuColliderSize != null)
                {
                    Constants.quickMenuColliderSize = Constants.quickMenu.GetComponent<BoxCollider>().size;
                    ListPositionManager.CombineQMColliderAndPlayerListRect();
                }

                EntryManager.OnSceneWasLoaded();
                UIManager.OnSceneWasLoaded();
            }
        }
    }
}
