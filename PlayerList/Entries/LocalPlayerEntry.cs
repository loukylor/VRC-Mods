﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.Management;

namespace PlayerList.Entries
{
    class LocalPlayerEntry : EntryBase
    {
        // " - <color={pingcolor}>{ping}ms</color> | <color={fpscolor}>{fps}</color> | {platform} | <color={perfcolor}>{perf}</color> | <color={rankcolor}>{displayname}</color>"
        public override string Name { get { return "Local Player"; } }

        public TMPro.TextMeshProUGUI perfText;

        public override void Init(object[] parameters)
        {
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new Action(() => PlayerEntry.OpenPlayerInQuickMenu(Player.prop_Player_0)));
            perfText = Player.prop_Player_0.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Quick Stats/Performance Text").GetComponent<TMPro.TextMeshProUGUI>();
        }
        protected override void ProcessText(object[] parameters)
        {
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

            if (Config.condensedText.Value)
                AddText("-");
            else
                AddText(" - ");

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
                AddColoredText(PlayerEntry.GetFpsColor((int)(1f / Time.deltaTime)), ((int)(1f / Time.deltaTime)).ToString().PadRight(3));
                AddSpacer();
            }

            if (Config.platformToggle.Value)
            { 
                AddText(PlayerEntry.ParsePlatform(Player.prop_Player_0).PadRight(5));
                AddSpacer();
            }

            if (Config.perfToggle.Value)
            { 
                if (perfText != null)
                {
                    AddColoredText("#" + ColorUtility.ToHtmlStringRGB(perfText.color), PlayerEntry.ParsePerformanceText(perfText.text).PadRight(5));
                    AddSpacer();
                }
                else
                {
                    perfText = Player.prop_Player_0.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Quick Stats/Performance Text").GetComponent<TMPro.TextMeshProUGUI>();
                    AddColoredText("#ff00000", "???");
                    AddSpacer();
                }
            }

            if (Config.displayNameToggle.Value)
            {
                AddColoredText("#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(APIUser.CurrentUser)), APIUser.CurrentUser.displayName);
                AddSpacer();
            }

            if (Config.condensedText.Value)
                textComponent.text = textComponent.text.Remove(textComponent.text.Length - 1, 1);
            else
                textComponent.text = textComponent.text.Remove(textComponent.text.Length - 3, 3);
        }
    }
}