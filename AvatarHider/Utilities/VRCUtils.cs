using VRC.Core;
using VRC.Management;

namespace AvatarHider.Utilities
{
    class VRCUtils
    {
        public static bool IsAvatarExplcitlyShown(APIUser user)
        {
            if (ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0.ContainsKey(user.id))
                foreach (ApiPlayerModeration moderation in ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0[user.id])
                    if (moderation.moderationType == ApiPlayerModeration.ModerationType.ShowAvatar)
                        return true;

            return false;
        }
        public static bool IsAvatarExplcitlyHidden(APIUser user)
        {
            if (ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0.ContainsKey(user.id))
                foreach (ApiPlayerModeration moderation in ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0[user.id])
                    if (moderation.moderationType == ApiPlayerModeration.ModerationType.HideAvatar)
                        return true;

            return false;
        }
    }
}
