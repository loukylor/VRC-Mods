using System;
using MelonLoader;
using VRC.Core;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class WorldNameEntry : EntryBase
    {
        public WorldNameEntry(IntPtr obj0) : base(obj0) { }

        public override string Name { get { return "World Name"; } }

        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            textComponent.text = OriginalText.Replace("{worldname}", world.name);
        }
    }
}
