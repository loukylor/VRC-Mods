using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MelonLoader;
using PlayerList.Components;
using PlayerList.Entries;
using PlayerList.UI;
using PlayerList.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using VRC;

[assembly: MelonInfo(typeof(PlayerList.PlayerListMod), "PlayerList", "1.2.8", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace PlayerList
{
    public class PlayerListMod : MelonMod
    {
        public static GameObject playerList;
        public static GameObject menuButton;
        public static RectTransform playerListRect;

        private static readonly Stopwatch timer = Stopwatch.StartNew();

        public static Label fontSizeLabel;
        private static int _fontSize;
        public static int FontSize
        {
            get { return _fontSize; }
            set
            {
                EntryManager.shouldSaveConfig = true;
                Config.fontSize.Value = value;
                _fontSize = value;
                fontSizeLabel.textComponent.text = $"Font Size: {value}";
                foreach (EntryBase entry in EntryManager.entries.Values)
                    entry.textComponent.fontSize = value;
            }
        }

        private static MenuButtonPositionEnum _menuButtonPosition;
        public static MenuButtonPositionEnum MenuButtonPosition
        {
            get { return _menuButtonPosition; }
            set
            {
                switch (value)
                {
                    case MenuButtonPositionEnum.TopRight:
                        menuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(4, -1));
                        menuButton.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                        break;
                    case MenuButtonPositionEnum.TopLeft:
                        menuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(2, -1));
                        menuButton.GetComponent<RectTransform>().pivot = new Vector2(1, 0);
                        break;
                    case MenuButtonPositionEnum.BottomLeft:
                        menuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(2, -1));
                        menuButton.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                        break;
                    case MenuButtonPositionEnum.BottomRight:
                        menuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(4, -1));
                        menuButton.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                        break;
                    default:
                        MenuButtonPosition = MenuButtonPositionEnum.TopRight;
                        return;
                }
                _menuButtonPosition = value;
                Config.MenuButtonPosition = value;
                EntryManager.shouldSaveConfig = true;
            }
        }

        public override void VRChat_OnUiManagerInit()
        {
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            Config.RegisterSettings();

            // Initialize Constants util
            Constants.UIInit();

            LoadAssetBundle();

            // Initialize UIManager
            UIManager.Init(Harmony);

            // Initialize submenu for the list 
            MenuManager.CreateMainSubMenu();

            // TODO: Add opacity options, maybe color too, (maybe even for each stage of ping and fps??)

            MenuManager.AddMenuListeners();
            MenuManager.CreateSubMenus();
            PlayerEntry.Patch(Harmony);
            EntryManager.AddGeneralInfoEntries();
            MenuManager.CreateGeneralInfoSubMenus();
            MenuManager.AdjustSubMenus();

            // Initialize on leave and join events
            NetworkHooks.NetworkInit();
            NetworkHooks.OnPlayerJoin += new Action<Player>((player) => OnPlayerJoin(player));
            NetworkHooks.OnPlayerLeave += new Action<Player>((player) => OnPlayerLeave(player));
            
            MelonLogger.Msg("Initialized!");
        }
        public static void LoadAssetBundle()
        {
            // Stolen from UIExpansionKit (https://github.com/knah/VRCMods/blob/master/UIExpansionKit) #Imnotaskidiswear
            MelonLogger.Msg("Loading List UI...");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlayerList.playerlistmod.assetbundle"))
            {
                using (var memoryStream = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(memoryStream);
                    AssetBundle assetBundle = AssetBundle.LoadFromMemory_Internal(memoryStream.ToArray(), 0);
                    assetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    playerList = UnityEngine.Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/Prefabs/PlayerListMod.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), Constants.quickMenu.transform);
                    menuButton = UnityEngine.Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/Prefabs/PlayerListMenuButton.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), Constants.shortcutMenu.transform);
                }
            }
            menuButton.SetLayerRecursive(12);
            menuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(4, -1));
            menuButton.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

            UiTooltip tooltip = menuButton.AddComponent<UiTooltip>();
            tooltip.field_Public_String_0 = "Open PlayerList menu";
            tooltip.field_Public_String_1 = "Open PlayerList menu";

            playerList.SetLayerRecursive(12);
            playerListRect = playerList.GetComponent<RectTransform>();
            playerListRect.anchoredPosition = Config.PlayerListPosition;
            playerListRect.localPosition = new Vector3(playerListRect.localPosition.x, playerListRect.localPosition.y, 25); // Do this or else it looks off for whatever reason
            playerList.SetActive(false);

            MenuManager.shouldStayHidden = !Config.enabledOnStart.Value;
            _fontSize = Config.fontSize.Value;
            MenuButtonPosition = Config.MenuButtonPosition;

            Constants.playerListLayout = playerList.transform.Find("PlayerList Viewport/PlayerList").GetComponent<VerticalLayoutGroup>();
            Constants.generalInfoLayout = playerList.transform.Find("GeneralInfo Viewport/GeneralInfo").GetComponent<VerticalLayoutGroup>();

            EnableDisableListener playerListListener = playerList.AddComponent<EnableDisableListener>();
            playerListListener.OnEnableEvent += EntryManager.RefreshAllEntries;
        }

        public override void OnUpdate()
        {
            if (timer.Elapsed.TotalSeconds > 1)
            {
                timer.Restart();
                EntryManager.RefreshAllEntries();
            }
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

                MenuManager.OnSceneWasLoaded();
                EntryManager.OnSceneWasLoaded();
                UIManager.OnSceneWasLoaded();
            }
        }
        public static void OnPlayerJoin(Player player)
        {
            EntryManager.OnPlayerJoin(player);
        }
        public static void OnPlayerLeave(Player player)
        {
            EntryManager.OnPlayerLeave(player);
        }

        public enum MenuButtonPositionEnum
        {
            TopRight,
            TopLeft,
            BottomLeft,
            BottomRight
        }
        public enum DisplayNameColorMode
        {
            TrustAndFriends,
            TrustOnly,
            FriendsOnly,
            None
        }
    }
}
