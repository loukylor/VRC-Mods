namespace PlayerList.Entries
{
    class RiskyFuncAllowedEntry : EntryBase
    {
        public override string Name { get { return "Risky Functions Allowed"; } }

        public override void Init(object[] parameters = null)
        {
            EntryManager.OnWorldAllowedChanged += OnWorldAllowedChanged;
        }

        public void OnWorldAllowedChanged()
        {
            textComponent.text = OriginalText.Replace("{riskyfuncallowed}", EntryManager.WorldAllowed.ToString());
        }
    }
}
