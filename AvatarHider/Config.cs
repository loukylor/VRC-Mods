using System;
using MelonLoader;

namespace AvatarHider
{
    public static class Config
    {
        public static MelonPreferences_Category category = MelonPreferences.CreateCategory("AvatarHider", "Avatar Hider");

        public static MelonPreferences_Entry<bool> HideAvatars;
        public static MelonPreferences_Entry<bool> HideAvatarsCompletely;
        public static MelonPreferences_Entry<bool> IgnoreFriends;
        public static MelonPreferences_Entry<bool> ExcludeShownAvatars;
        public static MelonPreferences_Entry<bool> IncludeHiddenAvatars;
        public static MelonPreferences_Entry<bool> DisableSpawnSound;
        public static MelonPreferences_Entry<float> HideDistance;

        public static event Action OnConfigChanged;
        public static void OnConfigChange()
        {
            OnConfigChanged?.Invoke();
        }

        public static void RegisterSettings()
        {
            

            HideAvatars = category.CreateEntry(nameof(HideAvatars), true, "Hide Avatars");
            HideAvatarsCompletely = category.CreateEntry(nameof(HideAvatarsCompletely), false, "Completely Hide Avatars Rather Than Just Renderers (Lags on avatar show, better avg framerate)");
            IgnoreFriends = category.CreateEntry(nameof(IgnoreFriends), true, "Ignore Friends");
            ExcludeShownAvatars = category.CreateEntry(nameof(ExcludeShownAvatars), true, "Exclude Shown Avatars");
            IncludeHiddenAvatars = category.CreateEntry(nameof(IncludeHiddenAvatars), false, "Include Hidden Avatars");
            DisableSpawnSound = category.CreateEntry(nameof(DisableSpawnSound), false, "Disable Spawn Sounds (Will only do something if \"HideAvatarsCompletely\" is on)");
            HideDistance = category.CreateEntry(nameof(HideDistance), 7.0f, "Distance (meters)");

            foreach (MelonPreferences_Entry entry in category.Entries)
                entry.OnValueChangedUntyped += OnConfigChange;
        }
    }
}
