using System.Linq;
using UnityEngine;

namespace PlayerList.Entries
{
    class GameVersionEntry : EntryBase
    {
        public override string Name { get { return "Game Version"; } }

        public static int buildNumber = Resources.FindObjectsOfTypeAll<VRCApplicationSetup>().First().field_Public_Int32_0;

        protected override void ProcessText(object[] parameters = null) => ChangeEntry("gameversion", buildNumber);
    }
}
