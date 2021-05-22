using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Harmony;
using MelonLoader;
using PlayerList.Config;
using PlayerList.UI;
using PlayerList.Utilities;
using UnhollowerBaseLib;
using UnityEngine;
using VRC;
using VRC.Core;
using VRCSDK2.Validation.Performance;

namespace PlayerList.Entries
{
    public class PlayerEntry : EntryBase
    {
        // - <color={pingcolor}>{ping}ms</color> | <color={fpscolor}>{fps}</color> | {platform} | <color={perfcolor}>{perf}</color> | {relationship} | <color={rankcolor}>{displayname}</color>
        public override string Name { get { return "Player"; } }

        public bool isSelf = false;

        public static event Action OnWorldAllowedChanged;
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

        public Player player;
        public APIUser apiUser;
        public string userId;
        protected string platform;

        public static string separator = " | ";

        private static bool spoofFriend;
        protected static int highestPhotonIdLength = 0;

        public PerformanceRating perf;
        public string perfString;
        public int ping;
        public int fps;
        public float distance;
        public bool isFriend;
        public string playerColor;
        public bool isFrozen;

        public readonly Stopwatch timeSinceLastUpdate = Stopwatch.StartNew();

        public delegate void UpdateEntryDelegate(PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString);
        public static UpdateEntryDelegate updateDelegate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr OnPlayerNetDecodeDelegate(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer);
        private static readonly List<OnPlayerNetDecodeDelegate> dontGarbageCollectDelegates = new List<OnPlayerNetDecodeDelegate>();

        public static void EntryInit()
        {
            UIManager.OnQuickMenuOpenEvent += new Action(() =>
            {
                foreach (PlayerEntry entry in EntryManager.sortedPlayerEntries)
                    entry.GetPlayerColor();
                EntrySortManager.SortAllPlayers();
                EntryManager.RefreshPlayerEntries();
            });

            PlayerListConfig.OnConfigChanged += OnStaticConfigChanged;
            NetworkEvents.OnFriended += OnFriended;
            NetworkEvents.OnUnFriended += OnUnfriended;
            NetworkEvents.OnSetupFlagsReceived += OnSetupFlagsReceived;

            PlayerListMod.Instance.Harmony.Patch(typeof(APIUser).GetMethod("IsFriendsWith"), new HarmonyMethod(typeof(PlayerEntry).GetMethod(nameof(OnIsFriend), BindingFlags.NonPublic | BindingFlags.Static)));        
        }
        public override void Init(object[] parameters)
        {
            player = (Player)parameters[0];
            apiUser = player.field_Private_APIUser_0;
            userId = apiUser.id;
            
            platform = platform = PlayerUtils.GetPlatform(player).PadRight(2);
            perf = PerformanceRating.None;
            perfString = "<color=#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, perf)) + ">" + PlayerUtils.ParsePerformanceText(perf) + "</color>";

            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new Action(() => PlayerUtils.OpenPlayerInQuickMenu(player)));

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
            if (PlayerListConfig.displayNameToggle.Value)
                updateDelegate += AddDisplayName;
            if (PlayerListConfig.distanceToggle.Value && EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.Distance) || PlayerListConfig.pingToggle.Value && EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.Ping))
                updateDelegate += (PlayerNet playerNet, PlayerEntry entry, ref StringBuilder tempString) => EntrySortManager.SortPlayer(entry);

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
                EntrySortManager.SortPlayer(this);
        }
        public override void OnAvatarChanged(ApiAvatar avatar, VRCAvatarManager manager)
        {
            if (manager.field_Private_VRCPlayer_0.prop_Player_0.field_Private_APIUser_0.id != userId)
                return;
            
            perf = PerformanceRating.None;
            perfString = "<color=#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, perf)) + ">" + PlayerUtils.ParsePerformanceText(perf) + "</color>";

            if (player.prop_PlayerNet_0 != null)
                UpdateEntry(player.prop_PlayerNet_0, this, true);
            
            if (EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.AvatarPerf))
                EntrySortManager.SortPlayer(this);
        }
        public override void OnAvatarInstantiated(VRCPlayer vrcPlayer, GameObject avatar)
        {
            apiUser = player.prop_APIUser_0;
            userId = apiUser.id;
            if (vrcPlayer.prop_Player_0.field_Private_APIUser_0?.id != userId)
                return;

            perf = vrcPlayer.prop_VRCAvatarManager_0.prop_AvatarPerformanceStats_0.field_Private_ArrayOf_PerformanceRating_0[(int)AvatarPerformanceCategory.Overall];
            perfString = "<color=#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, perf)) + ">" + PlayerUtils.ParsePerformanceText(perf) + "</color>";
            
            if (player.prop_PlayerNet_0 != null)
                UpdateEntry(player.prop_PlayerNet_0, this, true);
            
            if (EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.AvatarPerf))
                EntrySortManager.SortPlayer(this);
        }
        public override void OnAvatarDownloadProgressed(AvatarLoadingBar loadingBar, float downloadPercentage, long fileSize)
        {
            if (loadingBar.field_Public_PlayerNameplate_0.field_Private_VRCPlayer_0.prop_Player_0.prop_APIUser_0?.id != userId)
                return;

            if (downloadPercentage < 1)
                perfString = ((downloadPercentage * 100).ToString("N1") + "%").PadRight(5);
            else
                perfString = "100% ";
        }

        // So apparently if you don't want to name an enum directly in a harmony patch you have to use int as the type... good to know
        private static void OnSetupFlagsReceived(VRCPlayer vrcPlayer, int SetupFlagType)
        {
            if (SetupFlagType == 64)
            {
                EntryManager.GetEntryFromPlayer(EntryManager.sortedPlayerEntries, vrcPlayer.prop_Player_0, out PlayerEntry entry);
                entry.GetPlayerColor();
            }
        }
        public static void UpdateEntry(PlayerNet playerNet, PlayerEntry entry, bool bypassActive = false)
        {
            entry.timeSinceLastUpdate.Restart();
            entry.isFrozen = false;

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
            if (WorldAllowed)
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
            tempString.Append(entry.player.prop_VRCPlayer_0.prop_PhotonView_0.field_Private_Int32_0.ToString().PadRight(highestPhotonIdLength) + separator);
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

        private void GetPlayerColor(bool shouldSort = true)
        {
            playerColor = "";
            switch (PlayerListConfig.DisplayNameColorMode)
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
                EntrySortManager.SortPlayer(this);
        }
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
