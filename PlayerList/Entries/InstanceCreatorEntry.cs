using System;
using VRC;
using VRC.Core;

namespace PlayerList.Entries
{
    class InstanceCreatorEntry : EntryBase
    {
        public override string Name { get { return "Instance Creator"; } }

        public bool hasCheckedInstance;
        public string lastUserDisplayName;
        protected override void ProcessText(object[] parameters = null)
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 != null)
            {
                if (hasCheckedInstance)
                {
                    ChangeEntry("instancecreator", lastUserDisplayName);
                    return;
                }

                Il2CppSystem.Collections.Generic.List<ApiWorldInstance.InstanceTag> tags = RoomManager.field_Internal_Static_ApiWorldInstance_0.ParseTags(RoomManager.field_Internal_Static_ApiWorldInstance_0.idWithTags);
                foreach (ApiWorldInstance.InstanceTag tag in tags)
                {
                    if (tag.name == "private" || tag.name == "friend" || tag.name == "hidden")
                    {
                        foreach (Player player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0)
                        {
                            if (player.field_Private_APIUser_0.id == tag.data)
                            {
                                ChangeEntry("instancecreator", player.field_Private_APIUser_0.displayName);
                                lastUserDisplayName = player.field_Private_APIUser_0.displayName;
                                hasCheckedInstance = true;
                                return;
                            }
                        }

                        APIUser.FetchUser(tag.data, new Action<APIUser>(OnIdReceived), null);
                        ChangeEntry("instancecreator", "Loading...");
                        hasCheckedInstance = true;
                        return;
                    }
                }
            }
            lastUserDisplayName = "No Instance Creator";
            ChangeEntry("instancecreator", "No Instance Creator");
            hasCheckedInstance = true;
        }
        public override void OnSceneWasLoaded()
        {
            hasCheckedInstance = false;
        }
        public void OnIdReceived(APIUser user)
        {
            MelonLoader.MelonLogger.Msg("user fetched");
            lastUserDisplayName = user.displayName;
        }
    }
}
