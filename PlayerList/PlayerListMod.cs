using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MelonLoader;
using PlayerList.Entries;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using VRC;

[assembly: MelonInfo(typeof(PlayerList.PlayerListMod), "PlayerList", "1.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace PlayerList
{
    public class PlayerListMod : MelonMod
    {
        public static GameObject playerList;
        public static Dictionary<string, PlayerEntry> playerEntries = new Dictionary<string, PlayerEntry>();
        public static Dictionary<int, EntryBase> entries = new Dictionary<int, EntryBase>();
        public static VerticalLayoutGroup playerListLayout;
        public static VerticalLayoutGroup generalInfoLayout;

        public static Stopwatch timer = Stopwatch.StartNew();

        public static bool hasMadeLocalPlayer = false;

        public static bool shouldUpdate = true;
        public override void VRChat_OnUiManagerInit()
        {
            ClassInjector.RegisterTypeInIl2Cpp<Components.PlayerListComponent>();

            // Stolen from UIExpansionKit (https://github.com/knah/VRCMods/blob/master/UIExpansionKit) #Imnotaskidiswear
            MelonLogger.Msg("Loading List UI...");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlayerList.playerlistmod.assetbundle"))
            {
                using (var memoryStream = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(memoryStream);
                    AssetBundle assetBundle = AssetBundle.LoadFromMemory_Internal(memoryStream.ToArray(), 0);
                    assetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    playerList = UnityEngine.Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/Prefabs/PlayerListMod.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), GameObject.Find("UserInterface/QuickMenu/ShortcutMenu").transform);
                }
            }
            SetLayerRecursive(playerList, 12);
            playerList.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            playerList.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            playerList.GetComponent<RectTransform>().localPosition = new Vector2(2000, 1700);
            playerList.transform.parent.parent.gameObject.GetComponent<BoxCollider>().size = new Vector2(6500, playerList.transform.parent.parent.gameObject.GetComponent<BoxCollider>().size.y);
            
            playerList.AddComponent<Components.PlayerListComponent>();

            playerListLayout = playerList.transform.Find("PlayerList Viewport/PlayerList").GetComponent<VerticalLayoutGroup>();
            generalInfoLayout = playerList.transform.Find("GeneralInfo Viewport/GeneralInfo").GetComponent<VerticalLayoutGroup>();

            // Refresh the menu so it doesn't start "collapsed"
            playerList.transform.parent.gameObject.SetActive(true);
            playerList.transform.parent.gameObject.SetActive(false);

            // Add entries
            MelonLogger.Msg("Adding List Entries...");
            AddEntry(EntryBase.CreateInstance<PlayerListHeaderEntry>(playerListLayout.transform.Find("Header").gameObject));
            AddEntry(EntryBase.CreateInstance<RoomTimeEntry>(generalInfoLayout.transform.Find("RoomTime").gameObject));
            AddEntry(EntryBase.CreateInstance<SystemTime12HrEntry>(generalInfoLayout.transform.Find("SystemTime12Hr").gameObject));
            AddEntry(EntryBase.CreateInstance<SystemTime24HrEntry>(generalInfoLayout.transform.Find("SystemTime24Hr").gameObject));
            AddEntry(EntryBase.CreateInstance<GameVersionEntry>(generalInfoLayout.transform.Find("GameVersion").gameObject));
            AddEntry(EntryBase.CreateInstance<CoordinatePositionEntry>(generalInfoLayout.transform.Find("CoordinatePosition").gameObject));
            AddEntry(EntryBase.CreateInstance<WorldNameEntry>(generalInfoLayout.transform.Find("WorldName").gameObject));
            AddEntry(EntryBase.CreateInstance<WorldAuthorEntry>(generalInfoLayout.transform.Find("WorldAuthor").gameObject));
            AddEntry(EntryBase.CreateInstance<InstanceMasterEntry>(generalInfoLayout.transform.Find("InstanceMaster").gameObject));
            AddEntry(EntryBase.CreateInstance<InstanceCreatorEntry>(generalInfoLayout.transform.Find("InstanceCreator").gameObject));

            NetworkHooks.NetworkInit();
            NetworkHooks.OnPlayerJoin += new Action<Player>((player) => OnPlayerJoin(player));
            NetworkHooks.OnPlayerLeave += new Action<Player>((player) => OnPlayerLeave(player));
            MelonLogger.Msg("Initialized!");
        }
        public override void OnUpdate()
        {
            if (timer.Elapsed.TotalSeconds > 1 && RoomManager.field_Internal_Static_ApiWorld_0 != null && Player.prop_Player_0 != null)
            {
                timer.Restart();
                RefreshAllEntries();
            }
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
                
                LocalPlayerEntry localPlayerEntry = (LocalPlayerEntry)EntryBase.CreateInstance<LocalPlayerEntry>(UnityEngine.Object.Instantiate(playerListLayout.transform.Find("Template").gameObject, playerListLayout.transform));
                localPlayerEntry.gameObject.SetActive(true);

                entries.Add(localPlayerEntry.Identifier, localPlayerEntry);
                return;
            }

            PlayerEntry playerEntry = (PlayerEntry)EntryBase.CreateInstance<PlayerEntry>(UnityEngine.Object.Instantiate(playerListLayout.transform.Find("Template").gameObject, playerListLayout.transform), new object[] { player });
            playerEntry.gameObject.SetActive(true);

            entries.Add(playerEntry.Identifier, playerEntry);
            playerEntries.Add(player.field_Private_APIUser_0.id, playerEntry);
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
            entries.Add(entry.Identifier, entry);
            RefreshLayout();
        }
        public static void RefreshAllEntries()
        {
            if (Player.prop_Player_0.gameObject == null) return;

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
                    }
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
            gameObject.layer = 12;
            foreach (var child in gameObject.transform)
                SetLayerRecursive(child.Cast<Transform>().gameObject, layer);
        }
        public static void RefreshLayout()
        {
            // Doing this should refresh the menu layout
            playerListLayout.enabled = false;
            playerListLayout.enabled = true;

            generalInfoLayout.enabled = false;
            generalInfoLayout.enabled = true;
        }
    }
}
