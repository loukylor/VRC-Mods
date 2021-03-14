using System;
using VRC.Core;

namespace PlayerList.Entries
{
    class InstanceCreatorEntry : EntryBase
    {
        public override string Name { get { return "Instance Creator"; } }

        public string creatorTag;
        public string lastUserDisplayName;
        protected override void ProcessText(object[] parameters = null)
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 != null)
            {
                Il2CppSystem.Collections.Generic.List<ApiWorldInstance.InstanceTag> tags = RoomManager.field_Internal_Static_ApiWorldInstance_0.ParseTags(RoomManager.field_Internal_Static_ApiWorldInstance_0.idWithTags);
                foreach (ApiWorldInstance.InstanceTag tag in tags)
                {
                    if (tag.name == "private" || tag.name == "friend" || tag.name == "hidden")
                    {
                        if (creatorTag == tag.data)
                        {
                            ChangeEntry("instancecreator", lastUserDisplayName);
                            return;
                        }

                        if (tag.data == APIUser.CurrentUser.id)
                        {
                            ChangeEntry("instancecreator", APIUser.CurrentUser.displayName);
                            lastUserDisplayName = APIUser.CurrentUser.displayName;
                        }
                        else
                        {
                            APIUser.FetchUser(tag.data, new Action<APIUser>(OnIdReceived), null);
                            ChangeEntry("instancecreator", "Loading...");
                        }

                        creatorTag = tag.data;
                        return;
                    }
                }
            }

            ChangeEntry("instancecreator", "No Instance Creator");
            creatorTag = null;
        }
        public void OnIdReceived(APIUser user)
        {
            lastUserDisplayName = user.displayName;
            MelonLoader.MelonLogger.Msg("User fetched");
        }
    }
}
