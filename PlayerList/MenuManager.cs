using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using PlayerList.Components;
using PlayerList.Config;
using PlayerList.Entries;
using PlayerList.UI;
using PlayerList.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerList
{
    class MenuManager
    {
        public static List<SubMenu> playerListMenus = new List<SubMenu>();
        public static ToggleButton menuToggleButton;

        public static bool shouldStayHidden;

        public static Label fontSizeLabel;

        public static GameObject playerList;
        public static RectTransform playerListRect;

        public static GameObject menuButton;

        public static void Init()
        {
            PlayerListConfig.OnConfigChangedEvent += OnConfigChanged;
        }
        public static void OnConfigChanged()
        {
            switch (PlayerListConfig.MenuButtonPosition)
            {
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
                    menuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(4, -1));
                    menuButton.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                    break;
            }
        }

        public static void ToggleMenu()
        {
            if (!playerListMenus.Any(subMenu => subMenu.gameObject.active) && !Constants.shortcutMenu.active) return;
            shouldStayHidden = !shouldStayHidden;
            menuToggleButton.State = !shouldStayHidden;
            if (playerListMenus.Any(subMenu => subMenu.gameObject.active) || Constants.shortcutMenu.active) playerList.SetActive(!playerList.activeSelf);
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
            playerListRect.anchoredPosition = PlayerListConfig.PlayerListPosition;
            playerListRect.localPosition = new Vector3(playerListRect.localPosition.x, playerListRect.localPosition.y, 25); // Do this or else it looks off for whatever reason
            playerList.SetActive(false);

            shouldStayHidden = !PlayerListConfig.enabledOnStart.Value;

            Constants.playerListLayout = playerList.transform.Find("PlayerList Viewport/PlayerList").GetComponent<VerticalLayoutGroup>();
            Constants.generalInfoLayout = playerList.transform.Find("GeneralInfo Viewport/GeneralInfo").GetComponent<VerticalLayoutGroup>();

            EnableDisableListener playerListListener = playerList.AddComponent<EnableDisableListener>();
            playerListListener.OnEnableEvent += EntryManager.RefreshAllEntries;
        }
        public static void CreateMainSubMenu()
        {
            MelonLogger.Msg("Initializing Menu...");
            playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage1"));

            SingleButton backButton = new SingleButton(playerListMenus[0].path, new Vector3(5, 2), "Back", new Action(() => UIManager.OpenPage("UserInterface/QuickMenu/ShortcutMenu")), "Press to go back", "BackButton");
            backButton.textComponent.color = Color.yellow;

            menuToggleButton = new ToggleButton(playerListMenus[0].path, new Vector3(5, 0), "Enabled", "Disabled", new Action<bool>((state) => ToggleMenu()), "Toggle the menu. Can also be toggled using Left Ctrl + F1", "Toggle the menu. Can also be toggled using Left Ctrl + F1", "ToggleMenuToggle", !shouldStayHidden);

            new ToggleButton(playerListMenus[0].path, new Vector3(5, -1), "Enabled on Start", "Disabled", new Action<bool>((state) => PlayerListConfig.enabledOnStart.Value = state), "Toggle if the list is toggled hidden on start", "Toggle if the list is toggled hidden on start", "EnabledOnStartToggle", PlayerListConfig.enabledOnStart.Value, true);

            new ToggleButton(playerListMenus[0].path, new Vector3(0, 1), "Condense Text", "Regular Text", new Action<bool>((state) => PlayerListConfig.condensedText.Value = !PlayerListConfig.condensedText.Value), "Toggle if text should be condensed", "Toggle if text should be condensed", "CondensedTextToggle", PlayerListConfig.condensedText.Value, true);
            new ToggleButton(playerListMenus[0].path, new Vector3(0, 0), "Numbered List", "Tick List", new Action<bool>((state) => PlayerListConfig.numberedList.Value = !PlayerListConfig.numberedList.Value), "Toggle if the list should be numbered or ticked", "Toggle if the list should be numbered or ticked", "NumberedTickToggle", PlayerListConfig.numberedList.Value, true);

            menuButton.GetComponent<Button>().onClick.AddListener(new Action(() => { UIManager.OpenPage(playerListMenus[0].path); playerList.SetActive(!shouldStayHidden); }));
        }
        public static void AddMenuListeners()
        {
            // Add listeners
            EnableDisableListener shortcutMenuListener = Constants.shortcutMenu.AddComponent<EnableDisableListener>();
            shortcutMenuListener.OnEnableEvent += new Action(() => { playerList.SetActive(!shouldStayHidden); UIManager.CurrentMenu = Constants.shortcutMenu; });
            shortcutMenuListener.OnDisableEvent += new Action(() => playerList.SetActive(false));
            // TODO: add listeners to tab buttons to close my menu or to make it so tabs are inaccesible when menu is open
            GameObject newElements = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements");
            GameObject Tabs = GameObject.Find("UserInterface/QuickMenu/QuickModeTabs");

            UIManager.OnQuickMenuCloseEvent += new Action(PlayerListConfig.SaveEntries);

            EnableDisableListener playerListMenuListener = playerListMenus[0].gameObject.AddComponent<EnableDisableListener>();
            playerListMenuListener.OnEnableEvent += new Action(() =>
            {
                playerList.SetActive(!shouldStayHidden);
                playerListRect.anchoredPosition = Converters.ConvertToUnityUnits(new Vector3(2.5f, 3.5f));
                Tabs.SetActive(false);
                newElements.SetActive(false);
            });
            playerListMenuListener.OnDisableEvent += new Action(() =>
            {
                playerList.SetActive(false);
                playerListRect.anchoredPosition = PlayerListConfig.PlayerListPosition;
                playerListRect.localPosition = new Vector3(playerListRect.localPosition.x, playerListRect.localPosition.y, 25);
                Tabs.SetActive(true);
                newElements.SetActive(true);
            });
        }
        public static void CreateSubMenus()
        {
            // Initialize Movement menu
            playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage2"));

            new SingleButton(playerListMenus[1].path, new Vector3(3, 0), "Edit PlayerList Position", new Action(ListPositionManager.MovePlayerList), "Click to edit the position of the PlayerList", "EditPlayerListPosButton", true);

            new SingleButton(playerListMenus[1].path, new Vector3(3, 1), "Move to Right of QuickMenu", new Action(ListPositionManager.MovePlayerListToEndOfMenu), "Move PlayerList to right side of menu, this can also serve as a reset position button", "LockPlayerListToRightButton", true);

            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(0, 0), "1", new Action(() => PlayerListConfig.MenuButtonPosition = MenuButtonPositionEnum.TopRight), "Move PlayerList menu button to the top right", "1PlayerListMenuButton");
            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(1, 0), "2", new Action(() => PlayerListConfig.MenuButtonPosition = MenuButtonPositionEnum.TopLeft), "Move PlayerList menu button to the top left", "2PlayerListMenuButton");
            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(1, 1), "3", new Action(() => PlayerListConfig.MenuButtonPosition = MenuButtonPositionEnum.BottomLeft), "Move PlayerList menu button to the bottom left", "3PlayerListMenuButton");
            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(0, 1), "4", new Action(() => PlayerListConfig.MenuButtonPosition = MenuButtonPositionEnum.BottomRight), "Move PlayerList menu button to the bottom right", "4PlayerListMenuButton");

            new SingleButton(playerListMenus[1].path, new Vector3(1, 0), "Snap Grid\nSize +", new Action(() => PlayerListConfig.snapToGridSize.Value += 10), "Increase the size of the snap to grid by 10", "IncreaseSnapGridSize", true);
            new SingleButton(playerListMenus[1].path, new Vector3(1, 2), "Snap Grid\nSize -", new Action(() => PlayerListConfig.snapToGridSize.Value -= 10), "Decrease the size of the snap to grid by 10", "DecreaseSnapGridSize", true);
            new SingleButton(playerListMenus[1].path, new Vector3(0, 0), "Reset Snap\nGrid Size", new Action(() => PlayerListConfig.snapToGridSize.Value = 420), "Set snap to grid to the default value (420)", "DefaultSnapGridSize", true);
            ListPositionManager.snapToGridSizeLabel = new Label(playerListMenus[1].path, new Vector3(1, 1), $"Snap Grid\nSize: {PlayerListConfig.snapToGridSize.Value}", "SnapToGridSizeLabel", resize: true);

            new SingleButton(playerListMenus[1].path, new Vector3(2, 0), "Font\nSize +", new Action(() => PlayerListConfig.fontSize.Value++), "Increase font size of the list by 1", "IncreaseFontSizeButton", true);
            new SingleButton(playerListMenus[1].path, new Vector3(2, 2), "Font\nSize -", new Action(() => PlayerListConfig.fontSize.Value--), "Decrease font size of the list by 1", "DecreaseFontSizeButton", true);
            new SingleButton(playerListMenus[1].path, new Vector3(0, 1), "Reset\nFont", new Action(() => PlayerListConfig.fontSize.Value = 35), "Set font size to the default value (35)", "DefaultFontSizeButton", true);
            fontSizeLabel = new Label(playerListMenus[1].path, new Vector3(2, 1), "", "FontSizeLabel", resize: true);
            EntryManager.SetFontSize(PlayerListConfig.fontSize.Value);

            //playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage3"));


            // Initialize PlayerList Customization menu
            playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage4"));

            new ToggleButton(playerListMenus[3].path, new Vector3(1, 0), "Enable Ping", "Disabled", new Action<bool>((state) => PlayerListConfig.pingToggle.Value = state), "Toggle player ping", "Toggle player ping", "PingToggle", PlayerListConfig.pingToggle.Value, true);
            new ToggleButton(playerListMenus[3].path, new Vector3(2, 0), "Enable Fps", "Disabled", new Action<bool>((state) => PlayerListConfig.fpsToggle.Value = state), "Toggle player fps", "Toggle player fps", "FpsToggle", PlayerListConfig.fpsToggle.Value, true);
            new ToggleButton(playerListMenus[3].path, new Vector3(3, 0), "Enable Platform", "Disabled", new Action<bool>((state) => PlayerListConfig.platformToggle.Value = state), "Toggle player Platform", "Toggle player Platform", "PlatformToggle", PlayerListConfig.platformToggle.Value, true);
            new ToggleButton(playerListMenus[3].path, new Vector3(1, 1), "Enable Avatar Performance", "Disabled", new Action<bool>((state) => PlayerListConfig.perfToggle.Value = state), "Toggle avatar performance", "Toggle avatar performance", "PerfToggle", PlayerListConfig.perfToggle.Value, true);
            new ToggleButton(playerListMenus[3].path, new Vector3(2, 1), "Enable Distance", "Disabled", new Action<bool>((state) => PlayerListConfig.distanceToggle.Value = state), "Toggle distance to player", "Toggle distance to player", "DistanceToPlayerToggle", PlayerListConfig.distanceToggle.Value, true);
            new ToggleButton(playerListMenus[3].path, new Vector3(3, 1), "Enable DisplayName", "Disabled", new Action<bool>((state) => PlayerListConfig.displayNameToggle.Value = state), "Why...?", "Why...?", "DisplayNameToggle", PlayerListConfig.displayNameToggle.Value, true);

            new QuarterButton(playerListMenus[3].path, new Vector3(3, 2), new Vector2(0, 0), "TF", new Action(() => PlayerListConfig.DisplayNameColorMode = PlayerEntry.DisplayNameColorMode.TrustAndFriends), "Set displayname coloring to show friends and trust rank", "TrustAndFriendsButton");
            new QuarterButton(playerListMenus[3].path, new Vector3(3, 2), new Vector2(1, 0), "T", new Action(() => PlayerListConfig.DisplayNameColorMode = PlayerEntry.DisplayNameColorMode.TrustOnly), "Set displayname coloring to show trust rank only", "TrustOnlyButton");
            new QuarterButton(playerListMenus[3].path, new Vector3(3, 2), new Vector2(1, 1), "F", new Action(() => PlayerListConfig.DisplayNameColorMode = PlayerEntry.DisplayNameColorMode.FriendsOnly), "Set displayname coloring to show friends only", "FriendsOnlyButton");
            new QuarterButton(playerListMenus[3].path, new Vector3(3, 2), new Vector2(0, 1), "N", new Action(() => PlayerListConfig.DisplayNameColorMode = PlayerEntry.DisplayNameColorMode.None), "Set displayname coloring to none", "NoneButton");
        }

        public static void CreateGeneralInfoSubMenus()
        {
            // Create Toggle Button Submenus (done automatically to enable expandability)
            int totalMade = 0;
            for (int i = 0; i < Math.Ceiling(EntryManager.generalInfoEntries.Count / 9f); i++)
            {
                SubMenu subMenu = new SubMenu("UserInterface/QuickMenu", $"PlayerListMenuPage{i + 5}");

                for (; totalMade < (9 * (i + 1)) && totalMade < EntryManager.generalInfoEntries.Count; totalMade++)
                {
                    EntryBase entry = EntryManager.generalInfoEntries.ElementAt(totalMade);
                    ToggleButton toggle = new ToggleButton(subMenu.path, new Vector3((totalMade % 3) + 1, (float)Math.Floor((totalMade - (9 * i)) / 3f)), $"Enable {entry.Name}", $"Disabled", new Action<bool>((state) => { entry.gameObject.SetActive(state); entry.prefEntry.Value = state; }), $"Toggle the {entry.Name} entry", $"Toggle the {entry.Name} entry", $"{entry.Name.Replace(" ", "")}EntryToggle", entry.prefEntry.Value, true);
                }

                playerListMenus.Add(subMenu);
            }
        }
        public static void AdjustSubMenus()
        {
            for (int i = 0; i < playerListMenus.Count; i++)
            {
                int k = i; // dum reference stuff

                if (i > 0)
                {
                    new SingleButton(playerListMenus[i].path, "UserInterface/QuickMenu/EmojiMenu/PageUp", new Vector3(4, 0), $"Page {i}", new Action(() => UIManager.OpenPage($"UserInterface/QuickMenu/PlayerListMenuPage{k}")), $"Go back to page {i}", "BackPageButton");
                    new SingleButton(playerListMenus[i].path, new Vector3(4, 1), $"Save", new Action(PlayerListConfig.SaveEntries), $"Saves all settings if you have made changes, this is also done automatically when you close the menu", "SaveEntriesButton");
                }
                if (i + 1 < playerListMenus.Count)
                    new SingleButton(playerListMenus[i].path, "UserInterface/QuickMenu/EmojiMenu/PageDown", new Vector3(4, 2), $"Page {i + 2}", new Action(() => UIManager.OpenPage($"UserInterface/QuickMenu/PlayerListMenuPage{k + 2}")), $"Go to page {i + 2}", "ForwardPageButton");

                if (i == 0) continue; // Skip main config menu

                EnableDisableListener subMenuListener = playerListMenus[i].gameObject.AddComponent<EnableDisableListener>();
                subMenuListener.OnEnableEvent += new Action(() =>
                {
                    playerList.SetActive(!shouldStayHidden);
                    playerListRect.anchoredPosition = Converters.ConvertToUnityUnits(new Vector3(6.5f, 3.5f));
                });
                subMenuListener.OnDisableEvent += new Action(() =>
                {
                    playerList.SetActive(false);
                    playerListRect.anchoredPosition = PlayerListConfig.PlayerListPosition;
                    playerListRect.localPosition = new Vector3(playerListRect.localPosition.x, playerListRect.localPosition.y, 25);
                });
            }
        }
        public enum MenuButtonPositionEnum
        {
            TopRight,
            TopLeft,
            BottomLeft,
            BottomRight
        }
    }
}
