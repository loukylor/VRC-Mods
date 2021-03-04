using System;
using System.Globalization;

namespace PlayerList.Entries
{
    class SystemTime12HrEntry : EntryBase
    {
        public override void ProcessText(object[] parameters = null) => ChangeEntry("systemtime12hr", DateTime.Now.ToString(@"hh\:mm\:ss tt", CultureInfo.InvariantCulture));
    }
}
