using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Dictionary<int, PlayerEntry> playerEntries = new Dictionary<int, PlayerEntry>(); // This will not be sorted
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
        }
        public static void OnUpdate()
        {
            RefreshAllEntries();
        }
        public static void OnSceneWasLoaded()
        {
            foreach (PlayerEntry playerEntry in playerEntries.Values.ToList())
                playerEntry.Remove();
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
        public static void OnFontSizeChange(int oldValue, int newValue)
        {
            SetFontSize(newValue);
        }

        public static void OnPlayerJoin(Player player)
        {
            if (playerEntries.ContainsKey(player.GetInstanceID())) return;

            if (player.field_Private_APIUser_0.IsSelf)
            {
                if (localPlayerEntry != null) return;

                localPlayerEntry = EntryBase.CreateInstance<LocalPlayerEntry>(UnityEngine.Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform));
                AddEntry(localPlayerEntry);
                playerEntriesWithLocal.Add(localPlayerEntry);
                localPlayerEntry.gameObject.SetActive(true);
                return;
            }

            AddPlayerEntry(EntryBase.CreateInstance<PlayerEntry>(UnityEngine.Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform), new object[] { player }));
        }
        public static void OnPlayerLeave(Player player)
        {
            if (!playerEntries.TryGetValue(player.GetInstanceID(), out PlayerEntry entry)) return;

            UnityEngine.Object.DestroyImmediate(entry.gameObject);
            entries.Remove(entry.Identifier);
            playerEntries.Remove(entry.playerInstanceId);
            for (int i = 0; i < playerEntriesWithLocal.Count; i++)
                if (playerEntriesWithLocal[i].playerInstanceId == entry.playerInstanceId)
                    playerEntriesWithLocal.RemoveAt(i);

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
        public static void AddEntry(EntryBase entry)
        {
            entry.textComponent.fontSize = PlayerListConfig.fontSize.Value;
            entries.Add(entry.Identifier, entry);
            RefreshLayout();
        }
        public static void AddPlayerEntry(PlayerEntry entry)
        {
            AddEntry(entry);
            playerEntries.Add(entry.playerInstanceId, entry);
            entry.gameObject.SetActive(true);
            RefreshPlayerEntries(true);
            EntrySortManager.SortPlayer(entry); // This function will handle adding the player to the sorted list
        }
        public static void AddGeneralInfoEntry(EntryBase entry)
        {
            AddEntry(entry);
            generalInfoEntries.Add(entry);
        }
        public static void RefreshPlayerEntries(bool bypassActive = false)
        {
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || Player.prop_Player_0 == null || Player.prop_Player_0.gameObject == null || Player.prop_Player_0.prop_VRCPlayerApi_0 == null || (!MenuManager.playerList.active && !bypassActive)) return;

            foreach (PlayerEntry entry in new List<EntryBase>(playerEntries.Values)) // Slow but allows me to remove things during its run
                if (entry.player == null)
                    entry.Remove();
            
            if (EntrySortManager.shouldSort)
                EntrySortManager.SortAllPlayers();
            EntrySortManager.shouldSort = false;

            foreach (PlayerEntry entry in playerEntries.Values)
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
