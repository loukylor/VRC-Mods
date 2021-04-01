using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using PlayerList.Entries;
using PlayerList.Utilities;
using UnityEngine;
using UnityEngine.UI;
using VRC;

namespace PlayerList
{
    class EntryManager
    {
        public static bool shouldSaveConfig = false;

        private static LocalPlayerEntry localPlayerEntry = null;

        public static Dictionary<string, PlayerEntry> playerEntries = new Dictionary<string, PlayerEntry>();
        public static Dictionary<string, EntryBase> generalInfoEntries = new Dictionary<string, EntryBase>();
        public static Dictionary<int, EntryBase> entries = new Dictionary<int, EntryBase>();
        
        public static void OnSceneWasLoaded()
        {
            foreach (PlayerEntry playerEntry in playerEntries.Values.ToList())
                playerEntry.Remove();
            foreach (EntryBase entry in generalInfoEntries.Values)
                entry.OnSceneWasLoaded();
        }
        public static void OnPlayerJoin(Player player)
        {
            if (playerEntries.ContainsKey(player.field_Private_APIUser_0.id)) return;

            if (player.field_Private_APIUser_0.IsSelf)
            {
                if (localPlayerEntry != null) return;

                localPlayerEntry = EntryBase.CreateInstance<LocalPlayerEntry>(Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform));
                AddEntry(localPlayerEntry);
                localPlayerEntry.gameObject.SetActive(true);
                return;
            }

            AddPlayerEntry(EntryBase.CreateInstance<PlayerEntry>(Object.Instantiate(Constants.playerListLayout.transform.Find("Template").gameObject, Constants.playerListLayout.transform), new object[] { player }));
        }
        public static void OnPlayerLeave(Player player)
        {
            if (!playerEntries.ContainsKey(player.field_Private_APIUser_0.id)) return;

            Object.DestroyImmediate(playerEntries[player.field_Private_APIUser_0.id].gameObject);
            entries.Remove(playerEntries[player.field_Private_APIUser_0.id].Identifier);
            playerEntries.Remove(player.field_Private_APIUser_0.id);
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
        public static void SaveEntries()
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 == null) return;

            if (shouldSaveConfig)
            {
                MelonPreferences.Save();
                shouldSaveConfig = false;
            }
            ListPositionManager.shouldMove = false;
        }
        public static void AddEntry(EntryBase entry)
        {
            entry.textComponent.fontSize = PlayerListMod.FontSize;
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
        public static void RefreshPlayerEntries(bool changeConfig = false)
        {
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || Player.prop_Player_0 == null || Player.prop_Player_0.gameObject == null || !PlayerListMod.playerList.active) return;

            foreach (PlayerEntry entry in new List<EntryBase>(playerEntries.Values)) // Slow but allows me to remove things during its run
                if (entry.player == null)
                    entry.Remove();

            foreach (PlayerEntry entry in playerEntries.Values)
                entry.Refresh();

            localPlayerEntry.Refresh();

            RefreshLayout();
            shouldSaveConfig = changeConfig;
        }
        public static void RefreshAllEntries()
        {
            // Dont refresh if the local player gameobject has been deleted or if the playerlist is hidden
            if (RoomManager.field_Internal_Static_ApiWorld_0 == null || Player.prop_Player_0 == null || Player.prop_Player_0.gameObject == null || !PlayerListMod.playerList.active) return;

            foreach (EntryBase entry in new List<EntryBase>(entries.Values))
                entry.Refresh();

            RefreshLayout();
        }
        public static void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Constants.playerListLayout.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(Constants.generalInfoLayout.GetComponent<RectTransform>());
        }
    }
}
