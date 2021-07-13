using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using VRC.Core;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class WorldAuthorEntry : EntryBase
    {
        public WorldAuthorEntry(IntPtr obj) : base(obj) { }

        [HideFromIl2Cpp]
        public override string Name => "World Author";

        [HideFromIl2Cpp]
        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            if (APIUser.CurrentUser != null && world.authorId == APIUser.CurrentUser.id && LocalPlayerEntry.emmNameSpoofEnabled)
                textComponent.text = OriginalText.Replace("{worldauthor}", LocalPlayerEntry.emmSpoofedName);
            else
                textComponent.text = OriginalText.Replace("{worldauthor}", world.authorName);
        }
    }
}