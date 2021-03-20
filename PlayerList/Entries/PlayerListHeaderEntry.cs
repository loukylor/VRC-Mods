namespace PlayerList.Entries
{
    class PlayerListHeaderEntry : EntryBase
    {
        public override string Name { get { return "PlayerList Header"; } }

        protected override void ProcessText(object[] parameters = null) => ChangeEntry("playercount", EntryManager.playerEntries.Count + 1);
    }
}
