namespace PlayerList.Entries
{
    class WorldAuthorEntry : EntryBase
    {
        public override string Name { get { return "World Author"; } }

        protected override void ProcessText(object[] parameters = null) => ChangeEntry("worldauthor", RoomManager.field_Internal_Static_ApiWorld_0.authorName);
    }
}