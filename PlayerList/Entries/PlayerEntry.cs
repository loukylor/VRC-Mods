using Harmony;
using MelonLoader;
using PlayerList.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

        public static bool worldAllowed = false;

        public int playerInstanceId;

        public Player player;
        public string userID;
        protected string playerColor;
        protected string platform;

        public static string separator = " | ";
        public static string leftPart = " - ";

        private static bool spoofFriend;
        protected static int highestPhotonIdLength = 0;

        public PerformanceRating lastPerf;

        public delegate void UpdateEntryDelegate(PlayerNet playerNet, PlayerEntry entry, ref string tempString);
        public static UpdateEntryDelegate updateDelegate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr OnPlayerNetDecode(IntPtr instancePointer, IntPtr objectsPointer, int objectIndex, float sendTime, IntPtr nativeMethodPointer);
        private static OnPlayerNetDecode originalDecodeDelegate = null;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int OnVRCPlayerDataReceived(IntPtr instancePointer, IntPtr hashtablePointer, IntPtr nativeMethodPointer);
        private static OnVRCPlayerDataReceived originalDataReceiveDelegate = null;
        private static Type vrcPlayerEnum;

        public static void EntryInit()
        {
            Config.OnConfigChangedEvent += OnStaticConfigChanged;
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
                vrcPlayerEnum = method.ReturnType;
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
            playerInstanceId = player.GetInstanceID();
            userID = player.field_Private_APIUser_0.id;
            platform = platform = GetPlatform(player).PadRight(2);

            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new Action(() => OpenPlayerInQuickMenu(player)));

            textComponent.text = "Loading...";

            GetPlayerColor();
            OnConfigChanged();
        }
        public static void OnStaticConfigChanged()
        {
            updateDelegate = null;
            if (Config.pingToggle.Value)
                updateDelegate += AddPing;
            if (Config.fpsToggle.Value)
                updateDelegate += AddFps;
            if (Config.platformToggle.Value)
                updateDelegate += AddPlatform;
            if (Config.perfToggle.Value)
                updateDelegate += AddPerf;
            if (Config.distanceToggle.Value)
                updateDelegate += AddDistance;
            if (Config.photonIdToggle.Value)
                updateDelegate += AddPhotonId;
            if (Config.displayNameToggle.Value)
                updateDelegate += AddDisplayName;

            if (Config.condensedText.Value)
                separator = "|";
            else
                separator = " | ";

            EntryManager.RefreshPlayerEntries();
        }
        public override void OnConfigChanged()
        {
            GetPlayerColor();
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
                EntryManager.playerEntries.TryGetValue(vrcPlayer.field_Private_Player_0.GetInstanceID(), out PlayerEntry entry);
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
        public static void UpdateEntry(PlayerNet playerNet, PlayerEntry entry = null)
        {
            if (!MenuManager.playerList.active)
                return;

            if (entry == null)
            {
                EntryManager.playerEntries.TryGetValue(playerNet.field_Internal_VRCPlayer_0.field_Private_Player_0.GetInstanceID(), out entry);
                if (entry == null)
                    return;
            }

            string tempString = "";

            updateDelegate?.Invoke(playerNet, entry, ref tempString);

            tempString = entry.AddLeftPart(tempString);
            entry.textComponent.text = tempString;
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
        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            MelonLogger.Msg("Checking if world is allowed to show distance...");
            worldAllowed = false;
            if (world != null)
                MelonCoroutines.Start(VRCUtils.CheckWorld(world));
        }

        private static void AddPing(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            short ping = playerNet.prop_Int16_0;

            tempString += "<color=" + GetPingColor(ping) + ">";
            if (ping < 9999 && ping > -999)
                tempString += ping.ToString().PadRight(4) + "ms</color>";
            else
                tempString += ((double)(ping / 1000)).ToString("N1").PadRight(5) + "s</color>";
            tempString += separator;
        }
        private static void AddFps(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            int fps = MelonUtils.Clamp((int)(1000f / playerNet.field_Private_Byte_0), -99, 999);

            tempString += "<color=" + GetFpsColor(fps) + ">";
            if (playerNet.field_Private_Byte_0 == 0)
                tempString += "?¿?</color>";
            else
                tempString += fps.ToString().PadRight(3) + "</color>";
            tempString += separator;
        }
        private static void AddPlatform(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            tempString += entry.platform + separator;
        }
        private static void AddPerf(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            PerformanceRating rating = entry.player.field_Internal_VRCPlayer_0.prop_VRCAvatarManager_0.prop_AvatarPerformanceStats_0.field_Private_ArrayOf_PerformanceRating_0[(int)AvatarPerformanceCategory.Overall]; // Get from cache so it doesnt calculate perf all at once
            if (rating != entry.lastPerf)
                EntryManager.shouldSort = true;
            entry.lastPerf = rating;
            tempString += "<color=#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, rating)) + ">" + ParsePerformanceText(rating) + "</color>" + separator;
        }
        private static void AddDistance(PlayerNet playerNet, PlayerEntry entry, ref string tempString)
        {
            if (worldAllowed)
            {
                float distance = (entry.player.transform.position - Player.prop_Player_0.transform.position).magnitude;
                if (distance < 100)
                {
                    tempString += distance.ToString("N1").PadRight(4) + "m";
                }
                else if (distance < 10000)
                {
                    tempString += (distance / 1000).ToString("N1").PadRight(3) + "km";
                }
                else if (distance < 999900)
                {
                    tempString += (distance / 1000).ToString("N0").PadRight(3) + "km";
                }
                else
                {
                    tempString += (distance / 9.461e+15).ToString("N2").PadRight(3) + "ly"; // If its too large for kilometers ***just convert to light years***
                }
            }
            else
            {
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
            tempString += "<color=" + entry.playerColor + ">" + entry.player.field_Private_APIUser_0.displayName + "</color>" + separator;
        }

        public void Remove()
        {
            EntryManager.playerEntries.Remove(playerInstanceId);
            EntryManager.entries.Remove(Identifier);
            UnityEngine.Object.DestroyImmediate(gameObject);
            return;
        }
        private void GetPlayerColor()
        {
            playerColor = "";
            switch (Config.DisplayNameColorMode)
            {
                case DisplayNameColorMode.TrustAndFriends:
                    playerColor = "#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(player.field_Private_APIUser_0));
                    break;
                case DisplayNameColorMode.None:
                    break;
                case DisplayNameColorMode.TrustOnly:
                    // ty bono for this (https://github.com/ddakebono/)
                    spoofFriend = true;
                    playerColor = "#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(player.field_Private_APIUser_0));
                    break;
                case DisplayNameColorMode.FriendsOnly:
                    if (APIUser.IsFriendsWith(player.field_Private_APIUser_0.id))
                        playerColor = "#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(player.field_Private_APIUser_0));
                    break;
            }
        }

        protected static string GetPlatform(Player player)
        {
            if (player.field_Private_APIUser_0.last_platform == "standalonewindows")
                if (player.prop_VRCPlayerApi_0.IsUserInVR())
                    return "VR".PadRight(2);
                else
                    return "PC".PadRight(2);
            else
                return "Q".PadRight(2);
        }

        protected static void OpenPlayerInQuickMenu(Player player)
        {
            InputManager.SelectPlayer(player.field_Internal_VRCPlayer_0);
            QuickMenuContextualDisplay.Method_Public_Static_Void_VRCPlayer_0(player.field_Internal_VRCPlayer_0);
        }

        protected static string GetPingColor(int ping)
        {
            if (ping <= 75)
                return "#00ff00";
            else if (ping > 75 && ping <= 125)
                return "#008000";
            else if (ping > 125 && ping <= 175)
                return "#ffff00";
            else if (ping > 175 && ping <= 225)
                return "#ffa500";
            else
                return "#ff0000";
        }
        protected static string GetFpsColor(int fps)
        {
            if (fps >= 60)
                return "#00ff00";
            else if (fps < 60 && fps >= 45)
                return "#008000";
            else if (fps < 45 && fps >= 30)
                return "#ffff00";
            else if (fps < 30 && fps >= 15)
                return "#ffa500";
            else
                return "#ff0000";
        }
        protected static string ParsePerformanceText(PerformanceRating rating)
        {
            switch (rating)
            {
                case PerformanceRating.VeryPoor:
                    return "Awful";
                case PerformanceRating.Poor:
                    return "Poor".PadRight(5);
                case PerformanceRating.Medium:
                    return "Med".PadRight(5);
                case PerformanceRating.Good:
                    return "Good".PadRight(5);
                case PerformanceRating.Excellent:
                    return "Great";
                case PerformanceRating.None:
                    return "?¿?¿?";
                    // TODO: add load percentage??
                default:
                    return rating.ToString().PadRight(5);
            }
        }
        protected string AddLeftPart(string tempString)
        {
            if (tempString.Length > 0)
                if (Config.condensedText.Value)
                    tempString = tempString.Remove(tempString.Length - 1, 1);
                else
                    tempString = tempString.Remove(tempString.Length - 3, 3);

            if (!Config.numberedList.Value)
                if (Config.condensedText.Value)
                    tempString = "-" + tempString;
                else
                    tempString = " - " + tempString;
            else
                if (Config.condensedText.Value)
                    tempString = $"{gameObject.transform.GetSiblingIndex() - 1}.".PadRight((gameObject.transform.parent.childCount - 2).ToString().Length + 1) + tempString; // Pad by weird amount because we cant include the header and disabled template in total number of gameobjects
                else
                    tempString = $"{gameObject.transform.GetSiblingIndex() - 1}. ".PadRight((gameObject.transform.parent.childCount - 2).ToString().Length + 2) + tempString;

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
