using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using MelonLoader.Tomlyn.Model;
using PlayerList.Entries;
using UnityEngine;

namespace PlayerList.Config
{
    static class PlayerListConfig
    {
        public static event Action OnConfigChangedEvent;
        private static bool hasConfigChanged = false;

        public static readonly string categoryIdentifier = "PlayerList Config";
        public static MelonPreferences_Category category = MelonPreferences.CreateCategory(categoryIdentifier);
        public static List<EntryWrapper> entries = new List<EntryWrapper>();

        public static EntryWrapper<bool> useTabMenu;
        public static EntryWrapper<bool> enabledOnStart;
        public static EntryWrapper<bool> onlyEnabledInConfig;
        public static EntryWrapper<bool> condensedText;
        public static EntryWrapper<bool> numberedList;
        public static EntryWrapper<int> fontSize;
        public static EntryWrapper<int> snapToGridSize;

        public static EntryWrapper<bool> pingToggle;
        public static EntryWrapper<bool> fpsToggle;
        public static EntryWrapper<bool> platformToggle;
        public static EntryWrapper<bool> perfToggle;
        public static EntryWrapper<bool> distanceToggle;
        public static EntryWrapper<bool> photonIdToggle;
        public static EntryWrapper<bool> displayNameToggle;
        private static EntryWrapper<string> displayNameColorMode;

        public static PlayerEntry.DisplayNameColorMode DisplayNameColorMode
        {
            get { return (PlayerEntry.DisplayNameColorMode)Enum.Parse(typeof(PlayerEntry.DisplayNameColorMode), displayNameColorMode.Value); }
            set { displayNameColorMode.Value = value.ToString(); }
        }

        public static EntryWrapper<bool> reverseBaseSort;
        private static EntryWrapper<string> currentBaseSort;
        public static EntrySortManager.SortType CurrentBaseSortType
        {
            get { return (EntrySortManager.SortType)Enum.Parse(typeof(EntrySortManager.SortType), currentBaseSort.Value); }
            set { currentBaseSort.Value = value.ToString(); }
        }
        public static EntryWrapper<bool> reverseUpperSort;
        private static EntryWrapper<string> currentUpperSort;
        public static EntrySortManager.SortType CurrentUpperSortType
        {
            get { return (EntrySortManager.SortType)Enum.Parse(typeof(EntrySortManager.SortType), currentUpperSort.Value); }
            set { currentUpperSort.Value = value.ToString(); }
        }
        public static EntryWrapper<bool> reverseHighestSort;
        private static EntryWrapper<string> currentHighestSort;
        public static EntrySortManager.SortType CurrentHighestSortType
        {
            get { return (EntrySortManager.SortType)Enum.Parse(typeof(EntrySortManager.SortType), currentHighestSort.Value); }
            set { currentHighestSort.Value = value.ToString(); }
        }
        public static EntryWrapper<bool> ShowSelfAtTop;

        private static EntryWrapper<string> menuButtonPosition;
        public static MenuManager.MenuButtonPositionEnum MenuButtonPosition
        {
            get { return (MenuManager.MenuButtonPositionEnum)Enum.Parse(typeof(MenuManager.MenuButtonPositionEnum), menuButtonPosition.Value); }
            set { menuButtonPosition.Value = value.ToString(); }
        }

        private static EntryWrapper<float> _playerListPositionX;
        private static EntryWrapper<float> _playerListPositionY;
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
            useTabMenu = CreateEntry(categoryIdentifier, nameof(useTabMenu), false, is_hidden: true);
            enabledOnStart = CreateEntry(categoryIdentifier, nameof(enabledOnStart), false, is_hidden: true);
            onlyEnabledInConfig = CreateEntry(categoryIdentifier, nameof(onlyEnabledInConfig), false, is_hidden: true);

            condensedText = CreateEntry(categoryIdentifier, nameof(condensedText), false, is_hidden: true);
            numberedList = CreateEntry(categoryIdentifier, nameof(numberedList), true, is_hidden: true);
            fontSize = CreateEntry(categoryIdentifier, nameof(fontSize), 35, is_hidden: true);
            snapToGridSize = CreateEntry(categoryIdentifier, nameof(snapToGridSize), 420, is_hidden: true);

            pingToggle = CreateEntry(categoryIdentifier, nameof(pingToggle), true, is_hidden: true);
            fpsToggle = CreateEntry(categoryIdentifier, nameof(fpsToggle), true, is_hidden: true);
            platformToggle = CreateEntry(categoryIdentifier, nameof(platformToggle), true, is_hidden: true);
            perfToggle = CreateEntry(categoryIdentifier, nameof(perfToggle), true, is_hidden: true);
            distanceToggle = CreateEntry(categoryIdentifier, nameof(distanceToggle), true, is_hidden: true);
            photonIdToggle = CreateEntry(categoryIdentifier, nameof(photonIdToggle), false, is_hidden: true);
            displayNameToggle = CreateEntry(categoryIdentifier, nameof(displayNameToggle), true, is_hidden: true);

            displayNameColorMode = CreateEntry(categoryIdentifier, nameof(displayNameColorMode), "TrustAndFriends", is_hidden: true);

            reverseBaseSort = CreateEntry(categoryIdentifier, nameof(reverseBaseSort), false, is_hidden: true);
            currentBaseSort = CreateEntry(categoryIdentifier, nameof(currentBaseSort), "None", is_hidden: true);
            reverseUpperSort = CreateEntry(categoryIdentifier, nameof(reverseUpperSort), false, is_hidden: true);
            currentUpperSort = CreateEntry(categoryIdentifier, nameof(currentUpperSort), "None", is_hidden: true);
            reverseHighestSort = CreateEntry(categoryIdentifier, nameof(reverseHighestSort), false, is_hidden: true);
            currentHighestSort = CreateEntry(categoryIdentifier, nameof(currentHighestSort), "None", is_hidden: true);
            ShowSelfAtTop = CreateEntry(categoryIdentifier, nameof(ShowSelfAtTop), true, is_hidden: true);

            menuButtonPosition = CreateEntry(categoryIdentifier, nameof(menuButtonPosition), "TopRight", is_hidden: true);

            _playerListPositionX = CreateEntry(categoryIdentifier, nameof(_playerListPositionX), 7.5f, is_hidden: true);
            _playerListPositionY = CreateEntry(categoryIdentifier, nameof(_playerListPositionY), 3.5f, is_hidden: true);

            foreach (EntryWrapper entry in entries)
                entry.OnValueChangedUntyped += new Action(() => OnConfigChanged());
        }

        public static EntryWrapper<T> CreateEntry<T>(string category_identifier, string entry_identifier, T default_value, string display_name = null, bool is_hidden = false)
        {
            MelonPreferences_Entry<T> melonPref = (MelonPreferences_Entry<T>)MelonPreferences.CreateEntry(category_identifier, entry_identifier, default_value, display_name, is_hidden);
            EntryWrapper<T> entry = new EntryWrapper<T>()
            {
                pref = melonPref
            };
            entries.Add(entry);

            return entry;
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

        public static void OnConfigChanged(bool shouldSetHasConfigChanged = true)
        {
            OnConfigChangedEvent?.Invoke();
            hasConfigChanged = shouldSetHasConfigChanged;
        }

        public static List<T> ReadList<T>(TomlObject value)
        {
            return MelonPreferences.Mapper.ReadArray<T>(value).ToList();
        }
        public static TomlObject WriteList<T>(List<T> value)
        {
            TomlArray array = new TomlArray();

            for (int i = 0; i < value.Count; i++)
                array.Add(value);

            return array;
        }
    }
}
