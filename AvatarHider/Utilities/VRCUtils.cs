using Il2CppSystem.Collections.Generic;
using VRC.Core;
using VRC.Management;

namespace AvatarHider.Utilities
{
    class VRCUtils
    {
        public static bool IsAvatarExplcitlyShown(APIUser user)
        {
            try
            {
                if (ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0.ContainsKey(user.id))
                    foreach (ApiPlayerModeration moderation in ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0[user.id])
                        if (moderation.moderationType == ApiPlayerModeration.ModerationType.ShowAvatar)
                            return true;
            }
            catch (System.Collections.Generic.KeyNotFoundException) { }

            return false;
        }
        public static bool IsAvatarExplcitlyHidden(APIUser user)
        {
            try
            {
                if (ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0.ContainsKey(user.id))
                    foreach (ApiPlayerModeration moderation in ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0[user.id])
                        if (moderation.moderationType == ApiPlayerModeration.ModerationType.HideAvatar)
                            return true;
            }
            catch (System.Collections.Generic.KeyNotFoundException) { }

            return false;
        }
    }
}
