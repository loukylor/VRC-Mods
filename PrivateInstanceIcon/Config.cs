using System;
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
            //get { return privateInstanceBehavior.Value; }
            get { return (InstanceBehavior)Enum.Parse(typeof(InstanceBehavior), privateInstanceBehavior.Value); }
        }
        public static InstanceBehavior JoinablePrivateInstanceBehavior
        {
            //get { return joinablePrivateInstanceBehavior.Value; }
            get { return (InstanceBehavior)Enum.Parse(typeof(InstanceBehavior), joinablePrivateInstanceBehavior.Value); }
        }
        public static InstanceBehavior FriendsInstanceBehavior
        {
            //get { return friendsInstanceBehavior.Value; }
            get { return (InstanceBehavior)Enum.Parse(typeof(InstanceBehavior), friendsInstanceBehavior.Value); }
        }
        public static InstanceBehavior FriendsPlusInstanceBehavior
        {
            //get { return friendsPlusInstanceBehavior.Value; }
            get { return (InstanceBehavior)Enum.Parse(typeof(InstanceBehavior), friendsPlusInstanceBehavior.Value); }
        }
        public static InstanceBehavior PublicInstanceBehavior
        {
            //get { return publicInstanceBehavior.Value; }
            get { return (InstanceBehavior)Enum.Parse(typeof(InstanceBehavior), publicInstanceBehavior.Value); }
        }
        public static bool IncludeFavoritesList
        {
            get { return includeFavoritesList.Value; }
        }

        private static readonly string categoryName = "PrivateInstanceIcon Config";
        protected static MelonPreferences_Category category;

        // TODO: Once UIX gets proper enum support change this to MelonPreferences_Entry<InstanceBehavior>
        protected static MelonPreferences_Entry<string> privateInstanceBehavior, joinablePrivateInstanceBehavior, friendsInstanceBehavior, friendsPlusInstanceBehavior, publicInstanceBehavior;
        protected static MelonPreferences_Entry<bool> includeFavoritesList;

        public static bool isAnyTypeHidden;

        public static void Init()
        {
            category = MelonPreferences.CreateCategory(categoryName);
            includeFavoritesList = category.CreateEntry(nameof(includeFavoritesList), true, "Whether to include the icons and hiding in the friends favorites list.");

            privateInstanceBehavior = CreateInstanceBehaviourMapping("Private", InstanceBehavior.ShowIcon);
            joinablePrivateInstanceBehavior = CreateInstanceBehaviourMapping("Joinable Private", InstanceBehavior.Default);
            friendsInstanceBehavior = CreateInstanceBehaviourMapping("Friends", InstanceBehavior.Default);
            friendsPlusInstanceBehavior = CreateInstanceBehaviourMapping("Friends+", InstanceBehavior.Default);
            publicInstanceBehavior = CreateInstanceBehaviourMapping("Public", InstanceBehavior.Default);

            OnBehaviorChange();
            foreach (MelonPreferences_Entry entry in category.Entries)
                if (entry is MelonPreferences_Entry<string> behavior)
                    behavior.OnValueChangedUntyped += OnBehaviorChange;
        }

        private static void OnBehaviorChange()
        {
            isAnyTypeHidden = PrivateInstanceBehavior == InstanceBehavior.HideUsers
                || JoinablePrivateInstanceBehavior == InstanceBehavior.HideUsers
                || FriendsInstanceBehavior == InstanceBehavior.HideUsers
                || FriendsPlusInstanceBehavior == InstanceBehavior.HideUsers
                || PublicInstanceBehavior == InstanceBehavior.HideUsers;
        }

        internal static MelonPreferences_Entry<string> CreateInstanceBehaviourMapping(string instanceTypeName, InstanceBehavior defaultVal)
        {
            string prefName = instanceTypeName + " Instances";
            var pref = category.CreateEntry(prefName, defaultVal.ToString(), $"How the list should behave for {instanceTypeName} instances"); ;
            if (VRCUtils.IsUIXPresent)
            {
                var enumPossibleValues = new[] {
                    (nameof(InstanceBehavior.ShowIcon), "Show Icons"),
                    (nameof(InstanceBehavior.Default), "Default"),
                    (nameof(InstanceBehavior.HideUsers), "Hide Users")
                };
                ExpansionKitApi.RegisterSettingAsStringEnum(categoryName, prefName, enumPossibleValues);
            }

            return pref;
        }
    }

}
