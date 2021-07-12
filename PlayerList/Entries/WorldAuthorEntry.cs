using System;
using MelonLoader;
using VRC.Core;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class WorldAuthorEntry : EntryBase
    {
        public WorldAuthorEntry(IntPtr obj) : base(obj) { }

        public override string Name { get { return "World Author"; } }

        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            if (APIUser.CurrentUser != null && world.authorId == APIUser.CurrentUser.id && LocalPlayerEntry.emmNameSpoofEnabled)
                textComponent.text = OriginalText.Replace("{worldauthor}", LocalPlayerEntry.emmSpoofedName);
            else
                textComponent.text = OriginalText.Replace("{worldauthor}", world.authorName);
        }
    }
}