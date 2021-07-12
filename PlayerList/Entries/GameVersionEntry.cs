using System;
using MelonLoader;
using UnityEngine;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class GameVersionEntry : EntryBase
    {
        public GameVersionEntry(IntPtr obj0) : base(obj0) { }

        public override string Name { get { return "Game Version"; } }

        public override void Init(object[] parameters = null)
        {
            int buildNumber = GameObject.Find("_Application/ApplicationSetup").GetComponent<VRCApplicationSetup>().field_Public_Int32_0;
            textComponent.text = OriginalText.Replace("{gameversion}", buildNumber.ToString());
        }
    }
}
