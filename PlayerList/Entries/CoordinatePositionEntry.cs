using System;
using UnityEngine;
using VRC;

namespace PlayerList.Entries
{
    class CoordinatePositionEntry : EntryBase
    {
        public override string Name { get { return "Coordinate Position"; } }

        private Vector3Int lastPos;

        protected override void ProcessText(object[] parameters = null)
        {
            Vector3Int pos = Vector3Int.RoundToInt(Player.prop_Player_0.gameObject.transform.position);

            if (pos == lastPos) return;
            lastPos = pos;

            string tempString = OriginalText;
            tempString = tempString.Replace("{x}", Math.Round(Player.prop_Player_0.gameObject.transform.position.x, 1).ToString());
            tempString = tempString.Replace("{y}", Math.Round(Player.prop_Player_0.gameObject.transform.position.y, 1).ToString());
            tempString = tempString.Replace("{z}", Math.Round(Player.prop_Player_0.gameObject.transform.position.z, 1).ToString());
            textComponent.text = tempString;
        }
    }
}
