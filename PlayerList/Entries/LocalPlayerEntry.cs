using System;
using UnityEngine;
using VRC;
using VRC.Core;
using VRCSDK2.Validation.Performance;

namespace PlayerList.Entries
{
    class LocalPlayerEntry : EntryBase
    {
        // " - <color={pingcolor}>{ping}ms</color> | <color={fpscolor}>{fps}</color> | {platform} | <color={perfcolor}>{perf}</color> | {relationship} | <color={rankcolor}>{displayname}</color>"
        public override string Name { get { return "Local Player"; } }

        public override void Init(object[] parameters)
        {
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new Action(() => PlayerEntry.OpenPlayerInQuickMenu(Player.prop_Player_0)));
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

            // Convert to byte as that's what's sent over network so if you spoof your ping you'll see what's actually sent over the network
            if (Config.pingToggle.Value)
            { 
                short ping = (short)Photon.Pun.PhotonNetwork.field_Public_Static_LoadBalancingClient_0.prop_LoadBalancingPeer_0.RoundTripTime;
                AddColor(PlayerEntry.GetPingColor(ping));
                if (ping < 9999 && ping > -999)
                    AddEndColor(ping.ToString().PadRight(4) + "ms");
                else
                    AddEndColor(((double)(ping / 1000)).ToString("N1").PadRight(5) + "s");
                AddSpacer();
            }

            // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
            if (Config.fpsToggle.Value)
            { 
                int fps = Mathf.Clamp((int)(1f / Time.deltaTime), -99, 999); // Clamp between -99 and 999
                AddColoredText(PlayerEntry.GetFpsColor(fps), fps.ToString().PadRight(3));
                AddSpacer();
            }

            if (Config.platformToggle.Value)
            { 
                AddText(PlayerEntry.ParsePlatform(Player.prop_Player_0).PadRight(2));
                AddSpacer();
            }

            if (Config.perfToggle.Value)
            {
                PerformanceRating rating = Player.prop_Player_0.field_Internal_VRCPlayer_0.prop_VRCAvatarManager_0.prop_AvatarPerformanceStats_0.field_Private_ArrayOf_PerformanceRating_0[(int)AvatarPerformanceCategory.Overall]; // Get from cache so it doesnt calculate perf all at once
                AddColoredText("#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, rating)), PlayerEntry.ParsePerformanceText(rating).PadRight(5));
                AddSpacer();
            }

            if (Config.distanceToggle.Value)
            {
                AddText("0.0 m");
                AddSpacer();
            }
            
            if (Config.displayNameToggle.Value)
            {
                switch (Config.DisplayNameColorMode)
                {
                    case PlayerListMod.DisplayNameColorMode.None:
                    case PlayerListMod.DisplayNameColorMode.FriendsOnly:
                        AddText(APIUser.CurrentUser.displayName);
                        break;
                    case PlayerListMod.DisplayNameColorMode.TrustAndFriends:
                    case PlayerListMod.DisplayNameColorMode.TrustOnly:
                        AddColoredText("#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(APIUser.CurrentUser)), APIUser.CurrentUser.displayName);
                        break;
                }
                AddSpacer();
            }

            if (textComponent.text.Length > 0)
                if (Config.condensedText.Value)
                    textComponent.text = textComponent.text.Remove(textComponent.text.Length - 1, 1);
                else
                    textComponent.text = textComponent.text.Remove(textComponent.text.Length - 3, 3);

            if (!Config.numberedList.Value)
                if (Config.condensedText.Value)
                    AddTextToBeginning("-");
                else
                    AddTextToBeginning(" - ");
            else
                if (Config.condensedText.Value)
                    AddTextToBeginning("1.".PadRight((gameObject.transform.parent.childCount - 2).ToString().Length + 1));
                else
                    AddTextToBeginning("1. ".PadRight((gameObject.transform.parent.childCount - 2).ToString().Length + 2));
        }
    }
}
