using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        public static bool worldAllowed = false;

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

        public delegate void UpdateEntryDelegate(PlayerNet playerNet, PlayerEntry entry, ref string tempString);
        public static UpdateEntryDelegate updateDelegate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr OnPlayerNetDecode(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer);
        private static OnPlayerNetDecode originalDecodeDelegate = null;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int OnVRCPlayerDataReceived(IntPtr instancePointer, IntPtr hashtablePointer, IntPtr nativeMethodPointer);
        private static OnVRCPlayerDataReceived originalDataReceiveDelegate = null;

        public static void EntryInit()
        {
            PlayerListConfig.OnConfigChangedEvent += OnStaticConfigChanged;
            UIManager.OnQuickMenuOpenEvent += () =>
            {
                foreach (PlayerEntry entry in EntryManager.playerEntries)
                    entry.GetPlayerColor();
            }; // OGT fork compatibility 
                    
            PlayerListMod.Instance.Harmony.Patch(typeof(APIUser).GetMethod("IsFriendsWith"), new HarmonyMethod(typeof(PlayerEntry).GetMethod(nameof(OnIsFriend), BindingFlags.NonPublic | BindingFlags.Static)));

            // Definitely not stolen code from our lord and savior knah (https://github.com/knah/VRCMods/blob/master/AdvancedSafety/AdvancedSafetyMod.cs) because im not a skid
            foreach (MethodInfo method in typeof(PlayerNet).GetMethods().Where(mi => mi.GetParameters().Length == 3 && mi.Name.StartsWith("Method_Public_Virtual_Final_New_Void_ValueTypePublic")))
            {
                unsafe
                {
                    var originalMethodPointer = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method).GetValue(null);

                    OnPlayerNetDecode replacement = (instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer) => OnPlayerNetDecodeDelegate(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer);

                    MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPointer), Marshal.GetFunctionPointerForDelegate(replacement));

                    originalDecodeDelegate = Marshal.GetDelegateForFunctionPointer<OnPlayerNetDecode>(originalMethodPointer);
                }
            }

            // Use native patch here as doing a harmony patch would mean I would have to use a named type. even if i got around that with some clever logic it would break player initialization somewhat
            foreach (MethodInfo method in typeof(VRCPlayer).GetMethods().Where(mi => mi.ReturnType.IsEnum && mi.GetParameters().Length == 1 && mi.GetParameters()[0].ParameterType == typeof(Il2CppSystem.Collections.Hashtable) && Xref.CheckMethod(mi, "Failed to read showSocialRank for {0}")))
            {
                unsafe
                {
                    var originalMethodPointer = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(method).GetValue(null);

                    OnVRCPlayerDataReceived replacement = (instancePointer, hashtablePointer, nativeMethodPointer) => OnVRCPlayerDataReceivedDelegate(instancePointer, hashtablePointer, nativeMethodPointer);

                    MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPointer), Marshal.GetFunctionPointerForDelegate(replacement));

                    originalDataReceiveDelegate = Marshal.GetDelegateForFunctionPointer<OnVRCPlayerDataReceived>(originalMethodPointer);
                }
            }
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
                updateDelegate += (PlayerNet playerNet, PlayerEntry entry, ref string tempString) => EntrySortManager.SortPlayer(entry);

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
            if (manager.field_Private_VRCPlayer_0.field_Private_Player_0.field_Private_APIUser_0.id != userId)
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
            apiUser = player.field_Private_APIUser_0;
            userId = apiUser.id;
            if (vrcPlayer.field_Private_Player_0.field_Private_APIUser_0?.id != userId)
                return;

            perf = vrcPlayer.prop_VRCAvatarManager_0.prop_AvatarPerformanceStats_0.field_Private_ArrayOf_PerformanceRating_0[(int)AvatarPerformanceCategory.Overall];
            perfString = "<color=#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, perf)) + ">" + PlayerUtils.ParsePerformanceText(perf) + "</color>";
            
            if (player.prop_PlayerNet_0 != null)
                UpdateEntry(player.prop_PlayerNet_0, this, true);
            
            if (EntrySortManager.IsSortTypeInUse(EntrySortManager.SortType.AvatarPerf))
                EntrySortManager.SortPlayer(this);
        }

        private static int OnVRCPlayerDataReceivedDelegate(IntPtr instancePointer, IntPtr hashtablePointer, IntPtr nativeMethodPointer)
        {
            if (instancePointer == IntPtr.Zero)
                return originalDataReceiveDelegate(instancePointer, hashtablePointer, nativeMethodPointer);

            VRCPlayer vrcPlayer = new Il2CppSystem.Object(instancePointer).TryCast<VRCPlayer>();
            if (vrcPlayer == null)
                return originalDataReceiveDelegate(instancePointer, hashtablePointer, nativeMethodPointer);

            int result = originalDataReceiveDelegate(instancePointer, hashtablePointer, nativeMethodPointer);


            if (result == 64)
            {
                EntryManager.GetEntryFromPlayer(EntryManager.sortedPlayerEntries, vrcPlayer.field_Private_Player_0, out PlayerEntry entry);
                if (entry == null)
                    return result;

                entry.GetPlayerColor();
            }

            return result;
        }
        private static IntPtr OnPlayerNetDecodeDelegate(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer)
        {
            if (instancePointer == IntPtr.Zero)
                return originalDecodeDelegate(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer);

            PlayerNet playerNet = new Il2CppSystem.Object(instancePointer).TryCast<PlayerNet>();
            if (playerNet == null)
                return originalDecodeDelegate(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer);

            IntPtr result = originalDecodeDelegate(instancePointer, objectsPointer, objectIndex, sendTime, nativeMethodPointer);
            if (result == IntPtr.Zero)
                return result;

            try
            {
                UpdateEntry(playerNet);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Something went horribly wrong:\n" + ex.ToString());
            }

            return result;
        }
        public static void UpdateEntry(PlayerNet playerNet, PlayerEntry entry = null, bool bypassActive = false)
        {
            if (entry == null)
            {
                EntryManager.GetEntryFromPlayer(EntryManager.sortedPlayerEntries, playerNet.prop_Player_0, out entry);
                if (entry == null)
                    return;
            }

            // Update values but not text even if playerlist not active
            entry.distance = (entry.player.transform.position - Player.prop_Player_0.transform.position).magnitude;
            entry.fps = MelonUtils.Clamp((int)(1000f / playerNet.field_Private_Byte_0), -99, 999);
            entry.ping = playerNet.prop_Int16_0;

            if (!(MenuManager.playerList.active || bypassActive))
                return;

            string tempString = "";

            updateDelegate?.Invoke(playerNet, entry, ref tempString);

            entry.textComponent.text = entry.TrimExtra(tempString);
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

        private static void AddPing(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            tempString += "<color=" + PlayerUtils.GetPingColor(entry.ping) + ">";
            if (entry.ping < 9999 && entry.ping > -999)
                tempString += entry.ping.ToString().PadRight(4) + "ms</color>";
            else
                tempString += ((double)(entry.ping / 1000)).ToString("N1").PadRight(5) + "s</color>";
            tempString += separator;
        }
        private static void AddFps(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            tempString += "<color=" + PlayerUtils.GetFpsColor(entry.fps) + ">";
            if (playerNet.field_Private_Byte_0 == 0)
                tempString += "?¿?</color>";
            else
                tempString += entry.fps.ToString().PadRight(3) + "</color>";
            tempString += separator;
        }
        private static void AddPlatform(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            tempString += entry.platform + separator;
        }
        private static void AddPerf(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            tempString += entry.perfString + separator;
        }
        private static void AddDistance(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            if (worldAllowed)
            {
                if (entry.distance < 100)
                {
                    tempString += entry.distance.ToString("N1").PadRight(4) + "m";
                }
                else if (entry.distance < 10000)
                {
                    tempString += (entry.distance / 1000).ToString("N1").PadRight(3) + "km";
                }
                else if (entry.distance < 999900)
                {
                    tempString += (entry.distance / 1000).ToString("N0").PadRight(3) + "km";
                }
                else
                {
                    tempString += (entry.distance / 9.461e+15).ToString("N2").PadRight(3) + "ly"; // If its too large for kilometers ***just convert to light years***
                }
            }
            else
            {
                entry.distance = 0;
                tempString += "0.0 m";
            }
            tempString += separator;
        }
        private static void AddPhotonId(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            tempString += entry.player.field_Internal_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0.ToString().PadRight(highestPhotonIdLength) + separator;
        }
        private static void AddDisplayName(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            tempString += "<color=" + entry.playerColor + ">" + entry.apiUser.displayName + "</color>" + separator;
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
