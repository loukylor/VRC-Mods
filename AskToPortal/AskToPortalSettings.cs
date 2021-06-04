using MelonLoader;
using System.Reflection;

namespace AskToPortal
{
    class AskToPortalSettings
    {
        private const string catagory = "AskToPortalSettings";

        public static MelonPreferences_Entry<bool> enabled;
        public static MelonPreferences_Entry<bool> autoAcceptFriends;
        public static MelonPreferences_Entry<bool> autoAcceptWorld;
        public static MelonPreferences_Entry<bool> autoAcceptHome;
        public static MelonPreferences_Entry<bool> autoAcceptSelf;
        public static MelonPreferences_Entry<bool> onlyPortalDrop;
        public static MelonPreferences_Entry<bool> startOnDetailed;

        public static void RegisterSettings()
        {
            MelonPreferences.CreateCategory(catagory, "AskToPortal Settings");

            enabled = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(catagory, nameof(enabled), true, "Enable/disable the mod");
            autoAcceptFriends = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(catagory, nameof(autoAcceptFriends), false, "Automatically enter friends portals");
            autoAcceptWorld = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(catagory, nameof(autoAcceptWorld), false, "Automatically enter portals that aren't player dropped");
            autoAcceptHome = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(catagory, nameof(autoAcceptHome), false, "Automatically enter home portals");
            autoAcceptSelf = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(catagory, nameof(autoAcceptSelf), true, "Automatically enter your own portals");
            onlyPortalDrop = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(catagory, nameof(onlyPortalDrop), false, "Only ask if a portal dropper is detected");
            startOnDetailed = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(catagory, nameof(startOnDetailed), false, "Only ask if a portal dropper is detected");
        }
    }
}
