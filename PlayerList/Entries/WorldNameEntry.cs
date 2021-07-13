using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using VRC.Core;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class WorldNameEntry : EntryBase
    {
        public WorldNameEntry(IntPtr obj0) : base(obj0) { }

        [HideFromIl2Cpp]
        public override string Name => "World Name";

        [HideFromIl2Cpp]
        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            textComponent.text = OriginalText.Replace("{worldname}", world.name);
        }
    }
}
