using System;
using MelonLoader;

namespace AvatarHider
{
    public static class Config
    {
        public const string categoryIdentifier = "AvatarHider";

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
            MelonPreferences.CreateCategory(categoryIdentifier, "Avatar Hider");

            HideAvatars = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(HideAvatars), false, "Hide Avatars");
            HideAvatarsCompletely = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(HideAvatarsCompletely), false, "Completely Hide Avatars Rather Than Just Renderers (Lags on avatar show, better avg framerate)");
            IgnoreFriends = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(IgnoreFriends), true, "Ignore Friends");
            ExcludeShownAvatars = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(ExcludeShownAvatars), true, "Exclude Shown Avatars");
            IncludeHiddenAvatars = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(IncludeHiddenAvatars), false, "Include Shown Avatars");
            DisableSpawnSound = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(DisableSpawnSound), false, "Disable Spawn Sounds (Will only do something if \"HideAvatarsCompletely\" is on)");
            HideDistance = (MelonPreferences_Entry<float>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(HideDistance), 7.0f, "Distance (meters)");

            foreach (MelonPreferences_Entry entry in MelonPreferences.GetCategory(categoryIdentifier).Entries)
                entry.OnValueChangedUntyped += OnConfigChange;
        }
    }
}
