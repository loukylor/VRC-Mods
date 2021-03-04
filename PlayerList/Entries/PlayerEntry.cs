using VRC;
using UnityEngine;

namespace PlayerList.Entries
{
    public class PlayerEntry : EntryBase
    {
        public Player player;

        public override void Init(object[] parameters)
        {
            player = (Player)parameters[0];

            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new System.Action(() => OpenPlayerInQuickMenu(player)));
        }
        public override void ProcessText(object[] parameters)
        {
            ChangeEntry("rankcolor", ("#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(player.field_Private_APIUser_0))).ToLower());
            ChangeEntry("displayname", player.field_Private_APIUser_0.displayName);

            ChangeEntry("pingcolor", GetPingColor(player.field_Internal_VRCPlayer_0.prop_Int16_0));
            ChangeEntry("ping", player.field_Internal_VRCPlayer_0.prop_Int16_0);

            // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
            ChangeEntry("fpscolor", GetFpsColor((int)(1000f / player.field_Internal_VRCPlayer_0.prop_PlayerNet_0.field_Private_Byte_0)));
            if (player.field_Internal_VRCPlayer_0.prop_PlayerNet_0.field_Private_Byte_0 == 0)
                ChangeEntry("fps", "Unknown");
            else
                ChangeEntry("fps", (int)(1000f / player.field_Internal_VRCPlayer_0.field_Private_PlayerNet_0.field_Private_Byte_0));

            ChangeEntry("platform", ParsePlatform(player.field_Private_APIUser_0.last_platform));
        }

        public static string ParsePlatform(string platform)
        {
            if (platform == "standalonewindows")
                return "PC";
            else
                return "Quest";
        }
        public static void OpenPlayerInQuickMenu(Player player) => QuickMenu.prop_QuickMenu_0.Method_Public_Void_Player_0(player);
        public static string GetPingColor(int ping)
        {
            if (ping <= 75)
                return "#00ff00ff";
            else if (ping > 75 && ping <= 125)
                return "#008000ff";
            else if (ping > 125 && ping <= 175)
                return "#ffff00ff";
            else if (ping > 175 && ping <= 225)
                return "#ffa500ff";
            else
                return "#ff0000ff";
        }
        public static string GetFpsColor(int fps)
        {
            if (fps >= 60)
                return "#00ff00ff";
            else if (fps < 60 && fps >= 45)
                return "#008000ff";
            else if (fps < 45 && fps >= 30)
                return "#ffff00ff";
            else if (fps < 30 && fps >= 15)
                return "#ffa500ff";
            else
                return "#ff0000ff";
        }
    }
}
