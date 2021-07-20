using System;
using MelonLoader;

namespace AvatarHider
{
    public static class Config
    {
        public static MelonPreferences_Category category = MelonPreferences.CreateCategory("AvatarHider", "Avatar Hider");

        public static MelonPreferences_Entry<bool> HideAvatars;
        public static MelonPreferences_Entry<bool> IgnoreFriends;
        public static MelonPreferences_Entry<bool> ExcludeShownAvatars;
        public static MelonPreferences_Entry<bool> IncludeHiddenAvatars;
        public static MelonPreferences_Entry<bool> DisableSpawnSound;
        public static MelonPreferences_Entry<bool> LimitAudioDistance;
        public static MelonPreferences_Entry<float> MaxAudioDistance;
        public static MelonPreferences_Entry<float> HideDistance;

        public static event Action OnConfigChanged;
        public static void OnConfigChange()
        {
            OnConfigChanged?.Invoke();
        }

        public static void RegisterSettings()
        {
            HideAvatars = category.CreateEntry(nameof(HideAvatars), true, "Hide Avatars");
            IgnoreFriends = category.CreateEntry(nameof(IgnoreFriends), true, "Ignore Friends");
            ExcludeShownAvatars = category.CreateEntry(nameof(ExcludeShownAvatars), true, "Exclude Shown Avatars");
            IncludeHiddenAvatars = category.CreateEntry(nameof(IncludeHiddenAvatars), false, "Include Hidden Avatars");
            DisableSpawnSound = category.CreateEntry(nameof(DisableSpawnSound), false, "Disable Spawn Sounds");
            LimitAudioDistance = category.CreateEntry(nameof(LimitAudioDistance), true, "Limit Audio Distance (will let audio play once, then never again)");
            MaxAudioDistance = category.CreateEntry(nameof(MaxAudioDistance), 25f, "Max Audio Distance (meters) (ignored if \"Limit Audio Distance\" is off)");
            HideDistance = category.CreateEntry(nameof(HideDistance), 7f, "Distance (meters)");

            foreach (MelonPreferences_Entry entry in category.Entries)
                entry.OnValueChangedUntyped += OnConfigChange;
        }
    }
}
