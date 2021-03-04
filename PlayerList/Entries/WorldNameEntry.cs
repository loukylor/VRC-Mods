namespace PlayerList.Entries
{
    class WorldNameEntry : EntryBase
    {
        public override void ProcessText(object[] parameters = null) => ChangeEntry("worldname", RoomManager.field_Internal_Static_ApiWorld_0.name);
    }
}
