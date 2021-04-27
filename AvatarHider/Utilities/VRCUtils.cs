using Il2CppSystem.Collections.Generic;
using VRC.Core;
using VRC.Management;

namespace AvatarHider.Utilities
{
    class VRCUtils
    {
        public static bool IsAvatarExplcitlyShown(APIUser user)
        {
            if (typeof(ModerationManager).GetProperty("field_Private_List_1_ApiPlayerModeration_0") != null)
            {
                foreach (ApiPlayerModeration moderation in (List<ApiPlayerModeration>)typeof(ModerationManager).GetProperty("field_Private_List_1_ApiPlayerModeration_0").GetValue(ModerationManager.prop_ModerationManager_0))
                    if (moderation.id == user.id && moderation.moderationType == ApiPlayerModeration.ModerationType.ShowAvatar)
                        return true;
            }
            else
            {
                try
                {
                    foreach (ApiPlayerModeration moderation in ((Dictionary<string, List<ApiPlayerModeration>>)typeof(ModerationManager).GetProperty("field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0").GetValue(ModerationManager.prop_ModerationManager_0))[user.id])
                        if (moderation.moderationType == ApiPlayerModeration.ModerationType.ShowAvatar)
                            return true;
                }
                catch (System.Collections.Generic.KeyNotFoundException) { }
            }
            return false;
        }
        public static bool IsAvatarExplcitlyHidden(APIUser user)
        {
            if (typeof(ModerationManager).GetProperty("field_Private_List_1_ApiPlayerModeration_0") != null)
            {
                foreach (ApiPlayerModeration moderation in (List<ApiPlayerModeration>)typeof(ModerationManager).GetProperty("field_Private_List_1_ApiPlayerModeration_0").GetValue(ModerationManager.prop_ModerationManager_0))
                    if (moderation.id == user.id && moderation.moderationType == ApiPlayerModeration.ModerationType.HideAvatar)
                        return true;
            }
            else
            {
                try
                {
                    foreach (ApiPlayerModeration moderation in ((Dictionary<string, List<ApiPlayerModeration>>)typeof(ModerationManager).GetProperty("field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0").GetValue(ModerationManager.prop_ModerationManager_0))[user.id])
                        if (moderation.moderationType == ApiPlayerModeration.ModerationType.HideAvatar)
                            return true;
                }
                catch (System.Collections.Generic.KeyNotFoundException) { }
            }
            return false;
        }
    }
}
