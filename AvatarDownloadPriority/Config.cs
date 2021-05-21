using MelonLoader;

namespace AvatarDownloadPriority
{
    class Config
    {
        public static MelonPreferences_Entry<int> maxDownloadingAvatarsAtOnce;
        public static MelonPreferences_Entry<bool> prioritizeSelf;
        public static MelonPreferences_Entry<bool> prioritizeFavoriteFriends;
        public static MelonPreferences_Entry<bool> prioritizeFriends;
        public static void Init()
        {
            MelonPreferences.CreateCategory("AvatarPriorityDownloadingConfig", "AvatarPriorityDownloading Config");
            maxDownloadingAvatarsAtOnce = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry("AvatarPriorityDownloadingConfig", nameof(maxDownloadingAvatarsAtOnce), 5, "Max number of avatars downloading at once");
            maxDownloadingAvatarsAtOnce.OnValueChanged += OnMaxDownloadingAvatarsAtOnceChange;
            prioritizeSelf = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("AvatarPriorityDownloadingConfig", nameof(prioritizeSelf), true, "Prioritize downloading your own avatar");
            prioritizeSelf.OnValueChanged += OnSortConfigChange;
            prioritizeFavoriteFriends = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("AvatarPriorityDownloadingConfig", nameof(prioritizeFavoriteFriends), true, "Prioritize downloading favorite friends' avatars");
            prioritizeFavoriteFriends.OnValueChanged += OnSortConfigChange;
            prioritizeFriends = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("AvatarPriorityDownloadingConfig", nameof(prioritizeFriends), true, "Prioritize downloading friends' avatars");
            prioritizeFriends.OnValueChanged += OnSortConfigChange;
            OnSortConfigChange(true, false);
        }

        public static void OnMaxDownloadingAvatarsAtOnceChange(int oldValue, int newValue)
        {
            if (oldValue == newValue)
                return;

            if (maxDownloadingAvatarsAtOnce.Value <= 0)
                maxDownloadingAvatarsAtOnce.Value = 1;
        }
        public static void OnSortConfigChange(bool oldValue, bool newValue)
        {
            if (oldValue == newValue)
                return;

            AvatarDownloadPriorityMod.sortComparison = null;
            if (prioritizeFavoriteFriends.Value)
                AvatarDownloadPriorityMod.sortComparison = AvatarDownloadPriorityMod.prioritizeFavoriteFriendsSortComparison;
            else if (prioritizeFriends.Value)
                AvatarDownloadPriorityMod.sortComparison = AvatarDownloadPriorityMod.prioritizeFriendsSortComparison;
        }
    }
}
