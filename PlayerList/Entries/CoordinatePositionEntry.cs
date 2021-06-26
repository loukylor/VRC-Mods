using UnityEngine;
using VRC;
using VRChatUtilityKit.Utilities;

namespace PlayerList.Entries
{
    class CoordinatePositionEntry : EntryBase
    {
        public override string Name { get { return "Coordinate Position"; } }

        private Vector3 lastPos;

        protected override void ProcessText(object[] parameters = null)
        {
            Vector3 pos = Player.prop_Player_0.gameObject.transform.position.RoundAmount(0.1f);

            if (pos == lastPos) return;
            lastPos = pos;

            string tempString = OriginalText;
            tempString = tempString.Replace("{x}", lastPos.x.ToString());
            tempString = tempString.Replace("{y}", lastPos.y.ToString());
            tempString = tempString.Replace("{z}", lastPos.z.ToString());
            textComponent.text = tempString;
        }
    }
}
