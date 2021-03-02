using MelonLoader;
using System.Reflection;

namespace UserInfoExtensions
{
    public class UserInfoExtensionsSettings
    {
        private const string catagory = "UserInfoExtensionsSettings";

        public static bool QuickMenuFromSocialButton;
        public static bool AuthorFromSocialMenuButton;
        public static bool AuthorFromAvatarMenuButton;
        public static bool BioButton;
        public static bool BioLinksButton;
        public static bool BioLanguagesButton;
        public static bool OpenUserInBrowserButton;
        public static bool OpenUserWorldInWorldMenu;

        public static void RegisterSettings()
        {
            MelonPreferences.CreateCategory(catagory, "UserInfoExtensions Settings");

            MelonPreferences.CreateEntry(catagory, nameof(QuickMenuFromSocialButton), false, "Show \"To Quick Menu\" button");
            MelonPreferences.CreateEntry(catagory, nameof(AuthorFromSocialMenuButton), false, "Show \"Avatar Author\" button in Social Menu");
            MelonPreferences.CreateEntry(catagory, nameof(AuthorFromAvatarMenuButton), true, "Show \"Avatar Author\" button in Avatar Menu");
            MelonPreferences.CreateEntry(catagory, nameof(BioButton), false, "Show \"Bio\" button");
            MelonPreferences.CreateEntry(catagory, nameof(BioLinksButton), false, "Show \"Bio Links\" button");
            MelonPreferences.CreateEntry(catagory, nameof(BioLanguagesButton), false, "Show \"Bio Languages\" button");
            MelonPreferences.CreateEntry(catagory, nameof(OpenUserInBrowserButton), false, "Show \"Open User in Browser\" button");
            MelonPreferences.CreateEntry(catagory, nameof(OpenUserWorldInWorldMenu), false, "Show\"Open User World in World Menu\" button");

            OnModSettingsApplied();
        }

        public static void OnModSettingsApplied()
        { 
            foreach (FieldInfo fieldInfo in typeof(UserInfoExtensionsSettings).GetFields())
            {
                fieldInfo.SetValue(null, MelonPreferences.GetEntryValue<bool>(catagory, fieldInfo.Name));
            }
        }
    }
}
