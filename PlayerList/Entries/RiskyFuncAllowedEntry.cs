using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using VRChatUtilityKit.Utilities;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class RiskyFuncAllowedEntry : EntryBase
    {
        public RiskyFuncAllowedEntry(IntPtr obj0) : base(obj0) { }

        [HideFromIl2Cpp]
        public override string Name => "Risky Functions Allowed";

        [HideFromIl2Cpp]
        public override void Init(object[] parameters = null)
        {
            VRCUtils.OnEmmWorldCheckCompleted += OnEmmWorldCheckCompleted;
        }

        [HideFromIl2Cpp]
        public void OnEmmWorldCheckCompleted(bool areRiskyFuncsAllowed)
        {
            textComponent.text = OriginalText.Replace("{riskyfuncallowed}", areRiskyFuncsAllowed.ToString());
        }
    }
}
