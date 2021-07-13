using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class SystemTime12HrEntry : EntryBase
    {
        public SystemTime12HrEntry(IntPtr obj0) : base(obj0) { }

        [HideFromIl2Cpp]
        public override string Name => "System Time 12Hr";

        public string lastTime;

        [HideFromIl2Cpp]
        protected override void ProcessText(object[] parameters = null)
        {
            string timeString = DateTime.Now.ToString(@"hh\:mm\:ss tt");
            if (lastTime == timeString)
                return;

            lastTime = timeString;
            textComponent.text = OriginalText.Replace("{systemtime12hr}", timeString);
        }
    }
}
