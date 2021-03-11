using UnityEngine;
using VRC;
using VRC.Core;

namespace PlayerList.Entries
{
    class LocalPlayerEntry : EntryBase
    {
        public override string Name { get { return "Local Player"; } }

        public TMPro.TextMeshProUGUI perfText;

        public override void Init(object[] parameters)
        {
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new System.Action(() => PlayerEntry.OpenPlayerInQuickMenu(Player.prop_Player_0)));
            perfText = Player.prop_Player_0.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Quick Stats/Performance Text").GetComponent<TMPro.TextMeshProUGUI>();
        }
        public override void ProcessText(object[] parameters)
        {
            if (Config.condensedText.Value && !Config.HasSomethingOff)
                textComponent.text = textComponent.text.Replace(" ", "");

            // Convert to byte as that's what's sent over network so if you spoof your ping you'll see what's actually sent over the network
            if (Config.pingToggle.Value)
            { 
                ChangeEntry("pingcolor", PlayerEntry.GetPingColor((short)Photon.Pun.PhotonNetwork.field_Public_Static_LoadBalancingClient_0.prop_LoadBalancingPeer_0.RoundTripTime)); 
                ChangeEntry("ping", ((short)Photon.Pun.PhotonNetwork.field_Public_Static_LoadBalancingClient_0.prop_LoadBalancingPeer_0.RoundTripTime).ToString().PadRight(4));
            }

            // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
            if (Config.fpsToggle.Value)
            {
                ChangeEntry("fpscolor", PlayerEntry.GetFpsColor((int)(1f / Time.deltaTime)));
                ChangeEntry("fps", ((int)(1f / Time.deltaTime)).ToString().PadRight(3));
            }

            if (Config.platformToggle.Value)
                ChangeEntry("platform", PlayerEntry.ParsePlatform(Player.prop_Player_0).PadRight(5));

            if (Config.perfToggle.Value)
            { 
                if (perfText != null)
                {
                    ChangeEntry("perfcolor", "#" + ColorUtility.ToHtmlStringRGB(perfText.color));
                    ChangeEntry("perf", PlayerEntry.ParsePerformanceText(perfText.text).PadRight(5));
                }
                else
                {
                    perfText = Player.prop_Player_0.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Quick Stats/Performance Text").GetComponent<TMPro.TextMeshProUGUI>();
                    ChangeEntry("perfcolor", "#ff00000");
                    ChangeEntry("perf", "???");
                }
            }

            if (Config.displayNameToggle.Value)
            {
                ChangeEntry("rankcolor", "#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(APIUser.CurrentUser)));
                ChangeEntry("displayname", APIUser.CurrentUser.displayName);
            }

            if (Config.HasSomethingOff)
                textComponent.text = " - " + PlayerEntry.RemoveOffToggles(textComponent.text.Substring(3));
        }
    }
}
