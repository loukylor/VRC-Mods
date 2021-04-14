using System.Collections.Generic;
using MelonLoader;
using PlayerList.Config;
using PlayerList.Entries;
using PlayerList.Utilities;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;

namespace PlayerList
{
    class EntryManager
    {
        internal static LocalPlayerEntry localPlayerEntry = null;

        public static List<PlayerEntry> playerEntries = new List<PlayerEntry>(); // This will not be sorted
        public static List<PlayerEntry> playerEntriesWithLocal = new List<PlayerEntry>(); // This will be sorted
        public static List<EntryBase> generalInfoEntries = new List<EntryBase>();
        public static Dictionary<int, EntryBase> entries = new Dictionary<int, EntryBase>();

        public static void Init()
        {
            PlayerListConfig.fontSize.OnValueChanged += OnFontSizeChange;
            PlayerListConfig.OnConfigChangedEvent += OnConfigChanged;
            NetworkEvents.OnPlayerJoin += OnPlayerJoin;
            NetworkEvents.OnPlayerLeave += OnPlayerLeave;
            NetworkEvents.OnInstanceChange += OnInstanceChange;
            NetworkEvents.OnAvatarChanged += OnAvatarChanged;
            NetworkEvents.OnAvatarInstantiated += OnAvatarInstantiated;
        }
        public static void OnUpdate()
        {
            RefreshAllEntries();
        }
        public static void OnSceneWasLoaded()
        {
            for (int i = playerEntries.Count - 1; i >= 0; i--)
                playerEntries[i].Remove();
            foreach (EntryBase entry in generalInfoEntries)
                entry.OnSceneWasLoaded();
        }
        public static void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            foreach (EntryBase entry in entries.Values)
                entry.OnInstanceChange(world, instance);
        }
        public static void OnConfigChanged()
        {
            foreach (EntryBase entry in entries.Values)
                entry.OnConfigChanged();
        }
        public static void OnAvatarChanged(ApiAvatar avatar, VRCAvatarManager manager)
        {
            foreach (EntryBase entry in entries.Values)
                entry.OnAvatarChanged(avatar, manager);
        }
        public static void OnAvatarInstantiated(VRCPlayer player, GameObject avatar)
        {
            foreach (EntryBase entry in entries.Values)
                entry.OnAvatarInstantiated(player, avatar);
        }
        public static void OnFontSizeChange(int oldValue, int newValue)
        {
            SetFontSize(newValue);
        }

        public static void OnPlayerJoin(Player player)
        {
            if (GetEntryFromPlayer(playerEntriesWithLocal, player, out _)) return; // If already in list
            if (player.field_Private_APIUser_0.IsSelf)
            {
                if (localPlayerEntry != null) return;

                EntryBase.CreateInstance<LocalPlayerEntry>(Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform));
                playerEntriesWithLocal.Add(localPlayerEntry);
                localPlayerEntry.CalculateLeftPart();
                AddEntry(localPlayerEntry);
                localPlayerEntry.gameObject.SetActive(true);
                return;
            }

            AddPlayerEntry(EntryBase.CreateInstance<PlayerEntry>(Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform), new object[] { player }));
        }
        public static void OnPlayerLeave(Player player)
        {
            if (!GetEntryFromPlayer(playerEntriesWithLocal, player, out PlayerEntry entry)) return;
            entry.Remove();
            RefreshLayout();
        }

        public static void AddGeneralInfoEntries()
        {
            MelonLogger.Msg("Adding List Entries...");
            AddGeneralInfoEntry(EntryBase.CreateInstance<PlayerListHeaderEntry>(Constants.playerListLayout.transform.Find("Header").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<RoomTimeEntry>(Constants.generalInfoLayout.transform.Find("RoomTime").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<SystemTime12HrEntry>(Constants.generalInfoLayout.transform.Find("SystemTime12Hr").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<SystemTime24HrEntry>(Constants.generalInfoLayout.transform.Find("SystemTime24Hr").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<GameVersionEntry>(Constants.generalInfoLayout.transform.Find("GameVersion").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<CoordinatePositionEntry>(Constants.generalInfoLayout.transform.Find("CoordinatePosition").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<WorldNameEntry>(Constants.generalInfoLayout.transform.Find("WorldName").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<WorldAuthorEntry>(Constants.generalInfoLayout.transform.Find("WorldAuthor").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<InstanceMasterEntry>(Constants.generalInfoLayout.transform.Find("InstanceMaster").gameObject, includeConfig: true));
            AddGeneralInfoEntry(EntryBase.CreateInstance<InstanceCreatorEntry>(Constants.generalInfoLayout.transform.Find("InstanceCreator").gameObject, includeConfig: true));
        }
        public static bool GetEntryFromPlayer(List<PlayerEntry> list, Player player, out PlayerEntry entry)
        {
            if (player == null)
            {
                entry = null;
                return false;
            }
            
            int playerInstanceId = player.GetInstanceID();
            foreach (PlayerEntry entryValue in list)
            {
                if (playerInstanceId == entryValue.playerInstanceId)
                { 
                    entry = entryValue;
                    return true;
                }
            }
            entry = null;
            return false;
        }
        public static void AddEntry(EntryBase entry)
        {
            entry.textComponent.fontSize = PlayerListConfig.fontSize.Value;
            entries.Add(entry.Identifier, entry);
            RefreshLayout();
        }
        public static void AddPlayerEntry(PlayerEntry entry)
        {
            AddEntry(entry);
            playerEntries.Add(entry);
            entry.gameObject.SetActive(true);
            EntrySortManager.SortPlayer(entry); 
        }
        public static void AddGeneralInfoEntry(EntryBase entry)
        {
            AddEntry(entry);
            generalInfoEntries.Add(entry);
        }
        public static void RefreshPlayerEntries(bool bypassActive = false)
        {
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || Player.prop_Player_0 == null || Player.prop_Player_0.gameObject == null || Player.prop_Player_0.prop_VRCPlayerApi_0 == null || (!MenuManager.playerList.active && !bypassActive)) return;

            foreach (PlayerEntry entry in playerEntries)
                if (entry.player == null)
                    entry.Remove();

            foreach (PlayerEntry entry in playerEntries)
                PlayerEntry.UpdateEntry(entry.player.prop_PlayerNet_0, entry, bypassActive);

            localPlayerEntry.Refresh();

            RefreshLayout();
        }
        public static void RefreshGeneralInfoEntries()
        {
            foreach (EntryBase entry in generalInfoEntries)
                entry.Refresh();
        }
        public static void RefreshAllEntries()
        {
            // Dont refresh if the local player gameobject has been deleted or if the playerlist is hidden
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || Player.prop_Player_0 == null || !MenuManager.playerList.active) return;

            localPlayerEntry?.Refresh();
            RefreshGeneralInfoEntries();
        }
        public static void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Constants.playerListLayout.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(Constants.generalInfoLayout.GetComponent<RectTransform>());
        }
        public static void SetFontSize(int fontSize)
        {
            MenuManager.fontSizeLabel.textComponent.text = $"Font\nSize: {fontSize}";
            foreach (EntryBase entry in entries.Values)
                entry.textComponent.fontSize = fontSize;
        }
    }
}
