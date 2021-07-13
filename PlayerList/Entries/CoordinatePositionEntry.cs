using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using VRC;
using VRChatUtilityKit.Utilities;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class CoordinatePositionEntry : EntryBase
    {
        public CoordinatePositionEntry(IntPtr obj0) : base(obj0) { }

        [HideFromIl2Cpp]
        public override string Name => "Coordinate Position";

        private Vector3 lastPos;

        [HideFromIl2Cpp]
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
