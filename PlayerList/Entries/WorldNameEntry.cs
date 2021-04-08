using VRC.Core;

namespace PlayerList.Entries
{
    class WorldNameEntry : EntryBase
    {
        public override string Name { get { return "World Name"; } }

        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            ChangeEntry("worldname", world.name);
        }
    }
}
