namespace PlayerList.Entries
{
    class WorldNameEntry : EntryBase
    {
        public override string Name { get { return "World Name"; } }

        protected override void ProcessText(object[] parameters = null) => ChangeEntry("worldname", RoomManager.field_Internal_Static_ApiWorld_0.name);
    }
}
