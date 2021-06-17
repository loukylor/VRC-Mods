using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using PlayerList.Config;
using PlayerList.Entries;
using PlayerList.Utilities;
using UnityEngine;
using VRC;
using VRC.Core;

namespace PlayerList
{
    class EntryManager
    {
        internal static LocalPlayerEntry localPlayerEntry = null;

        public static List<PlayerEntry> playerEntries = new List<PlayerEntry>(); // This will not be sorted
        public static List<PlayerEntry> sortedPlayerEntries = new List<PlayerEntry>(); // This will be sorted
        public static List<LeftSidePlayerEntry> leftSidePlayerEntries = new List<LeftSidePlayerEntry>();
        public static List<EntryBase> generalInfoEntries = new List<EntryBase>();
        public static List<EntryBase> entries = new List<EntryBase>();

        public static event System.Action OnWorldAllowedChanged;
        private static bool _worldAllowed = false;
        public static bool WorldAllowed
        {
            get { return _worldAllowed; }
            set
            {
                _worldAllowed = value;
                OnWorldAllowedChanged?.SafeInvoke();
            }
        }

        private static string localUserId;

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
                        Object.DestroyImmediate(playerEntries[i].gameObject.transform.parent.gameObject);
                        RemovePlayerEntry(playerEntries[i]);
                        RemoveLeftPlayerEntry(leftSidePlayerEntries[i + 1]); // Skip first entry
                        continue;
                    }

                    if (playerEntries[i].timeSinceLastUpdate.ElapsedMilliseconds > 100)
                        PlayerEntry.UpdateEntry(playerEntries[i].player.prop_PlayerNet_0, playerEntries[i]);
                }
                catch (System.Exception ex)
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
            {
                Object.DestroyImmediate(playerEntries[i].gameObject.transform.parent.gameObject);   
                RemovePlayerEntry(playerEntries[i]);
                RemoveLeftPlayerEntry(leftSidePlayerEntries[i + 1]); // Skip first entry
            }
        }
        public static void OnInstanceChanged(ApiWorld world, ApiWorldInstance instance)
        {
            WorldAllowed = false;
            if (world != null)
                MelonCoroutines.Start(VRCUtils.CheckWorld(world));

            foreach (EntryBase entry in entries)
                entry.OnInstanceChange(world, instance);
            RefreshLeftPlayerEntries(0, 0, true);
        }
        public static void OnConfigChanged()
        {
            foreach (EntryBase entry in entries)
                entry.OnConfigChanged();
        }
        public static void OnAvatarInstantiated(VRCAvatarManager player, GameObject avatar)
        {
            foreach (EntryBase entry in playerEntries)
                entry.OnAvatarInstantiated(player, avatar);
            localPlayerEntry?.OnAvatarInstantiated(player, avatar);
        }
        public static void OnAvatarDownloadProgressed(AvatarLoadingBar loadingBar, float downloadPercent, long fileSize)
        {
            foreach (EntryBase entry in playerEntries)
                entry.OnAvatarDownloadProgressed(loadingBar, downloadPercent, fileSize);
            localPlayerEntry?.OnAvatarDownloadProgressed(loadingBar, downloadPercent, fileSize);
        }

        public static void OnPlayerJoined(Player player)
        {
            if (GetEntryFromPlayer(sortedPlayerEntries, player, out _)) return; // If already in list

            GameObject template;

            if (player.prop_APIUser_0 == null || player.prop_APIUser_0.IsSelf) // The apiuser in player will only be null on the first join of the first instance of the client, and only occasionally. So it can be garunteed to be local player
            {
                if (localPlayerEntry != null) return;

                localUserId = APIUser.CurrentUser.id;

                template = Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform);
                template.SetActive(true);
                
                EntryBase.CreateInstance<LocalPlayerEntry>(template.transform.Find("RightPart").gameObject);
                sortedPlayerEntries.Add(localPlayerEntry);
                AddEntry(localPlayerEntry);

                AddLeftSidePlayerEntry(EntryBase.CreateInstance<LeftSidePlayerEntry>(template.transform.Find("LeftPart").gameObject));

                return;
            }

            template = Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform);
            template.SetActive(true);

            AddPlayerEntry(EntryBase.CreateInstance<PlayerEntry>(template.transform.Find("RightPart").gameObject, new object[] { player }));
            AddLeftSidePlayerEntry(EntryBase.CreateInstance<LeftSidePlayerEntry>(template.transform.Find("LeftPart").gameObject));
        }
        public static void OnPlayerLeft(Player player)
        {
            if (player.prop_APIUser_0.id == localUserId) return;

            int index = GetEntryFromPlayerWithIndex(sortedPlayerEntries, player, out PlayerEntry entry);
            if (index < 0) return;

            RemovePlayerEntry(entry);
            RemoveLeftPlayerEntry(leftSidePlayerEntries[index]);
            Object.DestroyImmediate(Constants.playerListLayout.transform.GetChild(index + 2).gameObject);

            RefreshLeftPlayerEntries(leftSidePlayerEntries.Count + 1, leftSidePlayerEntries.Count, true);
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
        public static int GetEntryFromPlayerWithIndex(List<PlayerEntry> list, Player player, out PlayerEntry entry)
        {
            if (player == null || player.prop_APIUser_0 == null)
            {
                entry = null;
                return -1;
            }

            string playerId = player.prop_APIUser_0.id;
            for (int i = 0; i < list.Count; i++)
            {
                if (playerId == list[i].userId)
                {
                    entry = list[i];
                    return i;
                }
            }
            entry = null;
            return -1;
        }
        public static bool GetEntryFromPlayer(List<PlayerEntry> list, Player player, out PlayerEntry entry)
        {
            return GetEntryFromPlayerWithIndex(list, player, out entry) >= 0;
        }
        public static void AddEntry(EntryBase entry)
        {
            entry.textComponent.fontSize = PlayerListConfig.fontSize.Value;
            entries.Add(entry);
        }
        public static void AddLeftSidePlayerEntry(LeftSidePlayerEntry entry)
        {
            AddEntry(entry);
            leftSidePlayerEntries.Add(entry);

            RefreshLeftPlayerEntries(leftSidePlayerEntries.Count - 1, leftSidePlayerEntries.Count);
        }
        public static void AddPlayerEntry(PlayerEntry entry)
        {
            AddEntry(entry);
            playerEntries.Add(entry);
            entry.gameObject.SetActive(true);
            sortedPlayerEntries.Add(entry);
            EntrySortManager.SortPlayer(entry); 
        }
        public static void AddGeneralInfoEntry(EntryBase entry)
        {
            AddEntry(entry);
            generalInfoEntries.Add(entry);
        }

        public static void RemoveEntry(EntryBase entry)
        {
            entries.Remove(entry);
            entry.Remove();
        }
        public static void RemoveLeftPlayerEntry(LeftSidePlayerEntry entry)
        {
            RemoveEntry(entry);
            leftSidePlayerEntries.Remove(entry);
        }
        public static void RemovePlayerEntry(PlayerEntry entry)
        {
            sortedPlayerEntries.Remove(entry);
            playerEntries.Remove(entry);
            RemoveEntry(entry);
        }

        public static void RefreshLeftPlayerEntries(int oldCount, int newCount, bool bypassCount = false)
        {
            // If new digit reached (like 9 - 10)
            if (oldCount.ToString().Length != newCount.ToString().Length || bypassCount)
                foreach (LeftSidePlayerEntry updateEntry in leftSidePlayerEntries)
                    updateEntry.CalculateLeftPart();
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
            MenuManager.fontSizeLabel.textComponent.text = $"Font\nSize: {fontSize}";
            foreach (EntryBase entry in entries)
                entry.textComponent.fontSize = fontSize;
        }
    }
}
