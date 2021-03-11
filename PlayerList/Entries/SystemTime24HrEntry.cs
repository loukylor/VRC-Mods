using System;

namespace PlayerList.Entries
{
    class SystemTime24HrEntry : EntryBase
    {
        public override string Name { get { return "System Time 24Hr"; } }

        public override void ProcessText(object[] parameters = null) => ChangeEntry("systemtime24hr", DateTime.Now.ToString(@"HH\:mm\:ss"));
    }
}