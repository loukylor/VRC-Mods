using System;
using VRC;

namespace PlayerList.Entries
{
    class CoordinatePositionEntry : EntryBase
    {
        public override string Name { get { return "Coordinate Position"; } }
        public override void ProcessText(object[] parameters = null)
        {
            ChangeEntry("x", Math.Round(Player.prop_Player_0.gameObject.transform.position.x, 1));
            ChangeEntry("y", Math.Round(Player.prop_Player_0.gameObject.transform.position.y, 1));
            ChangeEntry("z", Math.Round(Player.prop_Player_0.gameObject.transform.position.z, 1));
        }
    }
}
