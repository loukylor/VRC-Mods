using System;

namespace PlayerList.Entries
{
    class SystemTime12HrEntry : EntryBase
    {
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
