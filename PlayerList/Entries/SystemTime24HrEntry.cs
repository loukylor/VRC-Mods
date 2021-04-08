using System;

namespace PlayerList.Entries
{
    class SystemTime24HrEntry : EntryBase
    {
        public override string Name { get { return "System Time 24Hr"; } }

        public string lastTime;

        protected override void ProcessText(object[] parameters = null)
        {
            string timeString = DateTime.Now.ToString(@"HH\:mm\:ss");
            if (lastTime == timeString)
                return;

            lastTime = timeString;
            ResetEntry();
            ChangeEntry("systemtime24hr", timeString);
        }
    }
}