using VRChatUtilityKit.Utilities;

namespace PlayerList.Entries
{
    class RiskyFuncAllowedEntry : EntryBase
    {
        public override string Name { get { return "Risky Functions Allowed"; } }

        public override void Init(object[] parameters = null)
        {
            VRCUtils.OnEmmWorldCheckCompleted += OnEmmWorldCheckCompleted;
        }

        public void OnEmmWorldCheckCompleted(bool areRiskyFuncsAllowed)
        {
            textComponent.text = OriginalText.Replace("{riskyfuncallowed}", areRiskyFuncsAllowed.ToString());
        }
    }
}
