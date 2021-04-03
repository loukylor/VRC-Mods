using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using PlayerList.Components;
using PlayerList.Entries;
using PlayerList.UI;
using PlayerList.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerList
{
    class MenuManager
    {
        public static List<SubMenu> playerListMenus = new List<SubMenu>();
        public static ToggleButton menuToggleButton;

        public static bool shouldStayHidden;

        public static void ToggleMenu()
        {
            if (!playerListMenus.Any(subMenu => subMenu.gameObject.active) && !Constants.shortcutMenu.active) return;
            shouldStayHidden = !shouldStayHidden;
            menuToggleButton.State = !shouldStayHidden;
            if (playerListMenus.Any(subMenu => subMenu.gameObject.active) || Constants.shortcutMenu.active) PlayerListMod.playerList.SetActive(!PlayerListMod.playerList.activeSelf);
        }

        public static void CreateMainSubMenu()
        {
            MelonLogger.Msg("Initializing Menu...");
            playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage1"));

            SingleButton backButton = new SingleButton(playerListMenus[0].path, new Vector3(5, 2), "Back", new Action(() => UIManager.OpenPage("UserInterface/QuickMenu/ShortcutMenu")), "Press to go back", "BackButton");
            backButton.textComponent.color = Color.yellow;

            menuToggleButton = new ToggleButton(playerListMenus[0].path, new Vector3(5, 0), "Enabled", "Disabled", new Action<bool>((state) => ToggleMenu()), "Toggle the menu. Can also be toggled using Left Ctrl + F1", "Toggle the menu. Can also be toggled using Left Ctrl + F1", "ToggleMenuToggle", !shouldStayHidden);

            new ToggleButton(playerListMenus[0].path, new Vector3(5, -1), "Enabled on Start", "Disabled", new Action<bool>((state) => { Config.enabledOnStart.Value = state; EntryManager.shouldSaveConfig = true; }), "Toggle if the list is toggled hidden on start", "Toggle if the list is toggled hidden on start", "EnabledOnStartToggle", Config.enabledOnStart.Value, true);

            new ToggleButton(playerListMenus[0].path, new Vector3(0, 1), "Condense Text", "Regular Text", new Action<bool>((state) => { Config.condensedText.Value = !Config.condensedText.Value; EntryManager.RefreshPlayerEntries(true); }), "Toggle if text should be condensed", "Toggle if text should be condensed", "CondensedTextToggle", Config.condensedText.Value, true);
            new ToggleButton(playerListMenus[0].path, new Vector3(0, 0), "Numbered List", "Tick List", new Action<bool>((state) => { Config.numberedList.Value = !Config.numberedList.Value; EntryManager.RefreshPlayerEntries(true); }), "Toggle if the list should be numbered or ticked", "Toggle if the list should be numbered or ticked", "NumberedTickToggle", Config.numberedList.Value, true);

            PlayerListMod.menuButton.GetComponent<Button>().onClick.AddListener(new Action(() => { UIManager.OpenPage(playerListMenus[0].path); PlayerListMod.playerList.SetActive(!shouldStayHidden); }));
        }
        public static void AddMenuListeners()
        {
            // Add listeners
            EnableDisableListener shortcutMenuListener = Constants.shortcutMenu.AddComponent<EnableDisableListener>();
            shortcutMenuListener.OnEnableEvent += new Action(() => { PlayerListMod.playerList.SetActive(!shouldStayHidden); UIManager.CurrentMenu = Constants.shortcutMenu; });
            shortcutMenuListener.OnDisableEvent += new Action(() => PlayerListMod.playerList.SetActive(false));
            // TODO: add listeners to tab buttons to close my menu or to make it so tabs are inaccesible when menu is open
            GameObject newElements = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements");
            GameObject Tabs = GameObject.Find("UserInterface/QuickMenu/QuickModeTabs");

            UIManager.OnQuickMenuCloseEvent += new Action(EntryManager.SaveEntries);

            EnableDisableListener playerListMenuListener = playerListMenus[0].gameObject.AddComponent<EnableDisableListener>();
            playerListMenuListener.OnEnableEvent += new Action(() =>
            {
                PlayerListMod.playerList.SetActive(!shouldStayHidden);
                PlayerListMod.playerListRect.anchoredPosition = Converters.ConvertToUnityUnits(new Vector3(2.5f, 3.5f));
                Tabs.SetActive(false);
                newElements.SetActive(false);
            });
            playerListMenuListener.OnDisableEvent += new Action(() =>
            {
                PlayerListMod.playerList.SetActive(false);
                PlayerListMod.playerListRect.anchoredPosition = Config.PlayerListPosition;
                PlayerListMod.playerListRect.localPosition = new Vector3(PlayerListMod.playerListRect.localPosition.x, PlayerListMod.playerListRect.localPosition.y, 25);
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

            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(0, 0), "1", new Action(() => PlayerListMod.MenuButtonPosition = PlayerListMod.MenuButtonPositionEnum.TopRight), "Move PlayerList menu button to the top right", "1PlayerListMenuButton");
            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(1, 0), "2", new Action(() => PlayerListMod.MenuButtonPosition = PlayerListMod.MenuButtonPositionEnum.TopLeft), "Move PlayerList menu button to the top left", "2PlayerListMenuButton");
            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(1, 1), "3", new Action(() => PlayerListMod.MenuButtonPosition = PlayerListMod.MenuButtonPositionEnum.BottomLeft), "Move PlayerList menu button to the bottom left", "3PlayerListMenuButton");
            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(0, 1), "4", new Action(() => PlayerListMod.MenuButtonPosition = PlayerListMod.MenuButtonPositionEnum.BottomRight), "Move PlayerList menu button to the bottom right", "4PlayerListMenuButton");

            new SingleButton(playerListMenus[1].path, new Vector3(1, 0), "Snap Grid\nSize +", new Action(() => ListPositionManager.SnapToGridSize += 10), "Increase the size of the snap to grid by 10", "IncreaseSnapGridSize", true);
            new SingleButton(playerListMenus[1].path, new Vector3(1, 2), "Snap Grid\nSize -", new Action(() => ListPositionManager.SnapToGridSize -= 10), "Decrease the size of the snap to grid by 10", "DecreaseSnapGridSize", true);
            new SingleButton(playerListMenus[1].path, new Vector3(0, 0), "Reset Snap\nGrid Size", new Action(() => ListPositionManager.SnapToGridSize = 420), "Set snap to grid to the default value (420)", "DefaultSnapGridSize", true);
            ListPositionManager.snapToGridSizeLabel = new Label(playerListMenus[1].path, new Vector3(1, 1), $"Snap Grid\nSize: {ListPositionManager.SnapToGridSize}", "SnapToGridSizeLabel", resize: true);
            ListPositionManager.SnapToGridSize = Config.snapToGridSize.Value;

            new SingleButton(playerListMenus[1].path, new Vector3(2, 0), "Font\nSize +", new Action(() => PlayerListMod.FontSize++), "Increase font size of the list by 1", "IncreaseFontSizeButton", true);
            new SingleButton(playerListMenus[1].path, new Vector3(2, 2), "Font\nSize -", new Action(() => PlayerListMod.FontSize--), "Decrease font size of the list by 1", "DecreaseFontSizeButton", true);
            new SingleButton(playerListMenus[1].path, new Vector3(0, 1), "Reset\nFont", new Action(() => PlayerListMod.FontSize = 35), "Set font size to the default value (35)", "DefaultFontSizeButton", true);
            PlayerListMod.fontSizeLabel = new Label(playerListMenus[1].path, new Vector3(2, 1), $"Font\nSize: {PlayerListMod.FontSize}", "FontSizeLabel", resize: true);

            // Initialize PlayerList Customization menu
            playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage3"));

            new ToggleButton(playerListMenus[2].path, new Vector3(1, 0), "Enable Ping", "Disabled", new Action<bool>((state) => { Config.pingToggle.Value = state; EntryManager.RefreshPlayerEntries(true); }), "Toggle player ping", "Toggle player ping", "PingToggle", Config.pingToggle.Value, true);
            new ToggleButton(playerListMenus[2].path, new Vector3(2, 0), "Enable Fps", "Disabled", new Action<bool>((state) => { Config.fpsToggle.Value = state; EntryManager.RefreshPlayerEntries(true); }), "Toggle player fps", "Toggle player fps", "FpsToggle", Config.fpsToggle.Value, true);
            new ToggleButton(playerListMenus[2].path, new Vector3(3, 0), "Enable Platform", "Disabled", new Action<bool>((state) => { Config.platformToggle.Value = state; EntryManager.RefreshPlayerEntries(true); }), "Toggle player Platform", "Toggle player Platform", "PlatformToggle", Config.platformToggle.Value, true);
            new ToggleButton(playerListMenus[2].path, new Vector3(1, 1), "Enable Avatar Performance", "Disabled", new Action<bool>((state) => { Config.perfToggle.Value = state; EntryManager.RefreshPlayerEntries(true); }), "Toggle avatar performance", "Toggle avatar performance", "PerfToggle", Config.perfToggle.Value, true);
            new ToggleButton(playerListMenus[2].path, new Vector3(2, 1), "Enable Distance", "Disabled", new Action<bool>((state) => { Config.distanceToggle.Value = state; EntryManager.RefreshPlayerEntries(true); }), "Toggle distance to player", "Toggle distance to player", "DistanceToPlayerToggle", Config.distanceToggle.Value, true);
            new ToggleButton(playerListMenus[2].path, new Vector3(3, 1), "Enable DisplayName", "Disabled", new Action<bool>((state) => { Config.displayNameToggle.Value = state; EntryManager.RefreshPlayerEntries(true); }), "Why...?", "Why...?", "DisplayNameToggle", Config.displayNameToggle.Value, true);

            new QuarterButton(playerListMenus[2].path, new Vector3(3, 2), new Vector2(0, 0), "TF", new Action(() => { Config.DisplayNameColorMode = PlayerListMod.DisplayNameColorMode.TrustAndFriends; EntryManager.RefreshPlayerEntries(true); }), "Set displayname coloring to show friends and trust rank", "TrustAndFriendsButton");
            new QuarterButton(playerListMenus[2].path, new Vector3(3, 2), new Vector2(1, 0), "T", new Action(() => { Config.DisplayNameColorMode = PlayerListMod.DisplayNameColorMode.TrustOnly; EntryManager.RefreshPlayerEntries(true); }), "Set displayname coloring to show trust rank only", "TrustOnlyButton");
            new QuarterButton(playerListMenus[2].path, new Vector3(3, 2), new Vector2(1, 1), "F", new Action(() => { Config.DisplayNameColorMode = PlayerListMod.DisplayNameColorMode.FriendsOnly; EntryManager.RefreshPlayerEntries(true); }), "Set displayname coloring to show friends only", "FriendsOnlyButton");
            new QuarterButton(playerListMenus[2].path, new Vector3(3, 2), new Vector2(0, 1), "N", new Action(() => { Config.DisplayNameColorMode = PlayerListMod.DisplayNameColorMode.None; EntryManager.RefreshPlayerEntries(true); }), "Set displayname coloring to none", "NoneButton");
        }

        public static void CreateGeneralInfoSubMenus()
        {
            // Create Toggle Button Submenus (done automatically to enable expandability)
            int totalMade = 0;
            for (int i = 0; i < Math.Ceiling(EntryManager.generalInfoEntries.Count / 9f); i++)
            {
                SubMenu subMenu = new SubMenu("UserInterface/QuickMenu", $"PlayerListMenuPage{i + 4}");

                for (; totalMade < (9 * (i + 1)) && totalMade < EntryManager.generalInfoEntries.Count; totalMade++)
                {
                    EntryBase entry = EntryManager.generalInfoEntries.Values.ElementAt(totalMade);
                    ToggleButton toggle = new ToggleButton(subMenu.path, new Vector3((totalMade % 3) + 1, (float)Math.Floor((totalMade - (9 * i)) / 3f)), $"Enable {entry.Name}", $"Disabled", new Action<bool>((state) => { entry.gameObject.SetActive(state); entry.prefEntry.Value = state; EntryManager.shouldSaveConfig = true; }), $"Toggle the {entry.Name} entry", $"Toggle the {entry.Name} entry", $"{entry.Name.Replace(" ", "")}EntryToggle", entry.prefEntry.Value, true);
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
                    new SingleButton(playerListMenus[i].path, new Vector3(4, 1), $"Save", new Action(EntryManager.SaveEntries), $"Saves all settings if you have made changes, this is also done automatically when you close the menu", "SaveEntriesButton");
                }
                if (i + 1 < playerListMenus.Count)
                    new SingleButton(playerListMenus[i].path, "UserInterface/QuickMenu/EmojiMenu/PageDown", new Vector3(4, 2), $"Page {i + 2}", new Action(() => UIManager.OpenPage($"UserInterface/QuickMenu/PlayerListMenuPage{k + 2}")), $"Go to page {i + 2}", "ForwardPageButton");

                if (i == 0) continue; // Skip main config menu

                EnableDisableListener subMenuListener = playerListMenus[i].gameObject.AddComponent<EnableDisableListener>();
                subMenuListener.OnEnableEvent += new Action(() =>
                {
                    PlayerListMod.playerList.SetActive(!shouldStayHidden);
                    PlayerListMod.playerListRect.anchoredPosition = Converters.ConvertToUnityUnits(new Vector3(6.5f, 3.5f));
                });
                subMenuListener.OnDisableEvent += new Action(() =>
                {
                    PlayerListMod.playerList.SetActive(false);
                    PlayerListMod.playerListRect.anchoredPosition = Config.PlayerListPosition;
                    PlayerListMod.playerListRect.localPosition = new Vector3(PlayerListMod.playerListRect.localPosition.x, PlayerListMod.playerListRect.localPosition.y, 25);
                });
            }
        }
    }
}
