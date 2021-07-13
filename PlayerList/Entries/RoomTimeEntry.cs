using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class RoomTimeEntry : EntryBase
    {
        public RoomTimeEntry(IntPtr obj0) : base(obj0) { }

        [HideFromIl2Cpp]
        public override string Name => "Room Time";

        public string lastTime;

        [HideFromIl2Cpp]
        protected override void ProcessText(object[] parameters = null)
        {
            TimeSpan time = TimeSpan.FromSeconds(RoomManager.prop_Single_0);
            string timeString = time.ToString(@"hh\:mm\:ss");
            if (lastTime == timeString)
                return;
            
            lastTime = timeString;
            textComponent.text = OriginalText.Replace("{roomtime}", timeString);
        }
    }
}
