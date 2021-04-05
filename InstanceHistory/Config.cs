using MelonLoader;

namespace InstanceHistory
{
    class Config
    {
        public static MelonPreferences_Category category;
        public static MelonPreferences_Entry<bool> useUIX;
        public static void Init()
        {
            category = MelonPreferences.CreateCategory("InstanceHistory Config");
            useUIX = (MelonPreferences_Entry<bool>)category.CreateEntry("UseUIX", false, "Should use UIX instead of regular buttons");
        }
    }
}
