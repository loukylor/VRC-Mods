using MelonLoader;
using System.Reflection;

namespace AskToPortal
{
    class AskToPortalSettings
    {
        private const string catagory = "AskToPortalSettings";

        public static bool enabled;
        public static bool autoAcceptFriends;
        public static bool autoAcceptWorld;
        public static bool autoAcceptHome;
        public static bool autoAcceptSelf;
        public static bool onlyPortalDrop;

        public static void RegisterSettings()
        {
            MelonPreferences.CreateCategory(catagory, "AskToPortal Settings");

            MelonPreferences.CreateEntry(catagory, nameof(enabled), true, "Enable/disable the mod");
            MelonPreferences.CreateEntry(catagory, nameof(autoAcceptFriends), false, "Automatically enter friends portals");
            MelonPreferences.CreateEntry(catagory, nameof(autoAcceptWorld), false, "Automatically enter portals that aren't player dropped");
            MelonPreferences.CreateEntry(catagory, nameof(autoAcceptHome), false, "Automatically enter home portals");
            MelonPreferences.CreateEntry(catagory, nameof(autoAcceptSelf), true, "Automatically enter your own portals");
            MelonPreferences.CreateEntry(catagory, nameof(onlyPortalDrop), false, "Only ask if a portal dropper is detected");

            OnModSettingsApplied();
        }

        public static void OnModSettingsApplied()
        {
            foreach (FieldInfo fieldInfo in typeof(AskToPortalSettings).GetFields())
            {
                fieldInfo.SetValue(null, MelonPreferences.GetEntryValue<bool>(catagory, fieldInfo.Name));
            }
        }
    }
}
