using System;
using MelonLoader;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class SystemTime12HrEntry : EntryBase
    {
        public SystemTime12HrEntry(IntPtr obj0) : base(obj0) { }

        public override string Name { get { return "System Time 12Hr"; } }

        public string lastTime;

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
