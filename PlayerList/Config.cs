using MelonLoader;
using UnityEngine;

namespace PlayerList
{
    class Config
    {
        // TODO: Make is so the vector 2 acutlaly uses the custom mapper when it gets fixed
        public static readonly string categoryIdentifier = "PlayerList Config";
        public static MelonPreferences_Category category = MelonPreferences.CreateCategory(categoryIdentifier);

        public static MelonPreferences_Entry<bool> enabledOnStart;
        public static MelonPreferences_Entry<bool> condensedText;
        public static MelonPreferences_Entry<int> fontSize;
        public static MelonPreferences_Entry<int> snapToGridSize;

        public static MelonPreferences_Entry<bool> pingToggle;
        public static MelonPreferences_Entry<bool> fpsToggle;
        public static MelonPreferences_Entry<bool> platformToggle;
        public static MelonPreferences_Entry<bool> perfToggle;
        public static MelonPreferences_Entry<bool> displayNameToggle;
        public static bool HasSomethingOff
        {
            get
            {
                if (!pingToggle.Value || !fpsToggle.Value || !platformToggle.Value || !perfToggle.Value || !displayNameToggle.Value)
                    return true;
                return false;
            }
        }

        public static MelonPreferences_Entry<int> playerListMenuButtonPosition;

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
            enabledOnStart = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(enabledOnStart), true, is_hidden: true);
            condensedText = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(condensedText), false, is_hidden: true);
            fontSize = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(fontSize), 35, is_hidden: true);
            snapToGridSize = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(snapToGridSize), 420, is_hidden: true);

            pingToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(pingToggle), true, is_hidden: true);
            fpsToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(fpsToggle), true, is_hidden: true);
            platformToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(platformToggle), true, is_hidden: true);
            perfToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(perfToggle), true, is_hidden: true);
            displayNameToggle = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(displayNameToggle), true, is_hidden: true);

            playerListMenuButtonPosition = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(playerListMenuButtonPosition), 1, is_hidden: true);

            _playerListPositionX = (MelonPreferences_Entry<float>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(_playerListPositionX), 7.5f, is_hidden: true);
            _playerListPositionY = (MelonPreferences_Entry<float>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(_playerListPositionY), 3.5f, is_hidden: true);
            // playerListPosition = (MelonPreferences_Entry<Vector2>)MelonPreferences.CreateEntry(categoryIdentifier, nameof(playerListPosition), (Vector2)Utilities.Converters.ConvertToUnityUnits(new Vector3(7.5f, 3.5f)), is_hidden: true);
        }
    }
}
