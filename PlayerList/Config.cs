using System;
using MelonLoader;
using PlayerList.Entries;
using UnityEngine;

namespace PlayerList
{
    static class Config
    {
        public static event Action OnConfigChangedEvent;
        private static bool hasConfigChanged;

        // TODO: Make is so the vector 2 acutlaly uses the custom mapper when it gets fixed
        public static readonly string categoryIdentifier = "PlayerList Config";
        public static MelonPreferences_Category category = MelonPreferences.CreateCategory(categoryIdentifier);

        public static MelonPreferences_Entry<bool> enabledOnStart;
        public static MelonPreferences_Entry<bool> condensedText;
        public static MelonPreferences_Entry<bool> numberedList;
        public static MelonPreferences_Entry<int> fontSize;
        public static MelonPreferences_Entry<int> snapToGridSize;

        public static MelonPreferences_Entry<bool> pingToggle;
        public static MelonPreferences_Entry<bool> fpsToggle;
        public static MelonPreferences_Entry<bool> platformToggle;
        public static MelonPreferences_Entry<bool> perfToggle;
        public static MelonPreferences_Entry<bool> distanceToggle;
        public static MelonPreferences_Entry<bool> photonIdToggle;
        public static MelonPreferences_Entry<bool> displayNameToggle;
        private static MelonPreferences_Entry<string> displayNameColorMode;

        public static PlayerEntry.DisplayNameColorMode DisplayNameColorMode
        {
            get { return (PlayerEntry.DisplayNameColorMode)Enum.Parse(typeof(PlayerEntry.DisplayNameColorMode), displayNameColorMode.Value); }
            set { displayNameColorMode.Value = value.ToString(); }
        }

        public static MelonPreferences_Entry<bool> excludeSelfFromSort;
        public static MelonPreferences_Entry<bool> sortFriendsFirst;
        private static MelonPreferences_Entry<string> currentSortType;

        private static MelonPreferences_Entry<string> menuButtonPosition;
        public static MenuManager.MenuButtonPositionEnum MenuButtonPosition
        {
            get { return (MenuManager.MenuButtonPositionEnum)Enum.Parse(typeof(MenuManager.MenuButtonPositionEnum), menuButtonPosition.Value); }
            set { menuButtonPosition.Value = value.ToString(); }
        }

        private static MelonPreferences_Entry<float> _playerListPositionX;
        private static MelonPreferences_Entry<float> _playerListPositionY;
        public static Vector2 PlayerListPosition
        {
            get { return Utilities.Converters.ConvertToUnityUnits(new Vector2(_playerListPositionX.Value, _playerListPositionY.Value)); }
            set 
            {
                Vector2 convertedVector = Utilities.Converters.ConvertToMenuUnits(value);
                _playerListPositionX.Value = convertedVector.x;
                _playerListPositionY.Value = convertedVector.y;
            }
        }

        public static void RegisterSettings()
        {
            // Converting to the generic then mine is required f
            enabledOnStart = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(enabledOnStart), true, is_hidden: true);
            condensedText = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(condensedText), false, is_hidden: true);
            numberedList = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(numberedList), true, is_hidden: true);
            fontSize = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(fontSize), 35, is_hidden: true);
            snapToGridSize = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(snapToGridSize), 420, is_hidden: true);

            pingToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(pingToggle), true, is_hidden: true);
            fpsToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(fpsToggle), true, is_hidden: true);
            platformToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(platformToggle), true, is_hidden: true);
            perfToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(perfToggle), true, is_hidden: true);
            distanceToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(distanceToggle), true, is_hidden: true);
            photonIdToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(photonIdToggle), false, is_hidden: true);
            displayNameToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(displayNameToggle), true, is_hidden: true);
            displayNameColorMode = (MelonPreferences_Entry<string>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(displayNameColorMode), "TrustAndFriends", is_hidden: true);

            menuButtonPosition = (MelonPreferences_Entry<string>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(menuButtonPosition), "TopRight", is_hidden: true);

            _playerListPositionX = (MelonPreferences_Entry<float>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(_playerListPositionX), 7.5f, is_hidden: true);
            _playerListPositionY = (MelonPreferences_Entry<float>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(_playerListPositionY), 3.5f, is_hidden: true);

            foreach (MelonPreferences_Entry entry in category.Entries)
                entry.OnValueChangedUntyped += OnConfigChanged;
        }

        public static void OnConfigChanged()
        {
            OnConfigChangedEvent?.Invoke();
            hasConfigChanged = true;
        }
        public static void SaveEntries()
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 == null) return;

            if (hasConfigChanged)
            {
                MelonPreferences.Save();
                hasConfigChanged = false;
            }

            ListPositionManager.shouldMove = false;
        }
    }
}
