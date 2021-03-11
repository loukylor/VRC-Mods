using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC;

namespace PlayerList.Entries
{
    public class PlayerEntry : EntryBase
    {
        public override string Name { get { return "Player"; } }

        public Player player;
        public string userID;
        public PlayerNet playerNet;
        public TMPro.TextMeshProUGUI perfText;

        public override void Init(object[] parameters)
        {
            player = (Player)parameters[0];
            userID = player.field_Private_APIUser_0.id;
            playerNet = player.GetComponent<PlayerNet>();

            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new System.Action(() => OpenPlayerInQuickMenu(player)));
            perfText = player.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Quick Stats/Performance Text").GetComponent<TMPro.TextMeshProUGUI>();
        }
        public override void ProcessText(object[] parameters)
        {
            if (player == null) // Sometimes ppl will desync causing the leave event to not call
            {
                PlayerListMod.playerEntries.Remove(userID);
                PlayerListMod.entries.Remove(Identifier);
                Object.DestroyImmediate(gameObject);
                return;
            }
            if (Config.condensedText.Value && !Config.HasSomethingOff)
                textComponent.text = textComponent.text.Replace(" ", "");

            if (playerNet != null)
            {
                if (Config.pingToggle.Value)
                {
                    ChangeEntry("pingcolor", GetPingColor(playerNet.prop_Int16_0));
                    ChangeEntry("ping", playerNet.prop_Int16_0.ToString().PadRight(4));
                }

                // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
                if (Config.fpsToggle.Value)
                {
                    ChangeEntry("fpscolor", GetFpsColor((int)(1000f / playerNet.field_Private_Byte_0)));
                    if (playerNet.field_Private_Byte_0 == 0)
                        ChangeEntry("fps", "?".PadRight(3));
                    else
                        ChangeEntry("fps", ((int)(1000f / playerNet.field_Private_Byte_0)).ToString().PadRight(3));
                }
            }
            else
            {
                if (Config.pingToggle.Value)
                {
                    ChangeEntry("pingcolor", "#ff0000");
                    ChangeEntry("ping", "?".PadRight(4));
                }


                // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
                if (Config.fpsToggle.Value)
                {
                    ChangeEntry("fpscolor", "#ff0000");
                    ChangeEntry("fps", "?".PadRight(3));
                }

                playerNet = player.GetComponent<PlayerNet>();
            }

            if (Config.platformToggle.Value)
                ChangeEntry("platform", ParsePlatform(player));

            if (Config.perfToggle.Value)
            {
                if (perfText != null)
                {
                    ChangeEntry("perfcolor", "#" + ColorUtility.ToHtmlStringRGB(perfText.color));
                    ChangeEntry("perf", ParsePerformanceText(perfText.text).PadRight(5));
                }
                else
                {
                    perfText = player.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Quick Stats/Performance Text").GetComponent<TMPro.TextMeshProUGUI>();
                    ChangeEntry("perfcolor", "#ff00000");
                    ChangeEntry("perf", "???");
                }
            }

            if (Config.displayNameToggle.Value) // Why?
            {
                ChangeEntry("rankcolor", "#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(player.field_Private_APIUser_0)));
                ChangeEntry("displayname", player.field_Private_APIUser_0.displayName);
            }

            if (Config.HasSomethingOff)
                textComponent.text = " - " + RemoveOffToggles(textComponent.text.Substring(3));
        }

        public static string ParsePlatform(Player player)
        {
            if (player.field_Private_APIUser_0.last_platform == "standalonewindows")
                if (player.field_Private_VRCPlayerApi_0.IsUserInVR())
                    return "VR".PadRight(5);
                else
                    return "PC".PadRight(5);
            else
                return "Quest".PadRight(5);
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
        public static string ParsePerformanceText(string perfText)
        {
            switch (perfText.ToLower())
            {
                case "very poor":
                    return "Awful";
                case "poor":
                    return "Poor";
                case "medium":
                    return "Med";
                case "good":
                    return "Good";
                case "excellent":
                    return "Great";
                default:
                    return perfText;
            }
        }
        public static string RemoveOffToggles(string originalString)
        {
            int totalRemoved = 0;
            List<string> splitText;
            splitText = originalString.Split(new string[] { " | " }, System.StringSplitOptions.None).ToList();

            if (!Config.pingToggle.Value)
            {
                splitText.RemoveAt(0);
                totalRemoved++;
            }
            if (!Config.fpsToggle.Value)
            {
                splitText.RemoveAt(1 - totalRemoved);
                totalRemoved++;
            }
            if (!Config.platformToggle.Value)
            {
                splitText.RemoveAt(2 - totalRemoved);
                totalRemoved++;
            }
            if (!Config.perfToggle.Value)
            {
                splitText.RemoveAt(3 - totalRemoved);
                totalRemoved++;
            }
            if (!Config.displayNameToggle.Value)
            {
                splitText.RemoveAt(4 - totalRemoved);
            }

            string finalString = "";
            for (int i = 0; i < splitText.Count; i++)
            {
                finalString += splitText[i];
                if (i + 1 == splitText.Count) continue;

                if (Config.condensedText.Value)
                    finalString += "|";
                else
                    finalString += " | ";
            }

            return finalString;
        }
    }
}
