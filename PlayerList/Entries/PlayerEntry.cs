using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MelonLoader;
using Photon.Pun;
using Photon.Realtime;
using PlayerList.Config;
using PlayerList.Utilities;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;
using VRCSDK2.Validation.Performance;

using Player = VRC.Player;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class PlayerEntry : EntryBase
    {
        public PlayerEntry(IntPtr obj0) : base(obj0) { }

        // - <color={pingcolor}>{ping}ms</color> | <color={fpscolor}>{fps}</color> | {platform} | <color={perfcolor}>{perf}</color> | {relationship} | <color={rankcolor}>{displayname}</color>
        [HideFromIl2Cpp]
        public override string Name => "Player";

        public bool isSelf = false;

        public PlayerLeftPairEntry playerLeftPairEntry;

        public Player player;
        public APIUser apiUser;
        public string userId;
        protected string platform;

        public static string separator = " | ";

        private static bool spoofFriend;
        protected static int highestPhotonIdLength = 0;
        protected static int highestOwnedObjectsLength = 0;

        public PerformanceRating perf;
        public string perfString;
        public int ping;
        public int fps;
        public float distance;
        public bool isFriend;
        public string playerColor;
        public int ownedObjects = 0;

        public readonly Stopwatch timeSinceLastUpdate = Stopwatch.StartNew();

        public delegate void UpdateEntryDelegate(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString);
        public static UpdateEntryDelegate updateDelegate;

        public static void EntryInit()
        {
            UiManager.OnQuickMenuOpened += new Action(() =>
            {
                foreach (PlayerLeftPairEntry entry in EntryManager.playerLeftPairsEntries)
                    entry.playerEntry.GetPlayerColor(false);
                EntrySortManager.SortAllPlayers();
                EntryManager.RefreshPlayerEntries();
            });

            PlayerListConfig.OnConfigChanged += OnStaticConfigChanged;
            NetworkEvents.OnFriended += OnFriended;
            NetworkEvents.OnUnfriended += OnUnfriended;
            NetworkEvents.OnSetupFlagsReceived += OnSetupFlagsReceived;

            PlayerListMod.Instance.HarmonyInstance.Patch(typeof(PhotonView).GetMethods().First(mb => mb.Name.StartsWith("Method_FamOrAssem_set_Void_Int32_")), new HarmonyMethod(typeof(PlayerEntry).GetMethod(nameof(OnOwnerShipTransferred), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.HarmonyInstance.Patch(typeof(APIUser).GetMethod("IsFriendsWith"), new HarmonyMethod(typeof(PlayerEntry).GetMethod(nameof(OnIsFriend), BindingFlags.NonPublic | BindingFlags.Static)));        
        }

        [HideFromIl2Cpp]
        public override void Init(object[] parameters)
        {
            player = (Player)parameters[0];
            apiUser = player.prop_APIUser_0;
            userId = apiUser.id;
            
            platform = platform = PlayerUtils.GetPlatform(player).PadRight(2);
            perf = PerformanceRating.None;
            perfString = "<color=#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, perf)) + ">" + PlayerUtils.ParsePerformanceText(perf) + "</color>";

            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new Action(() => UiManager.OpenUserInQuickMenu(player)));

            isFriend = APIUser.IsFriendsWith(apiUser.id);
            GetPlayerColor();
            if (player.prop_PlayerNet_0 != null)
                UpdateEntry(player.prop_PlayerNet_0, this, true);
        }
        public static void OnStaticConfigChanged()
        {
            updateDelegate = null;
            if (PlayerListConfig.pingToggle.Value)
                updateDelegate += AddPing;
            if (PlayerListConfig.fpsToggle.Value)
                updateDelegate += AddFps;
            if (PlayerListConfig.platformToggle.Value)
                updateDelegate += AddPlatform;
            if (PlayerListConfig.perfToggle.Value)
                updateDelegate += AddPerf;
            if (PlayerListConfig.distanceToggle.Value)
                updateDelegate += AddDistance;
            if (PlayerListConfig.photonIdToggle.Value)
                updateDelegate += AddPhotonId;
            if (PlayerListConfig.ownedObjectsToggle.Value)
                updateDelegate += AddOwnedObjects;
            if (PlayerListConfig.displayNameToggle.Value)
                updateDelegate += AddDisplayName;
            if (PlayerListConfig.distanceToggle.Value && EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.Distance) || PlayerListConfig.pingToggle.Value && EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.Ping))
                updateDelegate += (PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString) => EntrySortManager.SortPlayer(entry.playerLeftPairEntry);

            if (PlayerListConfig.condensedText.Value)
                separator = "|";
            else
                separator = " | ";

            EntryManager.RefreshPlayerEntries();
        }
        public override void OnConfigChanged()
        {
            GetPlayerColor(false);
            if (EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.Distance))
                EntrySortManager.SortPlayer(playerLeftPairEntry);
        }
        public override void OnAvatarInstantiated(VRCAvatarManager manager, ApiAvatar avatar, GameObject gameObject)
        {
            apiUser = player.prop_APIUser_0;
            userId = apiUser.id;
            if (manager.field_Private_VRCPlayer_0.prop_Player_0.prop_APIUser_0?.id != userId)
                return;

            perf = manager.prop_AvatarPerformanceStats_0.field_Private_ArrayOf_PerformanceRating_0[(int)AvatarPerformanceCategory.Overall];
            perfString = "<color=#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, perf)) + ">" + PlayerUtils.ParsePerformanceText(perf) + "</color>";
            
            if (player.prop_PlayerNet_0 != null)
                UpdateEntry(player.prop_PlayerNet_0, this, true);
            
            if (EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.AvatarPerf))
                EntrySortManager.SortPlayer(playerLeftPairEntry);
        }
        [HideFromIl2Cpp]
        public override void OnAvatarDownloadProgressed(AvatarLoadingBar loadingBar, float downloadPercentage, long fileSize)
        {
            if (loadingBar.field_Public_PlayerNameplate_0.field_Private_VRCPlayer_0.prop_Player_0.prop_APIUser_0?.id != userId)
                return;

            perf = PerformanceRating.None;
            if (downloadPercentage < 1)
                perfString = ((downloadPercentage * 100).ToString("N1") + "%").PadRight(5);
            else
                perfString = "100% ";
        }

        // So apparently if you don't want to name an enum directly in a harmony patch you have to use int as the type... good to know
        private static void OnSetupFlagsReceived(VRCPlayer vrcPlayer, int SetupFlagType)
        {
            if (SetupFlagType == 64)
                EntryManager.idToEntryTable[vrcPlayer.prop_Player_0.prop_APIUser_0.id].playerEntry.GetPlayerColor();
        }
        public static void UpdateEntry(PlayerNet playerNet, PlayerEntry entry, bool bypassActive = false)
        {
            entry.timeSinceLastUpdate.Restart();

            // Update values but not text even if playerlist not active and before decode
            entry.distance = (entry.player.transform.position - Player.prop_Player_0.transform.position).magnitude;
            entry.fps = MelonUtils.Clamp((int)(1000f / playerNet.field_Private_Byte_0), -99, 999);
            entry.ping = playerNet.prop_Int16_0;

            if (!(MenuManager.playerList.active || bypassActive))
                return;

            StringBuilder tempString = new StringBuilder();

            updateDelegate?.Invoke(playerNet, entry, ref tempString);

            entry.textComponent.text = entry.TrimExtra(tempString.ToString());
        }
        private static bool OnIsFriend(ref bool __result)
        {
            if (spoofFriend)
            {
                __result = false;
                spoofFriend = false;
                return false;
            }
            return true;
        }

        protected static void AddPing(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString)
        {
            tempString.Append("<color=" + PlayerUtils.GetPingColor(entry.ping) + ">");
            if (entry.ping < 9999 && entry.ping > -999)
                tempString.Append(entry.ping.ToString().PadRight(4) + "ms</color>");
            else
                tempString.Append(((double)(entry.ping / 1000)).ToString("N1").PadRight(5) + "s</color>");
            tempString.Append(separator);
        }
        protected static void AddFps(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString)
        {
            tempString.Append("<color=" + PlayerUtils.GetFpsColor(entry.fps) + ">");
            if (entry.fps == 0)
                tempString.Append("?¿?</color>");
            else
                tempString.Append(entry.fps.ToString().PadRight(3) + "</color>");
            tempString.Append(separator);
        }
        private static void AddPlatform(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString)
        {
            tempString.Append(entry.platform + separator);
        }
        private static void AddPerf(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString)
        {
            tempString.Append(entry.perfString + separator);
        }
        private static void AddDistance(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString)
        {
            if (VRCUtils.AreRiskyFunctionsAllowed)
            {
                if (entry.distance < 100)
                {
                    tempString.Append(entry.distance.ToString("N1").PadRight(4) + "m");
                }
                else if (entry.distance < 10000)
                {
                    tempString.Append((entry.distance / 1000).ToString("N1").PadRight(3) + "km");
                }
                else if (entry.distance < 999900)
                {
                    tempString.Append((entry.distance / 1000).ToString("N0").PadRight(3) + "km");
                }
                else
                {
                    tempString.Append((entry.distance / 9.461e+15).ToString("N2").PadRight(3) + "ly"); // If its too large for kilometers ***just convert to light years***
                }
            }
            else
            {
                entry.distance = 0;
                tempString.Append("0.0 m");
            }
            tempString.Append(separator);
        }
        private static void AddPhotonId(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString)
        {
            tempString.Append(playerNet.prop_PhotonView_0.field_Private_Int32_0.ToString().PadRight(highestPhotonIdLength) + separator);
        }
        private static void AddOwnedObjects(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString)
        {
            tempString.Append(entry.ownedObjects.ToString().PadRight(highestOwnedObjectsLength) + separator);
        }
        private static void AddDisplayName(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString)
        {
            tempString.Append("<color=" + entry.playerColor + ">" + entry.apiUser.displayName + "</color>" + separator);
        }

        private static void OnFriended(APIUser user)
        {
            foreach (PlayerEntry entry in EntryManager.playerEntries)
                if (entry.userId == user.id)
                    entry.isFriend = true;
        }
        private static void OnUnfriended(string userId)
        {
            foreach (PlayerEntry entry in EntryManager.playerEntries)
                if (entry.userId == userId)
                    entry.isFriend = false;
        }

        private static void OnOwnerShipTransferred(PhotonView __instance, int __0)
        {
            if (__instance.GetComponent<VRC_Pickup>() == null)
                return;

            try
            {
                Room room = Player.prop_Player_0?.prop_PlayerNet_0?.field_Private_PhotonView_0?.prop_Player_0?.field_Private_Room_0;
                if (room == null)
                    return;

                // something is up with the  photon player constructor that makes me have to not use trygetvalue
                string oldOwner = null;
                if (room.field_Private_Dictionary_2_Int32_Player_0.ContainsKey(__instance.field_Private_Int32_0))
                    oldOwner = room.field_Private_Dictionary_2_Int32_Player_0[__instance.field_Private_Int32_0].field_Public_Player_0?.prop_APIUser_0?.id;
                string newOwner = null;
                if (room.field_Private_Dictionary_2_Int32_Player_0.ContainsKey(__0))
                    newOwner = room.field_Private_Dictionary_2_Int32_Player_0[__0].field_Public_Player_0?.prop_APIUser_0?.id;
                
                int highestOwnedObjects = 0;
                foreach (PlayerLeftPairEntry entry in EntryManager.playerLeftPairsEntries)
                {
                    if (entry.playerEntry.userId == oldOwner)
                    {
                        if (entry.playerEntry.ownedObjects > 0)
                            entry.playerEntry.ownedObjects -= 1;
                    }
                    else if (entry.playerEntry.userId == newOwner)
                    {
                        entry.playerEntry.ownedObjects += 1;
                    }

                    if (entry.playerEntry.ownedObjects > highestOwnedObjects)
                        highestOwnedObjects = entry.playerEntry.ownedObjects;
                }

                highestOwnedObjectsLength = highestOwnedObjects.ToString().Length;
            }
            catch (Exception ex)
            {
                MelonLogger.Error(ex.ToString());
            }
        }

        [HideFromIl2Cpp]
        private void GetPlayerColor(bool shouldSort = true)
        {
            playerColor = "";
            switch (PlayerListConfig.displayNameColorMode.Value)
            {
                case DisplayNameColorMode.TrustAndFriends:
                    playerColor = "#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(apiUser));
                    break;
                case DisplayNameColorMode.None:
                    break;
                case DisplayNameColorMode.TrustOnly:
                    // ty bono for this (https://github.com/ddakebono/)
                    spoofFriend = true;
                    playerColor = "#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(apiUser));
                    break;
                case DisplayNameColorMode.FriendsOnly:
                    if (APIUser.IsFriendsWith(apiUser.id))
                        playerColor = "#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(apiUser));
                    break;
            }
            if (EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.NameColor) && shouldSort)
                EntrySortManager.SortPlayer(playerLeftPairEntry);
        }
        [HideFromIl2Cpp]
        protected string TrimExtra(string tempString)
        {
            if (tempString.Length > 0)
                if (PlayerListConfig.condensedText.Value)
                    tempString = tempString.Remove(tempString.Length - 1, 1);
                else
                    tempString = tempString.Remove(tempString.Length - 3, 3);

            return tempString;
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
