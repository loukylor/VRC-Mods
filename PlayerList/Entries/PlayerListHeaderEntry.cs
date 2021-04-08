namespace PlayerList.Entries
{
    class PlayerListHeaderEntry : EntryBase
    {
        public override string Name { get { return "PlayerList Header"; } }

        public int lastPlayerCount;

        protected override void ProcessText(object[] parameters = null)
        {
            if (lastPlayerCount == EntryManager.playerEntries.Count + 1)
                return;

            lastPlayerCount = EntryManager.playerEntries.Count + 1; // Is doing it like this any faster?
            ResetEntry();
            ChangeEntry("playercount", EntryManager.playerEntries.Count + 1);
        }
    }
}
