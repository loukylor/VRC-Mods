using System;
using System.Reflection;
using MelonLoader;
using PlayerList.Config;
using PlayerList.Entries;

namespace PlayerList
{
    class EntrySortManager
    {
        public static PropertyInfo baseComparisonProperty;
        public static PropertyInfo upperComparisonProperty;
        public static PropertyInfo highestComparisonProperty;
        public static PropertyInfo currentcomparisonProperty;

        public static Comparison<PlayerEntry> currentBaseComparison;
        public static Comparison<PlayerEntry> currentUpperComparison;
        public static Comparison<PlayerEntry> currentHighestComparison;
        public static int reverseBase = 1; // 1 = regular, -1 = reverse
        public static int reverseUpper = 1;
        public static int reverseHighest = 1;
        private static readonly Comparison<PlayerEntry> noneSort = (lEntry, rEntry) =>
        {
            return 0;
        };
        private static readonly Comparison<PlayerEntry> alphabeticalSort = (lEntry, rEntry) =>
        {
            return lEntry.player.field_Private_APIUser_0.displayName.CompareTo(rEntry.player.field_Private_APIUser_0.displayName);
        };
        private static readonly Comparison<PlayerEntry> avatarPerfSort = (lEntry, rEntry) =>
        {
            return lEntry.perf.CompareTo(rEntry.perf);
        };
        private static readonly Comparison<PlayerEntry> defaultSort = (lEntry, rEntry) =>
        {
            return lEntry.player.field_Internal_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0.CompareTo(rEntry.player.field_Internal_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0);
        };
        private static readonly Comparison<PlayerEntry> distanceSort = (lEntry, rEntry) =>
        {
            return lEntry.distance.CompareTo(rEntry.distance);
        };
        private static readonly Comparison<PlayerEntry> fpsSort = (lEntry, rEntry) =>
        {
            return lEntry.fps.CompareTo(rEntry.fps);
        };
        private static readonly Comparison<PlayerEntry> friendsSort = (lEntry, rEntry) =>
        {
            if (!lEntry.isFriend && rEntry.isFriend)
                return -1;
            else if (lEntry.isFriend == rEntry.isFriend)
                return 0;
            else
                return 1;
        };
        private static readonly Comparison<PlayerEntry> pingSort = (lEntry, rEntry) =>
        {
            return lEntry.ping.CompareTo(rEntry.ping);
        };
        private static readonly Comparison<PlayerEntry> nameColor = (lEntry, rEntry) =>
        {
            return lEntry.playerColor.CompareTo(rEntry.playerColor);
        };

        private static readonly Comparison<PlayerEntry> sort = (lEntry, rEntry) =>
        {
            int comparison = currentHighestComparison(lEntry, rEntry) * reverseHighest;
            if (comparison == 0)
            { 
                int upperComparison = currentUpperComparison(lEntry, rEntry) * reverseUpper;
                if (upperComparison == 0)
                    return currentBaseComparison(lEntry, rEntry) * reverseHighest;
                return upperComparison;
            }
            return comparison;
        };

        public static void Init()
        {
            baseComparisonProperty = typeof(PlayerListConfig).GetProperty(nameof(PlayerListConfig.CurrentBaseSortType));
            upperComparisonProperty = typeof(PlayerListConfig).GetProperty(nameof(PlayerListConfig.CurrentUpperSortType));
            highestComparisonProperty = typeof(PlayerListConfig).GetProperty(nameof(PlayerListConfig.CurrentHighestSortType));

            PlayerListConfig.OnConfigChangedEvent += OnStaticConfigChange;
        }
        private static void OnStaticConfigChange()
        {
            switch (PlayerListConfig.CurrentBaseSortType)
            {
                case SortType.None:
                    currentBaseComparison = noneSort;
                    break;
                case SortType.Alphabetical:
                    currentBaseComparison = alphabeticalSort;
                    break;
                case SortType.AvatarPerf:
                    currentBaseComparison = avatarPerfSort;
                    break;
                case SortType.Distance:
                    currentBaseComparison = distanceSort;
                    break;
                case SortType.Friends:
                    currentBaseComparison = friendsSort;
                    break;
                case SortType.NameColor:
                    currentBaseComparison = nameColor;
                    break;
                default:
                    currentBaseComparison = defaultSort;
                    break;
            }

            switch (PlayerListConfig.CurrentUpperSortType)
            {
                case SortType.None:
                    currentUpperComparison = noneSort;
                    break;
                case SortType.Alphabetical:
                    currentUpperComparison = alphabeticalSort;
                    break;
                case SortType.AvatarPerf:
                    currentUpperComparison = avatarPerfSort;
                    break;
                case SortType.Distance:
                    currentUpperComparison = distanceSort;
                    break;
                case SortType.Friends:
                    currentUpperComparison = friendsSort;
                    break;
                case SortType.NameColor:
                    currentUpperComparison = nameColor;
                    break;
                default:
                    currentUpperComparison = defaultSort;
                    break;
            }

            switch (PlayerListConfig.CurrentHighestSortType)
            {
                case SortType.None:
                    currentHighestComparison = noneSort;
                    break;
                case SortType.Alphabetical:
                    currentHighestComparison = alphabeticalSort;
                    break;
                case SortType.AvatarPerf:
                    currentHighestComparison = avatarPerfSort;
                    break;
                case SortType.Distance:
                    currentHighestComparison = distanceSort;
                    break;
                case SortType.Friends:
                    currentHighestComparison = friendsSort;
                    break;
                case SortType.NameColor:
                    currentHighestComparison = nameColor;
                    break;
                default:
                    currentHighestComparison = defaultSort;
                    break;
            }

            if (PlayerListConfig.reverseBaseSort.Value)
                reverseBase = -1;
            else
                reverseBase = 1;

            if (PlayerListConfig.reverseUpperSort.Value)
                reverseUpper = -1;
            else
                reverseUpper = 1;

            if (PlayerListConfig.reverseHighestSort.Value)
                reverseHighest = -1;
            else
                reverseHighest = 1;

            SortAllPlayers();
        }

        public static void SortAllPlayers() // This is slow asf but it shouldn't matter too much
        {
            EntryManager.playerEntriesWithLocal.Sort(sort);

            for (int i = 0; i < EntryManager.playerEntriesWithLocal.Count; i++)
                EntryManager.playerEntriesWithLocal[i].gameObject.transform.SetSiblingIndex(i + 2);

            if (PlayerListConfig.numberedList.Value)
                foreach (PlayerEntry entry in EntryManager.playerEntriesWithLocal)
                    entry.textComponent.text = entry.CalculateLeftPart() + entry.withoutLeftPart;
        }
        public static void SortPlayer(PlayerEntry sortEntry)
        {
            int oldIndex = EntryManager.playerEntriesWithLocal.IndexOf(sortEntry);
            if (oldIndex < 0)
                oldIndex = int.MaxValue;
            else
                EntryManager.playerEntriesWithLocal.RemoveAt(oldIndex);

            int finalIndex = 0;
            for (int i = 0; i < EntryManager.playerEntriesWithLocal.Count; i++)
            {
                int sortResult = sort(sortEntry, EntryManager.playerEntriesWithLocal[i]);
                if (sortResult >= 0)
                    finalIndex += sortResult;
                else
                    break;
            }

            EntryManager.playerEntriesWithLocal.Insert(finalIndex, sortEntry);
            sortEntry.gameObject.transform.SetSiblingIndex(finalIndex + 2);

            // Refresh count on numbered thingys that have changed
            if (PlayerListConfig.numberedList.Value)
                if (EntryManager.playerEntries.Count.ToString().Length != EntryManager.playerEntriesWithLocal.Count.ToString().Length)
                    for (int i = 0; i < EntryManager.playerEntriesWithLocal.Count; i++)
                        EntryManager.playerEntriesWithLocal[i].textComponent.text = EntryManager.playerEntriesWithLocal[i].CalculateLeftPart() + EntryManager.playerEntriesWithLocal[i].withoutLeftPart;
                else
                    for (int i = Math.Min(oldIndex, finalIndex); i < EntryManager.playerEntriesWithLocal.Count; i ++)
                        EntryManager.playerEntriesWithLocal[i].textComponent.text = EntryManager.playerEntriesWithLocal[i].CalculateLeftPart() + EntryManager.playerEntriesWithLocal[i].withoutLeftPart;
        }

        public enum SortType
        {
            None,
            Default,
            Alphabetical,
            AvatarPerf,
            Distance,
            Friends,
            NameColor
        }
        public enum FilterType
        {
            NonFriends,
            Friends,
            Self
        }
    }
}
