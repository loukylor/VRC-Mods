using VRC;

namespace PlayerList.Entries
{
    class InstanceMasterEntry : EntryBase
    {
        public override string Name { get { return "Instance Master"; } }

        public override void ProcessText(object[] parameters = null)
        {
            if (PlayerManager.field_Private_Static_PlayerManager_0 == null) return;

            foreach (Player player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player.prop_VRCPlayerApi_0 == null) return;

                if (player.prop_VRCPlayerApi_0.isMaster)
                {
                    if (player.field_Private_APIUser_0 == null) return;
                    ChangeEntry("instancemaster", player.field_Private_APIUser_0.displayName);
                    return;
                }
            }
        }
    }
}
