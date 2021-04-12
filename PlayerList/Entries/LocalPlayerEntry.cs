using System;
using System.Linq;
using System.Reflection;
using Harmony;
using PlayerList.Config;
using PlayerList.Utilities;
using UnityEngine;
using VRC;
using VRC.Core;
using VRCSDK2.Validation.Performance;

namespace PlayerList.Entries
{
    class LocalPlayerEntry : PlayerEntry
    {
        // " - <color={pingcolor}>{ping}ms</color> | <color={fpscolor}>{fps}</color> | {platform} | <color={perfcolor}>{perf}</color> | {relationship} | <color={rankcolor}>{displayname}</color>"
        public override string Name { get { return "Local Player"; } }

        public new delegate void UpdateEntryDelegate(Player player, LocalPlayerEntry entry, ref string tempString);
        public static new UpdateEntryDelegate updateDelegate;
        public static new void EntryInit()
        {
            MethodInfo OnShowSocialRankChangeMethod = typeof(QuickMenu).GetMethods()
                .First(mi => mi.Name.StartsWith("Method_Public_Void_") && mi.GetParameters().Length == 0 && Xref.CheckUsing(mi, "Method_Public_Static_Void_EnumNPublicSealed", typeof(VRCInputManager))); // && (!mi.Name.EndsWith("1") && !mi.Name.EndsWith("8") && !mi.Name.EndsWith("12") && !mi.Name.EndsWith("17") && !mi.Name.EndsWith("6"))))
            PlayerListMod.Instance.Harmony.Patch(OnShowSocialRankChangeMethod, null, new HarmonyMethod(typeof(LocalPlayerEntry).GetMethod(nameof(OnShowSocialRankChange), BindingFlags.NonPublic | BindingFlags.Static)));
        }
        public override void Init(object[] parameters)
        {
            player = Player.prop_Player_0;
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new Action(() => OpenPlayerInQuickMenu(player)));

            platform = GetPlatform(player).PadRight(2);

            NetworkEvents.OnPlayerJoin += new Action<Player>((player) =>
            {
                int highestId = 0;
                foreach (int photonId in PlayerManager.prop_PlayerManager_0.field_Private_Dictionary_2_Int32_Player_0.Keys)
                    if (photonId > highestId)
                        highestId = photonId;

                highestPhotonIdLength = highestId.ToString().Length;
            });

            textComponent.text = "Loading...";
            
            GetPlayerColor();
            OnConfigChanged();
        }
        public override void OnConfigChanged()
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

            GetPlayerColor();
        }
        protected override void ProcessText(object[] parameters)
        {
            // TODO: Figure out how to figure out how to know when someone blcosk u
            /*
            List<PlayerEntry> playerEntries = PlayerListMod.playerEntries.Values.ToList();
            // Get blocked things
            foreach (ApiPlayerModeration moderation in ModerationManager.prop_ModerationManager_0.field_Private_List_1_ApiPlayerModeration_0)
            {
                if (moderation.moderationType != ApiPlayerModeration.ModerationType.Block) continue;

                foreach (PlayerEntry playerEntry in playerEntries)
                {
                    if (playerEntry.youBlocked) continue;

                    if (playerEntry.player == null)
                    {
                        playerEntry.Remove();
                        continue;
                    }

                    if (playerEntry.userID == moderation.targetUserId)
                    { 
                        playerEntry.youBlocked = true;
                        MelonLoader.MelonLogger.Msg($"You have blocked {moderation.targetDisplayName}");
                        break;
                    }
                }
            }
            */
            string tempString = "";

            player = Player.prop_Player_0;
            updateDelegate?.Invoke(player, this, ref tempString);

            tempString = AddLeftPart(tempString);
            textComponent.text = tempString;
        }

        private static void AddPing(Player player, LocalPlayerEntry entry, ref string tempString)
        {
            short ping = (short)Photon.Pun.PhotonNetwork.field_Public_Static_LoadBalancingClient_0.prop_LoadBalancingPeer_0.RoundTripTime;
            tempString += "<color=" + GetPingColor(ping) + ">";
            if (ping < 9999 && ping > -999)
                tempString += ping.ToString().PadRight(4) + "ms</color>";
            else
                tempString += ((double)(ping / 1000)).ToString("N1").PadRight(5) + "s</color>";
            tempString += separator;
        }
        private static void AddFps(Player player, LocalPlayerEntry entry, ref string tempString)
        {
            int fps = Mathf.Clamp((int)(1f / Time.deltaTime), -99, 999); // Clamp between -99 and 999
            tempString += "<color=" + GetFpsColor(fps) + ">" + fps.ToString().PadRight(3) + "</color>" + separator;
        }
        private static void AddPlatform(Player player, LocalPlayerEntry entry, ref string tempString)
        {
            tempString += entry.platform + separator;
        }
        private static void AddPerf(Player player, LocalPlayerEntry entry, ref string tempString)
        {
            entry.perf = player.field_Internal_VRCPlayer_0.prop_VRCAvatarManager_0.prop_AvatarPerformanceStats_0.field_Private_ArrayOf_PerformanceRating_0[(int)AvatarPerformanceCategory.Overall]; // Get from cache so it doesnt calculate perf all at once
            tempString += "<color=#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, entry.perf)) + ">" + ParsePerformanceText(entry.perf) + "</color>" + separator;
        }
        private static void AddDistance(Player player, LocalPlayerEntry entry, ref string tempString)
        {
            tempString += "0.0 m" + separator;
        }
        private static void AddPhotonId(Player player, LocalPlayerEntry entry, ref string tempString)
        {
            tempString += player.field_Internal_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0.ToString().PadRight(highestPhotonIdLength) + separator;
        }
        private static void AddDisplayName(Player player, LocalPlayerEntry entry, ref string tempString)
        {
            tempString += "<color=" + entry.playerColor + ">" + player.field_Private_APIUser_0.displayName + "</color>" + separator;
        }

        private static void OnShowSocialRankChange()
        {
            EntryManager.localPlayerEntry.GetPlayerColor();
        }
        private void GetPlayerColor()
        {
            playerColor = "";
            switch (PlayerListConfig.DisplayNameColorMode)
            {
                case DisplayNameColorMode.None:
                case DisplayNameColorMode.FriendsOnly:
                    break;
                case DisplayNameColorMode.TrustAndFriends:
                case DisplayNameColorMode.TrustOnly:
                    playerColor = "#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(APIUser.CurrentUser));
                    break;
            }
        }
    }
}
