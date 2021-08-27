using MelonLoader;
using UIExpansionKit.API;
using VRChatUtilityKit.Utilities;

namespace PrivateInstanceIcon
{
    public enum InstanceBehavior
    {
        ShowIcon,
        Default,
        HideUsers
    }

    public class Config
    {
        public static InstanceBehavior PrivateInstanceBehavior
        {
            get { return privateInstanceBehavior.Value; }
        }
        public static InstanceBehavior JoinablePrivateInstanceBehavior
        {
            get { return joinablePrivateInstanceBehavior.Value; }
        }
        public static InstanceBehavior FriendsInstanceBehavior
        {
            get { return friendsInstanceBehavior.Value; }
        }
        public static InstanceBehavior FriendsPlusInstanceBehavior
        {
            get { return friendsPlusInstanceBehavior.Value; }
        }
        public static InstanceBehavior PublicInstanceBehavior
        {
            get { return publicInstanceBehavior.Value; }
        }
        public static bool IncludeFavoritesList
        {
            get { return includeFavoritesList.Value; }
        }

        private static readonly string categoryName = "PrivateInstanceIcon Config";
        protected static MelonPreferences_Category category;

        protected static MelonPreferences_Entry<InstanceBehavior> privateInstanceBehavior, joinablePrivateInstanceBehavior, friendsInstanceBehavior, friendsPlusInstanceBehavior, publicInstanceBehavior;
        protected static MelonPreferences_Entry<bool> includeFavoritesList;

        public static void Init()
        {
            category = MelonPreferences.CreateCategory(categoryName);
            includeFavoritesList = category.CreateEntry(nameof(includeFavoritesList), true, "Whether to include the icons and hiding in the friends favorites list.");

            privateInstanceBehavior = CreateInstanceBehaviourMapping("Private", InstanceBehavior.ShowIcon);
            joinablePrivateInstanceBehavior = CreateInstanceBehaviourMapping("Joinable Private", InstanceBehavior.ShowIcon);
            friendsInstanceBehavior = CreateInstanceBehaviourMapping("Friends", InstanceBehavior.Default);
            friendsPlusInstanceBehavior = CreateInstanceBehaviourMapping("Friends+", InstanceBehavior.Default);
            publicInstanceBehavior = CreateInstanceBehaviourMapping("Public", InstanceBehavior.Default);
        }

        internal static MelonPreferences_Entry<InstanceBehavior> CreateInstanceBehaviourMapping(string instanceTypeName, InstanceBehavior defaultVal)
        {
            string prefName = instanceTypeName + " Instances";
            var pref = category.CreateEntry(prefName, defaultVal, $"How the list should behave for {instanceTypeName} instances"); ;
            // The lack of propper enum support in UIX is very very very frustrating.
            // Some other preferences managers actually properly support enums, so storing the actual preference as a string
            // would just make the actually good implementations not be able to be utilitized properly.
            // Thusly this jank double registers the preferences as also strings if UIX is present.
            if (VRCUtils.IsUIXPresent)
            {
                MelonLogger.Msg($"Registering UIX pref for {prefName}");
                var enumPossibleValues = new[] {
                    (nameof(InstanceBehavior.ShowIcon), "Show Icons"),
                    (nameof(InstanceBehavior.Default), "Default"),
                    (nameof(InstanceBehavior.HideUsers), "Hide Users")
                };
                var uixPrefName = prefName + "(UIX)";
                MelonPreferences_Entry<string> uixPref = category.CreateEntry(uixPrefName, pref.Value.ToString(), $"How the list should behave for {instanceTypeName} instances.");
                uixPref.OnValueChanged += (oldValue, newValue) =>
                {
                    if (oldValue != newValue)
                    {
                        bool parsingSuccess = System.Enum.TryParse(newValue, out InstanceBehavior parsed);
                        if (parsingSuccess)
                        {
                            MelonLogger.Msg($"Changed instance type enum for {uixPref.DisplayName}, value = {newValue}");
                            pref.Value = parsed;
                        }
                        else MelonLogger.Warning($"Failed to parse instance type enum for {uixPref.DisplayName}, value = {newValue}");
                    }
                };
                pref.OnValueChanged += (oldValue, newValue) =>
                {
                    if (oldValue != newValue)
                        uixPref.Value = newValue.ToString();
                };
                ExpansionKitApi.RegisterSettingAsStringEnum(categoryName, uixPrefName, enumPossibleValues);
            }

            return pref;
        }
    }

}
