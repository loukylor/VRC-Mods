using System;

namespace PlayerList.Entries
{
    class RoomTimeEntry : EntryBase
    {
        public override string Name { get { return "Room Time"; } }

        protected override void ProcessText(object[] parameters = null)
        {
            TimeSpan time = TimeSpan.FromSeconds(RoomManager.prop_Single_0);
            ChangeEntry("roomtime", time.ToString(@"hh\:mm\:ss"));
        }
    }
}
