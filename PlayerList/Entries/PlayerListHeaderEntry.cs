namespace PlayerList.Entries
{
    class PlayerListHeaderEntry : EntryBase
    {
        public override void ProcessText(object[] parameters = null) => ChangeEntry("playercount", PlayerListMod.playerEntries.Count + 1);
    }
}
