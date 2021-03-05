using MelonLoader;

namespace PlayerList
{
    class Config
    {
        public static readonly string categoryIdentifier = "PlayerList Config";
        public static MelonPreferences_Category category = MelonPreferences.CreateCategory(categoryIdentifier);

        public static MelonPreferences_Entry<bool> enabledOnStart;
        public static MelonPreferences_Entry<int> fontSize;

        public static void RegisterSettings()
        {
            enabledOnStart = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(enabledOnStart), true, is_hidden: true);
            fontSize = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(fontSize), 60, is_hidden: true);
        }
    }
}
