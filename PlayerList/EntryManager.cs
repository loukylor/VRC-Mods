using System;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using PlayerList.Config;
using PlayerList.Entries;
using PlayerList.Utilities;
using UnityEngine;
using VRC;
using VRC.Core;
using VRChatUtilityKit.Utilities;

using Object = UnityEngine.Object;

namespace PlayerList
{
    public class EntryManager
    {
        internal static LocalPlayerEntry localPlayerEntry = null;

        public static List<PlayerEntry> playerEntries = new List<PlayerEntry>(); // This will not be sorted
        public static List<PlayerLeftPairEntry> playerLeftPairsEntries = new List<PlayerLeftPairEntry>();
        public static Dictionary<string, PlayerLeftPairEntry> idToEntryTable = new Dictionary<string, PlayerLeftPairEntry>();
        public static List<EntryBase> generalInfoEntries = new List<EntryBase>();
        public static List<EntryBase> entries = new List<EntryBase>();

        public static void Init()
        {
            PlayerListConfig.fontSize.OnValueChanged += (oldValue, newValue) => SetFontSize(newValue);
            PlayerListConfig.OnConfigChanged += OnConfigChanged;
            NetworkEvents.OnPlayerJoined += OnPlayerJoined;
            NetworkEvents.OnPlayerLeft += OnPlayerLeft;
            NetworkEvents.OnInstanceChanged += OnInstanceChanged;
            NetworkEvents.OnAvatarInstantiated += OnAvatarInstantiated;
            NetworkEvents.OnAvatarDownloadProgressed += OnAvatarDownloadProgressed;

            MelonCoroutines.Start(EntryRefreshEnumerator());
        }
        private static IEnumerator EntryRefreshEnumerator()
        {
            while (playerEntries.Count == 0)
                yield return null;

            int i = -1;
            while (true)
            {
                i += 1;
                if (i >= playerEntries.Count)
                {
                    i = 0;
                    if (playerEntries.Count == 0)
                    {
                        yield return null;
                        continue;
                    }
                }

                try
                {
                    if (playerEntries[i].player == null)
                    {
                        playerEntries[i].playerLeftPairEntry.Remove();
                        continue;
                    }

                    if (playerEntries[i].timeSinceLastUpdate.ElapsedMilliseconds > 100)
                        PlayerEntry.UpdateEntry(playerEntries[i].player.prop_PlayerNet_0, playerEntries[i]);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error(ex.ToString());
                }

                yield return null;
            }
        }
        public static void OnUpdate()
        {
            RefreshAllEntries();
        }
        public static void OnSceneWasLoaded()
        {
            for (int i = playerEntries.Count - 1; i >= 0; i--)
                playerEntries[i].playerLeftPairEntry.Remove();
        }
        public static void OnInstanceChanged(ApiWorld world, ApiWorldInstance instance)
        {
            foreach (EntryBase entry in entries)
                entry.OnInstanceChange(world, instance);
            RefreshLeftPlayerEntries(0, 0, true);
        }
        public static void OnConfigChanged()
        {
            foreach (EntryBase entry in entries)
                entry.OnConfigChanged();
        }
        public static void OnAvatarInstantiated(VRCAvatarManager player, ApiAvatar avatar, GameObject gameObject)
        {
            foreach (EntryBase entry in playerEntries)
                entry.OnAvatarInstantiated(player, avatar, gameObject);
            localPlayerEntry?.OnAvatarInstantiated(player, avatar, gameObject);
        }
        public static void OnAvatarDownloadProgressed(AvatarLoadingBar loadingBar, float downloadPercent, long fileSize)
        {
            foreach (EntryBase entry in playerEntries)
                entry.OnAvatarDownloadProgressed(loadingBar, downloadPercent, fileSize);
            localPlayerEntry?.OnAvatarDownloadProgressed(loadingBar, downloadPercent, fileSize);
        }

        public static void OnPlayerJoined(Player player)
        {
            if (player.prop_APIUser_0 == null)
                return;

            if (idToEntryTable.ContainsKey(player.prop_APIUser_0.id))
                return; // If already in list

            if (player.prop_APIUser_0.IsSelf)
            {
                if (localPlayerEntry != null)
                    return;

                GameObject template = Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform);
                template.SetActive(true);

                LeftSidePlayerEntry leftSidePlayerEntry = EntryBase.CreateInstance<LeftSidePlayerEntry>(template.transform.Find("LeftPart").gameObject);
                EntryBase.CreateInstance<LocalPlayerEntry>(template.transform.Find("RightPart").gameObject);
                AddPlayerLeftPairEntry(EntryBase.CreateInstance<PlayerLeftPairEntry>(template, new object[] { leftSidePlayerEntry, localPlayerEntry }));
            }
            else
            {
                GameObject template = Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform);
                template.SetActive(true);

                LeftSidePlayerEntry leftSidePlayerEntry = EntryBase.CreateInstance<LeftSidePlayerEntry>(template.transform.Find("LeftPart").gameObject);
                PlayerEntry playerEntry = EntryBase.CreateInstance<PlayerEntry>(template.transform.Find("RightPart").gameObject, new object[] { player });
                AddPlayerLeftPairEntry(EntryBase.CreateInstance<PlayerLeftPairEntry>(template, new object[] { leftSidePlayerEntry, playerEntry }));
            }
        }
        public static void OnPlayerLeft(Player player)
        {
            if (player.prop_APIUser_0.IsSelf)
                return;

            if (!idToEntryTable.TryGetValue(player.prop_APIUser_0.id, out PlayerLeftPairEntry entry))
                return;

            entry.Remove();

            RefreshLeftPlayerEntries(0, 0, true);
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
            AddGeneralInfoEntry(EntryBase.CreateInstance<RiskyFuncAllowedEntry>(Constants.generalInfoLayout.transform.Find("RiskyFuncAllowed").gameObject, includeConfig: true));
        }
        public static void AddEntry(EntryBase entry)
        {
            if (entry.textComponent != null)
                entry.textComponent.fontSize = PlayerListConfig.fontSize.Value;
            entries.Add(entry);
        }
        public static void AddPlayerLeftPairEntry(PlayerLeftPairEntry entry)
        {
            playerLeftPairsEntries.Add(entry);
            idToEntryTable.Add(entry.playerEntry.userId, entry);
            if (!entry.playerEntry.isSelf)
                playerEntries.Add(entry.playerEntry);
            AddEntry(entry);
            AddEntry(entry.leftSidePlayerEntry);
            AddEntry(entry.playerEntry);

            entry.playerEntry.gameObject.SetActive(true);
            EntrySortManager.SortPlayer(entry);

            RefreshLeftPlayerEntries(0, 0, true);
        }
        public static void AddGeneralInfoEntry(EntryBase entry)
        {
            AddEntry(entry);
            generalInfoEntries.Add(entry);
        }
        public static void RefreshLeftPlayerEntries(int oldCount, int newCount, bool bypassCount = false)
        {
            // If new digit reached (like 9 - 10)
            if (oldCount.ToString().Length != newCount.ToString().Length || bypassCount)
                foreach (PlayerLeftPairEntry playerLeftPairEntry in playerLeftPairsEntries)
                    playerLeftPairEntry.leftSidePlayerEntry.CalculateLeftPart();
        }
        public static void RefreshPlayerEntries(bool bypassActive = false)
        {
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || Player.prop_Player_0 == null || Player.prop_Player_0.gameObject == null || Player.prop_Player_0.prop_VRCPlayerApi_0 == null || (!MenuManager.playerList.active && !bypassActive)) return;

            foreach (PlayerEntry entry in playerEntries)
                PlayerEntry.UpdateEntry(entry.player.prop_PlayerNet_0, entry, bypassActive);
            localPlayerEntry.Refresh();
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

        public static void SetFontSize(int fontSize)
        {
            MenuManager.fontSizeLabel.TextComponent.text = $"Font\nSize: {fontSize}";
            foreach (EntryBase entry in entries)
                if (entry.textComponent != null)
                    entry.textComponent.fontSize = fontSize;
        }
    }
}
