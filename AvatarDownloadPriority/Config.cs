using MelonLoader;

namespace AvatarDownloadPriority
{
    class Config
    {
        public const string categoryIdentifier = "AvatarPriorityDownloadingConfig";
        public static MelonPreferences_Entry<int> maxDownloadingAvatarsAtOnce;
        public static MelonPreferences_Entry<int> maxLoadingAvatarsAtOnce;
        public static MelonPreferences_Entry<bool> prioritizeSelf;
        public static MelonPreferences_Entry<bool> prioritizeFavoriteFriends;
        public static MelonPreferences_Entry<bool> prioritizeFriends;
        public static void Init()
        {
            MelonPreferences.CreateCategory(categoryIdentifier, "AvatarPriorityDownloading Config");
            
            maxDownloadingAvatarsAtOnce = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(maxDownloadingAvatarsAtOnce), 15, "Max number of avatars downloading at once");
            maxDownloadingAvatarsAtOnce.OnValueChanged += OnMaxConfigValuesChange;
            maxLoadingAvatarsAtOnce = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(maxLoadingAvatarsAtOnce), 5, "Max number of avatars loading at once");
            maxLoadingAvatarsAtOnce.OnValueChanged += OnMaxConfigValuesChange;
            OnMaxConfigValuesChange(0, 1);

            prioritizeSelf = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(prioritizeSelf), true, "Prioritize downloading your own avatar");
            prioritizeSelf.OnValueChanged += OnSortConfigChange;
            prioritizeFavoriteFriends = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(prioritizeFavoriteFriends), true, "Prioritize downloading favorite friends' avatars");
            prioritizeFavoriteFriends.OnValueChanged += OnSortConfigChange;
            prioritizeFriends = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(prioritizeFriends), true, "Prioritize downloading friends' avatars");
            prioritizeFriends.OnValueChanged += OnSortConfigChange;

            OnSortConfigChange(true, false);
        }

        public static void OnMaxConfigValuesChange(int oldValue, int newValue)
        {
            if (oldValue == newValue)
                return;

            if (maxDownloadingAvatarsAtOnce.Value <= 0)
                maxDownloadingAvatarsAtOnce.Value = 1;
            if (maxLoadingAvatarsAtOnce.Value <= 0)
                maxLoadingAvatarsAtOnce.Value = 1;
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
