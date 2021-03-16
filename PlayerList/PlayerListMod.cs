using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

[assembly: MelonInfo(typeof(PlayerList.PlayerListMod), "PlayerList", "1.2.5", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace PlayerList
{
    public class PlayerListMod : MelonMod
    {
        public static List<SubMenu> playerListMenus = new List<SubMenu>();
        public static GameObject playerList;
        public static GameObject playerListMenuButton;
        public static RectTransform playerListRect;
        public static Dictionary<string, PlayerEntry> playerEntries = new Dictionary<string, PlayerEntry>();
        public static Dictionary<string, EntryBase> generalInfoEntries = new Dictionary<string, EntryBase>();
        public static Dictionary<int, EntryBase> entries = new Dictionary<int, EntryBase>();
        public static VerticalLayoutGroup playerListLayout;
        public static VerticalLayoutGroup generalInfoLayout;

        private static Vector2 quickMenuColliderSize;

        private static readonly Stopwatch timer = Stopwatch.StartNew();

        private static LocalPlayerEntry localPlayerEntry = null;

        private static Label snapToGridSizeLabel;
        private static int _snapToGridSize;
        public static int SnapToGridSize
        {
            get { return _snapToGridSize; }
            set
            {
                if (value <= 0) return;

                hasConfigChanged = true;
                Config.snapToGridSize.Value = value;
                snapToGridSizeLabel.textComponent.text = $"Snap Grid\nSize: {value}";
                _snapToGridSize = value;
            }
        }

        private static bool hasChangedTabs = false;
        private static bool shouldMove = false;
        private static bool hasConfigChanged = false;
        private static bool shouldStayHidden;
        private static Label fontSizeLabel;
        private static int _fontSize;
        public static int FontSize
        {
            get { return _fontSize; }
            set
            {
                hasConfigChanged = true;
                Config.fontSize.Value = value;
                _fontSize = value;
                fontSizeLabel.textComponent.text = $"Font Size: {value}";
                foreach (EntryBase entry in entries.Values)
                    entry.textComponent.fontSize = value;
                RefreshLayout();
            }
        }

        private static PlayerListButtonPosition _playerListMenuButtonPosition;
        public static PlayerListButtonPosition PlayerListMenuButtonPosition
        {
            get { return _playerListMenuButtonPosition; }
            set
            {
                if (value == Config.PlayerListMenuButtonPosition) return;

                switch (value)
                {
                    case PlayerListButtonPosition.TopLeft:
                        playerListMenuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(4, -1));
                        playerListMenuButton.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                        break;
                    case PlayerListButtonPosition.TopRight:
                        playerListMenuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(2, -1));
                        playerListMenuButton.GetComponent<RectTransform>().pivot = new Vector2(1, 0);
                        break;
                    case PlayerListButtonPosition.BottomLeft:
                        playerListMenuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(2, -1));
                        playerListMenuButton.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                        break;
                    case PlayerListButtonPosition.BottomRight:
                        playerListMenuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(4, -1));
                        playerListMenuButton.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                        break;
                    default:
                        PlayerListMenuButtonPosition = PlayerListButtonPosition.TopLeft;
                        return;
                }
                _playerListMenuButtonPosition = value;
                Config.PlayerListMenuButtonPosition = value;
                hasConfigChanged = true;
            }
        }
        public static ToggleButton menuToggleButton;

        public override void OnApplicationStart()
        {
        }
        public override void VRChat_OnUiManagerInit()
        {
            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            Config.RegisterSettings();

            // Initialize Constants util
            Constants.UIInit();

            // Initialize input manager
            InputManager.UiInit();

            quickMenuColliderSize = Constants.quickMenu.GetComponent<BoxCollider>().size;

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
                    playerListMenuButton = UnityEngine.Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/Prefabs/PlayerListMenuButton.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), Constants.shortcutMenu.transform);
                }
            }
            SetLayerRecursive(playerListMenuButton, 12);
            playerListMenuButton.GetComponent<Button>().onClick.AddListener(new Action(() => { UIManager.OpenPage(playerListMenus[0].path); playerList.SetActive(!shouldStayHidden); }));
            playerListMenuButton.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(4, -1));
            playerListMenuButton.GetComponent<RectTransform>().pivot = new Vector2(0, 0);

            UiTooltip tooltip = playerListMenuButton.AddComponent<UiTooltip>();
            tooltip.field_Public_String_0 = "Open PlayerList menu";
            tooltip.field_Public_String_1 = "Open PlayerList menu";

            SetLayerRecursive(playerList, 12);
            playerListRect = playerList.GetComponent<RectTransform>();
            playerListRect.anchoredPosition = Config.PlayerListPosition;
            CombineQMColliderAndPlayerListRect();
            playerListRect.localPosition = new Vector3(playerListRect.localPosition.x, playerListRect.localPosition.y, 25); // Do this or else it looks off for whatever reason
            playerList.SetActive(false);

            shouldStayHidden = !Config.enabledOnStart.Value;
            _fontSize = Config.fontSize.Value;
            _snapToGridSize = Config.snapToGridSize.Value;
            PlayerListMenuButtonPosition = Config.PlayerListMenuButtonPosition;

            EnableDisableListener playerListListener = playerList.AddComponent<EnableDisableListener>();
            playerListListener.OnEnableEvent += RefreshAllEntries;

            playerListLayout = playerList.transform.Find("PlayerList Viewport/PlayerList").GetComponent<VerticalLayoutGroup>();
            generalInfoLayout = playerList.transform.Find("GeneralInfo Viewport/GeneralInfo").GetComponent<VerticalLayoutGroup>();

            // Initialize UIManager
            UIManager.Init();
            UIManager.UIInit(Harmony);

            // Initialize submenu for the list 
            MelonLogger.Msg("Initializing Menu...");
            playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage1"));

            SingleButton backButton = new SingleButton(playerListMenus[0].path, new Vector3(5, 2), "Back", new Action(() => UIManager.OpenPage("UserInterface/QuickMenu/ShortcutMenu")), "Press to go back", "BackButton");
            backButton.textComponent.color = Color.yellow;

            menuToggleButton = new ToggleButton(playerListMenus[0].path, new Vector3(5, 0), "Enabled", "Disabled", new Action<bool>((state) => ToggleMenu()), "Toggle the menu. Can also be toggled using Left Ctrl + F1", "Toggle the menu. Can also be toggled using Left Ctrl + F1", "ToggleMenuToggle", !shouldStayHidden);

            new ToggleButton(playerListMenus[0].path, new Vector3(5, -1), "Enabled on Start", "Disabled", new Action<bool>((state) => { Config.enabledOnStart.Value = state; hasConfigChanged = true; }), "Toggle if the list is toggled hidden on start", "Toggle if the list is toggled hidden on start", "EnabledOnStartToggle", Config.enabledOnStart.Value, true);

            new ToggleButton(playerListMenus[0].path, new Vector3(0, 1), "Condense Text", "Regular Text", new Action<bool>((state) => { Config.condensedText.Value = !Config.condensedText.Value; RefreshPlayerEntries(); hasConfigChanged = true; }), "Toggle if text should be condensed", "Toggle if text should be condensed", "CondensedTextToggle", Config.condensedText.Value, true);
            new ToggleButton(playerListMenus[0].path, new Vector3(0, 0), "Numbered List", "Tick List", new Action<bool>((state) => { Config.numberedList.Value = !Config.numberedList.Value; RefreshPlayerEntries(); hasConfigChanged = true; }), "Toggle if the list should be numbered or ticked", "Toggle if the list should be numbered or ticked", "NumberedTickToggle", Config.numberedList.Value, true);

            // TODO: Add opacity options, maybe color too, (maybe even for each stage of ping and fps??)

            // Add listeners
            EnableDisableListener shortcutMenuListener = Constants.shortcutMenu.AddComponent<EnableDisableListener>();
            shortcutMenuListener.OnEnableEvent += new Action(() => { playerList.SetActive(!shouldStayHidden); UIManager.CurrentMenu = Constants.shortcutMenu; });
            shortcutMenuListener.OnDisableEvent += new Action(() => playerList.SetActive(false));
            // TODO: add listeners to tab buttons to close my menu or to make it so tabs are inaccesible when menu is open
            GameObject newElements = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements");
            GameObject Tabs = GameObject.Find("UserInterface/QuickMenu/QuickModeTabs");

            UIManager.OnQuickMenuCloseEvent += new Action(SaveEntries);

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
                playerListRect.anchoredPosition = Config.PlayerListPosition;
                playerListRect.localPosition = new Vector3(playerListRect.localPosition.x, playerListRect.localPosition.y, 25);
                Tabs.SetActive(true);
                newElements.SetActive(true);
            });

            // Initialize Movement menu
            playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage2"));

            new SingleButton(playerListMenus[1].path, new Vector3(3, 0), "Edit PlayerList Position", new Action(MovePlayerList), "Click to edit the position of the PlayerList", "EditPlayerListPosButton", true);

            new SingleButton(playerListMenus[1].path, new Vector3(3, 1), "Move to Right of QuickMenu", new Action(MovePlayerListToEndOfMenu), "Move PlayerList to right side of menu, this can also serve as a reset position button", "LockPlayerListToRightButton", true);

            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(0, 0), "1", new Action(() => PlayerListMenuButtonPosition = PlayerListButtonPosition.TopRight), "Move PlayerList menu button to the top right", "1PlayerListMenuButton");
            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(1, 0), "2", new Action(() => PlayerListMenuButtonPosition = PlayerListButtonPosition.TopLeft), "Move PlayerList menu button to the top left", "2PlayerListMenuButton");
            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(1, 1), "3", new Action(() => PlayerListMenuButtonPosition = PlayerListButtonPosition.BottomLeft), "Move PlayerList menu button to the bottom left", "3PlayerListMenuButton");
            new QuarterButton(playerListMenus[1].path, new Vector3(3, 2), new Vector2(0, 1), "4", new Action(() => PlayerListMenuButtonPosition = PlayerListButtonPosition.BottomRight), "Move PlayerList menu button to the bottom right", "4PlayerListMenuButton");

            new SingleButton(playerListMenus[1].path, new Vector3(1, 0), "Snap Grid\nSize +", new Action(() => SnapToGridSize += 10), "Increase the size of the snap to grid by 10", "IncreaseSnapGridSize", true);
            new SingleButton(playerListMenus[1].path, new Vector3(1, 2), "Snap Grid\nSize -", new Action(() => SnapToGridSize -= 10), "Decrease the size of the snap to grid by 10", "DecreaseSnapGridSize", true);
            new SingleButton(playerListMenus[1].path, new Vector3(0, 0), "Reset Snap\nGrid Size", new Action(() => SnapToGridSize = 420), "Set snap to grid to the default value (420)", "DefaultSnapGridSize", true);
            snapToGridSizeLabel = new Label(playerListMenus[1].path, new Vector3(1, 1), $"Snap Grid\nSize: {SnapToGridSize}", "SnapToGridSizeLabel", resize: true);

            new SingleButton(playerListMenus[1].path, new Vector3(2, 0), "Font\nSize +", new Action(() => FontSize++), "Increase font size of the list by 1", "IncreaseFontSizeButton", true);
            new SingleButton(playerListMenus[1].path, new Vector3(2, 2), "Font\nSize -", new Action(() => FontSize--), "Decrease font size of the list by 1", "DecreaseFontSizeButton", true);
            new SingleButton(playerListMenus[1].path, new Vector3(0, 1), "Reset\nFont", new Action(() => FontSize = 35), "Set font size to the default value (35)", "DefaultFontSizeButton", true);
            fontSizeLabel = new Label(playerListMenus[1].path, new Vector3(2, 1), $"Font\nSize: {FontSize}", "FontSizeLabel", resize: true);

            // Initialize PlayerList Customization menu
            playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage3"));

            new ToggleButton(playerListMenus[2].path, new Vector3(1, 0), "Enable Ping", "Disabled", new Action<bool>((state) => { Config.pingToggle.Value = state; hasConfigChanged = true; RefreshPlayerEntries(); }), "Toggle player ping", "Toggle player ping", "PingToggle", Config.pingToggle.Value, true);
            new ToggleButton(playerListMenus[2].path, new Vector3(2, 0), "Enable Fps", "Disabled", new Action<bool>((state) => { Config.fpsToggle.Value = state; hasConfigChanged = true; RefreshPlayerEntries(); }), "Toggle player fps", "Toggle player fps", "FpsToggle", Config.fpsToggle.Value, true);
            new ToggleButton(playerListMenus[2].path, new Vector3(3, 0), "Enable Platform", "Disabled", new Action<bool>((state) => { Config.platformToggle.Value = state; hasConfigChanged = true; RefreshPlayerEntries(); }), "Toggle player Platform", "Toggle player Platform", "PlatformToggle", Config.platformToggle.Value, true);
            new ToggleButton(playerListMenus[2].path, new Vector3(1, 1), "Enable Avatar Performance", "Disabled", new Action<bool>((state) => { Config.perfToggle.Value = state; hasConfigChanged = true; RefreshPlayerEntries(); }), "Toggle avatar performance", "Toggle avatar performance", "PerfToggle", Config.perfToggle.Value, true);
            new ToggleButton(playerListMenus[2].path, new Vector3(2, 1), "Enable DisplayName", "Disabled", new Action<bool>((state) => { Config.displayNameToggle.Value = state; hasConfigChanged = true; RefreshPlayerEntries(); }), "Why...?", "Why...?", "DisplayNameToggle", Config.displayNameToggle.Value, true);

            new QuarterButton(playerListMenus[2].path, new Vector3(3, 1), new Vector2(0, 0), "TF", new Action(() => { Config.DisplayNameColorMode = DisplayNameColorMode.TrustAndFriends; RefreshPlayerEntries(); hasConfigChanged = true; }), "Set displayname coloring to show friends and trust rank", "TrustAndFriendsButton");
            new QuarterButton(playerListMenus[2].path, new Vector3(3, 1), new Vector2(1, 0), "T", new Action(() => { Config.DisplayNameColorMode = DisplayNameColorMode.TrustOnly; RefreshPlayerEntries(); hasConfigChanged = true; }), "Set displayname coloring to show trust rank only", "TrustOnlyButton");
            new QuarterButton(playerListMenus[2].path, new Vector3(3, 1), new Vector2(1, 1), "F", new Action(() => { Config.DisplayNameColorMode = DisplayNameColorMode.FriendsOnly; RefreshPlayerEntries(); hasConfigChanged = true; }), "Set displayname coloring to show friends only", "FriendsOnlyButton");
            new QuarterButton(playerListMenus[2].path, new Vector3(3, 1), new Vector2(0, 1), "N", new Action(() => { Config.DisplayNameColorMode = DisplayNameColorMode.None; RefreshPlayerEntries(); hasConfigChanged = true; }), "Set displayname coloring to none", "NoneButton");

            // Add entries
            MelonLogger.Msg("Adding List Entries...");
            PlayerEntry.Patch(Harmony);

            AddGeneralInfoEntry(EntryBase.CreateInstance<PlayerListHeaderEntry>(playerListLayout.transform.Find("Header").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<RoomTimeEntry>(generalInfoLayout.transform.Find("RoomTime").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<SystemTime12HrEntry>(generalInfoLayout.transform.Find("SystemTime12Hr").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<SystemTime24HrEntry>(generalInfoLayout.transform.Find("SystemTime24Hr").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<GameVersionEntry>(generalInfoLayout.transform.Find("GameVersion").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<CoordinatePositionEntry>(generalInfoLayout.transform.Find("CoordinatePosition").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<WorldNameEntry>(generalInfoLayout.transform.Find("WorldName").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<WorldAuthorEntry>(generalInfoLayout.transform.Find("WorldAuthor").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<InstanceMasterEntry>(generalInfoLayout.transform.Find("InstanceMaster").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<InstanceCreatorEntry>(generalInfoLayout.transform.Find("InstanceCreator").gameObject, includeConfig: true));

            // Create Toggle Button Submenus (done automatically to enable expandability)
            int totalMade = 0;
            for (int i = 0; i < Math.Ceiling(generalInfoEntries.Count / 9f); i++)
            {
                SubMenu subMenu = new SubMenu("UserInterface/QuickMenu", $"PlayerListMenuPage{i + 4}");

                for (; totalMade < (9 * (i + 1)) && totalMade < generalInfoEntries.Count; totalMade++)
                {
                    EntryBase entry = generalInfoEntries.Values.ElementAt(totalMade);
                    ToggleButton toggle = new ToggleButton(subMenu.path, new Vector3((totalMade % 3) + 1, (float)Math.Floor((totalMade - (9 * i)) / 3f)), $"Enable {entry.Name}", $"Disabled", new Action<bool>((state) => { entry.gameObject.SetActive(state); entry.prefEntry.Value = state; hasConfigChanged = true; }), $"Toggle the {entry.Name} entry", $"Toggle the {entry.Name} entry", $"{entry.Name.Replace(" ", "")}EntryToggle", entry.prefEntry.Value, true);
                }

                playerListMenus.Add(subMenu);
            }

            for (int i = 0; i < playerListMenus.Count; i++)
            {
                int k = i; // dum reference stuff


                if (i > 0)
                {
                    new SingleButton(playerListMenus[i].path, "UserInterface/QuickMenu/EmojiMenu/PageUp", new Vector3(4, 0), $"Page {i}", new Action(() => UIManager.OpenPage($"UserInterface/QuickMenu/PlayerListMenuPage{k}")), $"Go back to page {i}", "BackPageButton");
                    new SingleButton(playerListMenus[i].path, new Vector3(4, 1), $"Save", new Action(SaveEntries), $"Saves all settings if you have made changes, this is also done automatically when you close the menu", "SaveEntriesButton");
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
                    playerListRect.anchoredPosition = Config.PlayerListPosition;
                    playerListRect.localPosition = new Vector3(playerListRect.localPosition.x, playerListRect.localPosition.y, 25);
                });
            }

            // Initialize on leave and join events
            NetworkHooks.NetworkInit();
            NetworkHooks.OnPlayerJoin += new Action<Player>((player) => OnPlayerJoin(player));
            NetworkHooks.OnPlayerLeave += new Action<Player>((player) => OnPlayerLeave(player));
            
            MelonLogger.Msg("Initialized!");
        }
        public override void OnUpdate()
        {
            if (timer.Elapsed.TotalSeconds > 1)
            {
                timer.Restart();
                RefreshAllEntries();
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.F1)) ToggleMenu();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex == -1 && !hasChangedTabs)
            {
                hasChangedTabs = true;
                foreach (var child in GameObject.Find("UserInterface/QuickMenu/QuickModeTabs").transform)
                {
                    try
                    {
                        Transform childTransform = child.Cast<Transform>();
                        if (childTransform.name == "HomeTab") continue; // Ignore home tab or else it go brrrr

                        childTransform.GetComponent<Button>().onClick.AddListener(new Action(() =>
                        {
                            foreach (SubMenu subMenu in playerListMenus)
                                subMenu.gameObject.SetActive(false);
                        }));
                    }
                    catch { }
                }
            }

            foreach (PlayerEntry playerEntry in playerEntries.Values.ToList())
                playerEntry.Remove();
        }
        public static void OnPlayerJoin(Player player)
        {
            if (playerEntries.ContainsKey(player.field_Private_APIUser_0.id)) return;

            if (player.field_Private_APIUser_0.IsSelf)
            {
                if (localPlayerEntry != null) return;
                
                localPlayerEntry = EntryBase.CreateInstance<LocalPlayerEntry>(UnityEngine.Object.Instantiate(playerListLayout.transform.Find("Template").gameObject, playerListLayout.transform));
                AddEntry(localPlayerEntry);
                localPlayerEntry.gameObject.SetActive(true);
                return;
            }

            AddPlayerEntry(EntryBase.CreateInstance<PlayerEntry>(UnityEngine.Object.Instantiate(playerListLayout.transform.Find("Template").gameObject, playerListLayout.transform), new object[] { player }));
        }
        public static void OnPlayerLeave(Player player)
        {
            if (!playerEntries.ContainsKey(player.field_Private_APIUser_0.id)) return;

            UnityEngine.Object.DestroyImmediate(playerEntries[player.field_Private_APIUser_0.id].gameObject);
            entries.Remove(playerEntries[player.field_Private_APIUser_0.id].Identifier);
            playerEntries.Remove(player.field_Private_APIUser_0.id);
            RefreshLayout();
        }

        public static void MovePlayerListToEndOfMenu()
        {
            RectTransform furthestTransform = new GameObject("temp", new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[] { Il2CppType.Of<RectTransform>() })).GetComponent<RectTransform>(); // Create new gameobject with recttransform on it
            foreach (var child in Constants.shortcutMenu.transform)
            {
                RectTransform childRect = child.Cast<RectTransform>();
                if (childRect.gameObject.activeSelf && childRect.anchoredPosition.x + childRect.rect.width > furthestTransform.anchoredPosition.x + furthestTransform.rect.width)
                    furthestTransform = childRect;
            }

            Config.PlayerListPosition = new Vector2(furthestTransform.anchoredPosition.x + (furthestTransform.rect.width / 2), playerListRect.anchoredPosition.y);
            CombineQMColliderAndPlayerListRect(useConfigValues: true);
        }
        public static void MovePlayerList()
        {
            playerListRect.anchoredPosition = Config.PlayerListPosition; // So old position var works properly
            shouldMove = true;
            MelonCoroutines.Start(WaitForPress(playerList, new Action<GameObject>((gameObject) => 
            { 
                Config.PlayerListPosition = playerListRect.anchoredPosition;
                gameObject.SetActive(!shouldStayHidden);
                UIManager.OpenPage(playerListMenus[1].path);
                playerListRect.localPosition = new Vector3(playerListRect.localPosition.x, playerListRect.localPosition.y, 25);
                hasConfigChanged = true;
            })));
            UIManager.OpenPage("UserInterface/QuickMenu/ShortcutMenu");
            playerList.SetActive(true);
        }
        private static System.Collections.IEnumerator WaitForPress(GameObject movingGameObject, Action<GameObject> onComplete = null)
        {
            RectTransform movingGameObjectRect = movingGameObject.GetComponent<RectTransform>();
            Vector3 oldPosition = movingGameObjectRect.anchoredPosition3D;

            while (InputManager.IsUseInputPressed && shouldMove)
            {
                CombineQMColliderAndPlayerListRect();
                movingGameObjectRect.transform.position = InputManager.HitPosition;
                movingGameObjectRect.transform.localPosition = new Vector3(movingGameObjectRect.transform.localPosition.x, movingGameObjectRect.transform.localPosition.y, oldPosition.z);
                movingGameObjectRect.anchoredPosition = Converters.RoundAmount(movingGameObjectRect.anchoredPosition, SnapToGridSize);

                yield return null;
            }

            while (!InputManager.IsUseInputPressed && shouldMove)
            {
                CombineQMColliderAndPlayerListRect();
                movingGameObjectRect.transform.position = InputManager.HitPosition;
                movingGameObjectRect.transform.localPosition = new Vector3(movingGameObjectRect.transform.localPosition.x, movingGameObjectRect.transform.localPosition.y, oldPosition.z);
                movingGameObjectRect.anchoredPosition = Converters.RoundAmount(movingGameObjectRect.anchoredPosition, SnapToGridSize);

                yield return null;
            }

            if (shouldMove)
            { 
                onComplete.Invoke(movingGameObject);
            }
            else
            { 
                movingGameObjectRect.anchoredPosition3D = oldPosition;
                CombineQMColliderAndPlayerListRect();
            }
            shouldMove = false;
        }
        public static void CombineQMColliderAndPlayerListRect(bool useConfigValues = false)
        {
            BoxCollider collider = Constants.quickMenu.GetComponent<BoxCollider>();
            float colliderLeft = -quickMenuColliderSize.x / 2;
            float colliderTop = quickMenuColliderSize.y / 2;
            float colliderRight = quickMenuColliderSize.x / 2;
            float colliderBottom = -quickMenuColliderSize.y / 2;

            float playerListLeft;
            float playerListTop;
            float playerListRight;
            float playerListBottom;
            if (!useConfigValues)
            {
                playerListLeft = playerListRect.anchoredPosition.x - playerListRect.sizeDelta.x / 2;
                playerListTop = playerListRect.anchoredPosition.y + (playerListRect.sizeDelta.y / 2);
                playerListRight = playerListRect.anchoredPosition.x + playerListRect.sizeDelta.x / 2;
                playerListBottom = playerListRect.anchoredPosition.y - (playerListRect.sizeDelta.y / 2);
            }
            else
            {
                playerListLeft = Config.PlayerListPosition.x - playerListRect.sizeDelta.x / 2;
                playerListTop = Config.PlayerListPosition.y + (playerListRect.sizeDelta.y / 2);
                playerListRight = Config.PlayerListPosition.x + playerListRect.sizeDelta.x / 2;
                playerListBottom = Config.PlayerListPosition.y - (playerListRect.sizeDelta.y / 2);
            }

            collider.size = new Vector2(Math.Abs(Math.Max(Math.Abs(Math.Min(colliderLeft, playerListLeft)), Math.Abs(Math.Max(colliderRight, playerListRight)))) * 2, Math.Abs(Math.Max(Math.Abs(Math.Min(colliderBottom, playerListBottom)), Math.Abs(Math.Max(colliderTop, playerListTop)))) * 2);
        }

        public static void SaveEntries()
        {
            if (hasConfigChanged)
            {
                MelonPreferences.Save();
                hasConfigChanged = false;
            }
            shouldMove = false;
        }
        public static void AddEntry(EntryBase entry)
        {
            entry.textComponent.fontSize = _fontSize;
            entries.Add(entry.Identifier, entry);
            RefreshLayout();
        }
        public static void AddPlayerEntry(PlayerEntry entry)
        {
            AddEntry(entry);
            playerEntries.Add(entry.player.field_Private_APIUser_0.id, entry);
            entry.gameObject.SetActive(true);
            entry.Refresh();
        }
        public static void AddGeneralInfoEntry(EntryBase entry)
        {
            AddEntry(entry);
            generalInfoEntries.Add(entry.Name, entry);
        }
        public static void RefreshPlayerEntries()
        {
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || Player.prop_Player_0 == null || Player.prop_Player_0.gameObject == null || !playerList.active) return;

            foreach (PlayerEntry entry in new List<EntryBase>(playerEntries.Values)) // Slow but allows me to remove things during its run
                if (entry.player == null)
                    entry.Remove();

            foreach (PlayerEntry entry in playerEntries.Values)
                entry.Refresh();

            localPlayerEntry.Refresh();

            RefreshLayout();
        }
        public static void RefreshAllEntries()
        {
            // Dont refresh if the local player gameobject has been deleted or if the playerlist is hidden
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || Player.prop_Player_0 == null || Player.prop_Player_0.gameObject == null || !playerList.active) return;

            foreach (EntryBase entry in new List<EntryBase>(entries.Values))
                entry.Refresh();

            RefreshLayout();
        }
        public static void SetLayerRecursive(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (var child in gameObject.transform)
                SetLayerRecursive(child.Cast<Transform>().gameObject, layer);
        }

        public static void ToggleMenu()
        {
            if (!playerListMenus.Any(subMenu => subMenu.gameObject.active) && !Constants.shortcutMenu.active) return;
            shouldStayHidden = !shouldStayHidden;
            menuToggleButton.State = !shouldStayHidden;
            if (playerListMenus.Any(subMenu => subMenu.gameObject.active) || Constants.shortcutMenu.active) playerList.SetActive(!playerList.activeSelf);
        }
        public static void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerListLayout.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(generalInfoLayout.GetComponent<RectTransform>());
        }
        public enum PlayerListButtonPosition
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
