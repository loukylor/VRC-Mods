using UnityEngine;
using VRC;
using VRC.Core;

namespace PlayerList.Entries
{
    class LocalPlayerEntry : EntryBase
    {
        public override string Name { get { return "Local Player"; } }

        public override void Init(object[] parameters) => gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new System.Action(() => PlayerEntry.OpenPlayerInQuickMenu(Player.prop_Player_0)));
        public override void ProcessText(object[] parameters)
        {
            ChangeEntry("rankcolor", ("#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(APIUser.CurrentUser))).ToLower());
            ChangeEntry("displayname", APIUser.CurrentUser.displayName);

            ChangeEntry("pingcolor", PlayerEntry.GetPingColor(Photon.Pun.PhotonNetwork.field_Public_Static_LoadBalancingClient_0.prop_LoadBalancingPeer_0.RoundTripTime));
            ChangeEntry("ping", Photon.Pun.PhotonNetwork.field_Public_Static_LoadBalancingClient_0.prop_LoadBalancingPeer_0.RoundTripTime);

            // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
            ChangeEntry("fpscolor", PlayerEntry.GetFpsColor((int)(1f / Time.deltaTime)));
            ChangeEntry("fps", (int)(1f / Time.deltaTime));

            ChangeEntry("platform", PlayerEntry.ParsePlatform(APIUser.CurrentUser.last_platform));
        }
    }
}
