using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class GameVersionEntry : EntryBase
    {
        public GameVersionEntry(IntPtr obj0) : base(obj0) { }

        [HideFromIl2Cpp]
        public override string Name => "Game Version";

        [HideFromIl2Cpp]
        public override void Init(object[] parameters = null)
        {
            int buildNumber = GameObject.Find("_Application/ApplicationSetup").GetComponent<VRCApplicationSetup>().field_Public_Int32_0;
            textComponent.text = OriginalText.Replace("{gameversion}", buildNumber.ToString());
        }
    }
}
