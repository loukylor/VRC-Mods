using UnityEngine;

namespace PlayerList.Entries
{
    class GameVersionEntry : EntryBase
    {
        public override string Name { get { return "Game Version"; } }

        public static int buildNumber = GameObject.Find("_Application/ApplicationSetup").GetComponent<VRCApplicationSetup>().field_Public_Int32_0;

        public override void Init(object[] parameters = null)
        {
            ChangeEntry("gameversion", buildNumber);
        }
    }
}
