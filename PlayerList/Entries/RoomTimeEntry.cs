using System;

namespace PlayerList.Entries
{
    class RoomTimeEntry : EntryBase
    {
        public override string Name { get { return "Room Time"; } }

        public string lastTime;

        protected override void ProcessText(object[] parameters = null)
        {
            TimeSpan time = TimeSpan.FromSeconds(RoomManager.prop_Single_0);
            string timeString = time.ToString(@"hh\:mm\:ss");
            if (lastTime == timeString)
                return;
            
            lastTime = timeString;
            ResetEntry();
            ChangeEntry("roomtime", timeString);
        }
    }
}
