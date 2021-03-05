using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using PlayerList.Entries;
using PlayerList.UI;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using VRC;

[assembly: MelonInfo(typeof(PlayerList.PlayerListMod), "PlayerList", "1.1.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace PlayerList
{
    public class PlayerListMod : MelonMod
    {
        public static List<SubMenu> playerListMenus = new List<SubMenu>();
        public static GameObject shortcutMenu;
        public static GameObject playerList;
        public static Dictionary<string, PlayerEntry> playerEntries = new Dictionary<string, PlayerEntry>();
        public static Dictionary<string, EntryBase> generalInfoEntries = new Dictionary<string, EntryBase>();
        public static Dictionary<int, EntryBase> entries = new Dictionary<int, EntryBase>();
        public static VerticalLayoutGroup playerListLayout;
        public static VerticalLayoutGroup generalInfoLayout;

        private static Stopwatch timer = Stopwatch.StartNew();

        private static bool hasMadeLocalPlayer = false;
        
        private static bool shouldStayHidden;
        private static Label fontSizeLabel;
        private static int _fontSize;
        public static int FontSize
        {
            get { return _fontSize; }
            set
            {
                Config.fontSize.Value = value;
                _fontSize = value;
                fontSizeLabel.text.text = $"Font Size: {value}";
                foreach (EntryBase entry in entries.Values)
                    entry.textComponent.fontSize = value;
                RefreshLayout();
            }
        }
        public static ToggleButton menuToggleButton;

        public override void VRChat_OnUiManagerInit()
        {
            ClassInjector.RegisterTypeInIl2Cpp<Components.EnableDisableListener>();
            Config.RegisterSettings();

            // Stolen from UIExpansionKit (https://github.com/knah/VRCMods/blob/master/UIExpansionKit) #Imnotaskidiswear
            MelonLogger.Msg("Loading List UI...");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlayerList.playerlistmod.assetbundle"))
            {
                using (var memoryStream = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(memoryStream);
                    AssetBundle assetBundle = AssetBundle.LoadFromMemory_Internal(memoryStream.ToArray(), 0);
                    assetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    playerList = UnityEngine.Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/Prefabs/PlayerListMod.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), GameObject.Find("UserInterface/QuickMenu").transform);
                }
            }
            SetLayerRecursive(playerList, 12);
            playerList.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            playerList.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            playerList.GetComponent<RectTransform>().localPosition = new Vector2(2000, 1400);
            playerList.transform.parent.gameObject.GetComponent<BoxCollider>().size = new Vector2(6500, playerList.transform.parent.gameObject.GetComponent<BoxCollider>().size.y);

            shouldStayHidden = !Config.enabledOnStart.Value;
            _fontSize = Config.fontSize.Value;

            Components.EnableDisableListener playerListListener = playerList.AddComponent<Components.EnableDisableListener>();
            playerListListener.OnEnableEvent += RefreshLayout;

            playerListLayout = playerList.transform.Find("PlayerList Viewport/PlayerList").GetComponent<VerticalLayoutGroup>();
            generalInfoLayout = playerList.transform.Find("GeneralInfo Viewport/GeneralInfo").GetComponent<VerticalLayoutGroup>();

            // Initialize UIManager
            UIManager.Init();
            UIManager.UIInit();

            // Initialize submenu for the list 
            playerListMenus.Add(new SubMenu("UserInterface/QuickMenu", "PlayerListMenuPage1"));

            new SingleButton(playerListMenus[0].path, new Vector3(4, 2), $"Page 2", new Action(() => UIManager.OpenPage($"UserInterface/QuickMenu/PlayerListMenuPage2")), $"Go to page 2", "ForwardPageButton");

            Button openMenuButton = playerList.transform.Find("Menu Button").gameObject.GetComponent<Button>();
            openMenuButton.onClick.AddListener(new Action(() => { UIManager.OpenPage(playerListMenus[0].path); playerList.SetActive(!shouldStayHidden); }));

            SingleButton backButton = new SingleButton(playerListMenus[0].path, new Vector3(5, 2), "Back", new Action(() => UIManager.OpenPage("UserInterface/QuickMenu/ShortcutMenu")), "Press to go back", "BackButton");
            backButton.text.color = Color.yellow;

            menuToggleButton = new ToggleButton(playerListMenus[0].path, new Vector3(4, 0), "Enabled", "Disabled", new Action<bool>((state) => ToggleMenu()), "Toggle the menu. Can also be toggled using Left Ctrl + F1", "Toggle the menu. Can also be toggled using Left Ctrl + F1", "ToggleMenuToggle", !shouldStayHidden);

            ToggleButton enabledOnStartToggle = new ToggleButton(playerListMenus[0].path, new Vector3(3, 0), "Enabled on Start", "Disabled", new Action<bool>((state) => Config.enabledOnStart.Value = state), "Toggle if the list is toggled hidden on start", "Toggle if the list is toggled hidden on start", "EnabledOnStartToggle", Config.enabledOnStart.Value);
            enabledOnStartToggle.onStateOnText.resizeTextForBestFit = true;
            enabledOnStartToggle.offStateOnText.resizeTextForBestFit = true;

            new SingleButton(playerListMenus[0].path, new Vector3(1, 0), "Font Size +", new Action(() => FontSize++), "Increase font size of the list by 1", "DecreaseFontSizeButton");
            new SingleButton(playerListMenus[0].path, new Vector3(1, 2), "Font Size -", new Action(() => FontSize--), "Decrease font size of the list by 1", "IncreaseFontSizeButton");
            new SingleButton(playerListMenus[0].path, new Vector3(2, 1), "Reset Font", new Action(() => FontSize = 60), "Set font size to the default value (60)", "DefaultFontSizeButton");
            fontSizeLabel = new Label(playerListMenus[0].path, new Vector3(1, 1), $"Font Size: {FontSize}", "FontSizeLabel");

            shortcutMenu = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu");

            // Add listeners
            Components.EnableDisableListener shortcutMenuListener = shortcutMenu.AddComponent<Components.EnableDisableListener>();
            shortcutMenuListener.OnEnableEvent += new Action(() => playerList.SetActive(!shouldStayHidden));
            shortcutMenuListener.OnDisableEvent += new Action(() => playerList.SetActive(false));

            Components.EnableDisableListener playerListMenuListener = playerListMenus[0].gameObject.AddComponent<Components.EnableDisableListener>();
            playerListMenuListener.OnEnableEvent += new Action(() => playerList.SetActive(!shouldStayHidden));
            playerListMenuListener.OnDisableEvent += new Action(() => playerList.SetActive(false));

            // Add entries
            MelonLogger.Msg("Adding List Entries...");
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
                int k = i; // dum reference stuff
                SubMenu subMenu = new SubMenu("UserInterface/QuickMenu", $"PlayerListMenuPage{i + 2}");
                new SingleButton(subMenu.path, new Vector3(4, 0), $"Page {i + 1}", new Action(() => UIManager.OpenPage($"UserInterface/QuickMenu/PlayerListMenuPage{k + 1}")), $"Go back to page {i + 1}", "BackPageButton");
                if (i + 1 < Math.Ceiling(generalInfoEntries.Count / 9f))
                    new SingleButton(subMenu.path, new Vector3(4, 2), $"Page {i + 3}", new Action(() => UIManager.OpenPage($"UserInterface/QuickMenu/PlayerListMenuPage{k + 3}")), $"Go to page {i + 3}", "ForwardPageButton");

                Components.EnableDisableListener subMenuListener = subMenu.gameObject.AddComponent<Components.EnableDisableListener>();
                subMenuListener.OnEnableEvent += new Action(() => playerList.SetActive(!shouldStayHidden));
                subMenuListener.OnDisableEvent += new Action(() => playerList.SetActive(false));

                for (; totalMade < (9 * (i + 1)) && totalMade < generalInfoEntries.Count; totalMade++)
                {
                    EntryBase entry = generalInfoEntries.Values.ElementAt(totalMade);
                    ToggleButton toggle = new ToggleButton(subMenu.path, new Vector3((totalMade % 3) + 1, (float)Math.Floor((totalMade - (9 * i)) / 3f)), $"Enable {entry.Name}", $"Disabled", new Action<bool>((state) => { entry.gameObject.SetActive(state); entry.prefEntry.Value = state; }), $"Toggle the {entry.Name} entry", $"Toggle the {entry.Name} entry", $"{entry.Name.Replace(" ", "")}EntryToggle", entry.prefEntry.Value);
                    toggle.onStateOnText.resizeTextForBestFit = true;
                    toggle.offStateOnText.resizeTextForBestFit = true;
                }

                playerListMenus.Add(subMenu);
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
            foreach (KeyValuePair<string, PlayerEntry> playerEntry in playerEntries)
                UnityEngine.Object.DestroyImmediate(playerEntry.Value.gameObject);

            playerEntries.Clear();
        }
        public static void OnPlayerJoin(Player player)
        {
            if (playerEntries.ContainsKey(player.field_Private_APIUser_0.id)) return;

            if (player.field_Private_APIUser_0.IsSelf)
            {
                if (hasMadeLocalPlayer) return;
                hasMadeLocalPlayer = true;
                
                LocalPlayerEntry localPlayerEntry = EntryBase.CreateInstance<LocalPlayerEntry>(UnityEngine.Object.Instantiate(playerListLayout.transform.Find("Template").gameObject, playerListLayout.transform));
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
        }
        public static void AddGeneralInfoEntry(EntryBase entry)
        {
            AddEntry(entry);
            generalInfoEntries.Add(entry.Name, entry);
        }
        public static void RefreshAllEntries()
        {
            // Dont refresh if the local player gameobject has been deleted or if the playerlist is hidden
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || Player.prop_Player_0 == null || Player.prop_Player_0.gameObject == null || !playerList.active) return;

            foreach (EntryBase entry in new List<EntryBase>(entries.Values))
            {
                try
                {
                    if (entry.textComponent == null || entry.gameObject == null)
                    {
                        entries.Remove(entry.Identifier);
                        PlayerEntry playerEntry = entry as PlayerEntry;

                        if (playerEntry != null) playerEntries.Remove(playerEntry.player.field_Private_APIUser_0.id);
                        continue;
                    } // Don't refresh if gameobject is hidden
                    else
                        if (!entry.gameObject.active) continue;
                    entry.Refresh();
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Error while processing text of entry with text {entry.OriginalText}:\n" + ex.ToString());
                }
            }
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
            if (!playerListMenus.Any(subMenu => subMenu.gameObject.active) && !shortcutMenu.active) return;
            shouldStayHidden = !shouldStayHidden;
            menuToggleButton.State = !shouldStayHidden;
            if (playerListMenus.Any(subMenu => subMenu.gameObject.active) || shortcutMenu.active) playerList.SetActive(!playerList.activeSelf);
        }
        public static void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerListLayout.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(generalInfoLayout.GetComponent<RectTransform>());
        }
    }
}
