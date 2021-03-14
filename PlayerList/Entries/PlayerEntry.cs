using System;
using UnityEngine;
using VRC;
using VRCSDK2.Validation.Performance;

namespace PlayerList.Entries
{
    public class PlayerEntry : EntryBase
    {
        // - <color={pingcolor}>{ping}ms</color> | <color={fpscolor}>{fps}</color> | {platform} | <color={perfcolor}>{perf}</color> | {relationship} | <color={rankcolor}>{displayname}</color>
        public override string Name { get { return "Player"; } }

        public Player player;
        public string userID;
        public PlayerNet playerNet;

        public bool blockedYou;
        public bool youBlocked;

        public override void Init(object[] parameters)
        {
            player = (Player)parameters[0];
            userID = player.field_Private_APIUser_0.id;
            playerNet = player.GetComponent<PlayerNet>();

            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new Action(() => OpenPlayerInQuickMenu(player)));
        }
        protected override void ProcessText(object[] parameters = null)
        {
            if (player == null) Remove(); // Sometimes ppl will desync causing the leave event to not call

            if (Config.condensedText.Value)
                AddText("-");
            else
                AddText(" - ");

            if (playerNet != null)
            {
                if (Config.pingToggle.Value)
                {
                    short ping = playerNet.prop_Int16_0;
                    AddColor(GetPingColor(ping));
                    if (ping < 9999 && ping > -999)
                        AddEndColor(ping.ToString().PadRight(4) + "ms");
                    else
                        AddEndColor(((double)(ping / 1000)).ToString("N1").PadRight(5) + "s");
                    AddSpacer();
                }

                // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
                if (Config.fpsToggle.Value)
                {
                    int fps = (int)(1000f / playerNet.field_Private_Byte_0) < -99 ? -99 : Math.Min((int)(1000f / playerNet.field_Private_Byte_0), 999);

                    AddColor(GetFpsColor(fps));
                    if (playerNet.field_Private_Byte_0 == 0)
                        AddEndColor("?".PadRight(3));
                    else
                        AddEndColor(fps.ToString().PadRight(3));
                    AddSpacer();
                }
            }
            else
            {
                if (Config.pingToggle.Value)
                {
                    AddColoredText("#ff0000", "?".PadRight(4));
                    AddSpacer();
                }

                // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
                if (Config.fpsToggle.Value)
                {
                    AddColoredText("#ff0000", "?".PadRight(3));
                    AddSpacer();
                }

                playerNet = player.GetComponent<PlayerNet>();
            }

            if (Config.platformToggle.Value)
            {
                AddText(ParsePlatform(player).PadRight(2));
                AddSpacer();
            }

            if (Config.perfToggle.Value)
            {
                PerformanceRating rating = player.field_Internal_VRCPlayer_0.prop_VRCAvatarManager_0.prop_AvatarPerformanceStats_0.field_Private_ArrayOf_PerformanceRating_0[(int)AvatarPerformanceCategory.Overall]; // Get from cache so it doesnt calculate perf all at once
                AddColoredText("#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, rating)), ParsePerformanceText(rating));
                AddSpacer();
            }

            if (Config.displayNameToggle.Value) // Why?
            {
                AddColoredText("#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(player.field_Private_APIUser_0)), player.field_Private_APIUser_0.displayName);
                AddSpacer();
            }

            if (Config.condensedText.Value)
                textComponent.text = textComponent.text.Remove(textComponent.text.Length - 1, 1);
            else
                textComponent.text = textComponent.text.Remove(textComponent.text.Length - 3, 3);
        }
        public void Remove()
        {
            PlayerListMod.playerEntries.Remove(userID);
            PlayerListMod.entries.Remove(Identifier);
            UnityEngine.Object.DestroyImmediate(gameObject);
            return;
        }

        public static string ParsePlatform(Player player)
        {
            if (player.field_Private_APIUser_0.last_platform == "standalonewindows")
                if (player.field_Private_VRCPlayerApi_0.IsUserInVR())
                    return "VR".PadRight(2);
                else
                    return "PC".PadRight(2);
            else
                return "Q".PadRight(2);
        }
        
        public static void OpenPlayerInQuickMenu(Player player)
        {
            if (InputManager.mouseCursor.gameObject.activeSelf) InputManager.mouseCursor.Method_Public_Void_VRCPlayer_0(player.field_Internal_VRCPlayer_0);

            if (InputManager.rightCursor.gameObject.activeSelf) InputManager.rightCursor.Method_Public_Void_VRCPlayer_0(player.field_Internal_VRCPlayer_0);

            if (InputManager.leftCursor.gameObject.activeSelf) InputManager.leftCursor.Method_Public_Void_VRCPlayer_0(player.field_Internal_VRCPlayer_0);

            QuickMenuContextualDisplay.Method_Public_Static_Void_VRCPlayer_0(player.field_Internal_VRCPlayer_0);
        }

        public static string GetPingColor(int ping)
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
        public static string GetFpsColor(int fps)
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
        public static string ParsePerformanceText(PerformanceRating rating)
        {
            switch (rating)
            {
                case PerformanceRating.VeryPoor:
                    return "Awful".PadRight(5);
                case PerformanceRating.Poor:
                    return "Poor".PadRight(5);
                case PerformanceRating.Medium:
                    return "Med".PadRight(5);
                case PerformanceRating.Good:
                    return "Good".PadRight(5);
                case PerformanceRating.Excellent:
                    return "Great".PadRight(5);
                default:
                    return rating.ToString().PadRight(5);
            }
        }
    }
}
