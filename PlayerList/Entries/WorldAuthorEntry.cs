using VRC.Core;

namespace PlayerList.Entries
{
    class WorldAuthorEntry : EntryBase
    {
        public override string Name { get { return "World Author"; } }

        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            ResetEntry();
            ChangeEntry("worldauthor", world.authorName);
        }
    }
}