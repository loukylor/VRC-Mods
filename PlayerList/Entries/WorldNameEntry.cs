using VRC.Core;

namespace PlayerList.Entries
{
    class WorldNameEntry : EntryBase
    {
        public override string Name { get { return "World Name"; } }

        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            ResetEntry();
            ChangeEntry("worldname", world.name);
        }
    }
}
