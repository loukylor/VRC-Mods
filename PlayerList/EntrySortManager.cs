using System;
using System.Reflection;
using PlayerList.Config;
using PlayerList.Entries;
using PlayerList.Utilities;

namespace PlayerList
{
    class EntrySortManager
    {
        public static int sortStartIndex = 0;

        public static PropertyInfo baseComparisonProperty;
        public static PropertyInfo upperComparisonProperty;
        public static PropertyInfo highestComparisonProperty;
        public static PropertyInfo currentComparisonProperty;

        public static Comparison<PlayerEntry> currentBaseComparison;
        public static Comparison<PlayerEntry> currentUpperComparison;
        public static Comparison<PlayerEntry> currentHighestComparison;
        public static int reverseBase = 1; // 1 = regular, -1 = reverse
        public static int reverseUpper = 1;
        public static int reverseHighest = 1;
        internal static readonly Comparison<PlayerEntry> noneSort = (lEntry, rEntry) =>
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
            return lEntry.player.prop_PlayerNet_0.prop_PhotonView_0.field_Private_Int32_0.CompareTo(rEntry.player.prop_PlayerNet_0.prop_VRCPlayer_0.prop_PhotonView_0.field_Private_Int32_0);
        };
        internal static readonly Comparison<PlayerEntry> distanceSort = (lEntry, rEntry) =>
        {
            return lEntry.distance.CompareTo(rEntry.distance);
        };
        /*private static readonly Comparison<PlayerEntry> fpsSort = (lEntry, rEntry) =>
        {
            return lEntry.fps.CompareTo(rEntry.fps);
        };*/
        private static readonly Comparison<PlayerEntry> friendsSort = (lEntry, rEntry) =>
        {
            if (!lEntry.isFriend && rEntry.isFriend)
                return 1;
            else if (lEntry.isFriend == rEntry.isFriend)
                return 0;
            else
                return -1;
        };
        private static readonly Comparison<PlayerEntry> nameColorSort = (lEntry, rEntry) =>
        {
            return lEntry.playerColor.CompareTo(rEntry.playerColor);
        };
        private static readonly Comparison<PlayerEntry> pingSort = (lEntry, rEntry) =>
        {
            return lEntry.ping.CompareTo(rEntry.ping);
        };

        private static readonly Comparison<PlayerEntry> sort = (lEntry, rEntry) =>
        {
            int comparison = currentHighestComparison(lEntry, rEntry) * reverseHighest;
            if (comparison == 0)
            { 
                int upperComparison = currentUpperComparison(lEntry, rEntry) * reverseUpper;
                if (upperComparison == 0)
                    return currentBaseComparison(lEntry, rEntry) * reverseBase;
                return upperComparison;
            }
            return comparison;
        };

        public static void Init()
        {
            baseComparisonProperty = typeof(PlayerListConfig).GetProperty(nameof(PlayerListConfig.CurrentBaseSortType));
            upperComparisonProperty = typeof(PlayerListConfig).GetProperty(nameof(PlayerListConfig.CurrentUpperSortType));
            highestComparisonProperty = typeof(PlayerListConfig).GetProperty(nameof(PlayerListConfig.CurrentHighestSortType));

            PlayerListConfig.OnConfigChanged += OnStaticConfigChange;
            EntryManager.OnWorldAllowedChanged += OnWorldAllowedChanged;
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
                    if (EntryManager.WorldAllowed)
                        currentBaseComparison = distanceSort;
                    else
                        currentBaseComparison = noneSort;
                    break;
                case SortType.Friends:
                    currentBaseComparison = friendsSort;
                    break;
                case SortType.NameColor:
                    currentBaseComparison = nameColorSort;
                    break;
                case SortType.Ping:
                    currentBaseComparison = pingSort;
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
                    if (EntryManager.WorldAllowed)
                        currentUpperComparison = distanceSort;
                    else
                        currentUpperComparison = noneSort;
                    break;
                case SortType.Friends:
                    currentUpperComparison = friendsSort;
                    break;
                case SortType.NameColor:
                    currentUpperComparison = nameColorSort;
                    break;
                case SortType.Ping:
                    currentUpperComparison = pingSort;
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
                    if (EntryManager.WorldAllowed)
                        currentHighestComparison = distanceSort;
                    else
                        currentHighestComparison = noneSort;
                    break;
                case SortType.Friends:
                    currentHighestComparison = friendsSort;
                    break;
                case SortType.NameColor:
                    currentHighestComparison = nameColorSort;
                    break;
                case SortType.Ping:
                    currentHighestComparison = pingSort;
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

            if (PlayerListConfig.showSelfAtTop.Value)
            {
                if (EntryManager.localPlayerEntry != null)
                { 
                    EntryManager.sortedPlayerEntries.Remove(EntryManager.localPlayerEntry);
                    EntryManager.sortedPlayerEntries.Insert(0, EntryManager.localPlayerEntry);
                    for (int i = 1; i < EntryManager.sortedPlayerEntries.Count; i++)
                    { 
                        EntryManager.sortedPlayerEntries[i].gameObject.transform.SetParent(Constants.playerListLayout.transform.GetChild(i + 2));
                        EntryManager.sortedPlayerEntries[i].gameObject.transform.localPosition = EntryManager.sortedPlayerEntries[i].gameObject.transform.localPosition.SetZ(0);
                    }
                }

                sortStartIndex = 1;
            }
            else
            {
                sortStartIndex = 0;
            }

            SortAllPlayers();
        }
        private static void OnWorldAllowedChanged()
        {
            if (EntryManager.WorldAllowed)
            { 
                if (PlayerListConfig.CurrentBaseSortType == SortType.Distance)
                    currentBaseComparison = distanceSort;
                if (PlayerListConfig.CurrentUpperSortType == SortType.Distance)
                    currentUpperComparison = distanceSort;
                if (PlayerListConfig.CurrentHighestSortType == SortType.Distance)
                    currentHighestComparison = distanceSort;
            }
            else
            {
                if (PlayerListConfig.CurrentBaseSortType == SortType.Distance)
                    currentBaseComparison = noneSort;
                if (PlayerListConfig.CurrentUpperSortType == SortType.Distance)
                    currentUpperComparison = noneSort;
                if (PlayerListConfig.CurrentHighestSortType == SortType.Distance)
                    currentHighestComparison = noneSort;
            }
        }

        public static void SortAllPlayers() // This only runs when sort type is changed and when the qm is opened (takes around 1-2ms in a full BClub)
        {
            EntryManager.sortedPlayerEntries.Sort(sort);

            if (PlayerListConfig.showSelfAtTop.Value)
                if (EntryManager.sortedPlayerEntries.Remove(EntryManager.localPlayerEntry))
                    EntryManager.sortedPlayerEntries.Insert(0, EntryManager.localPlayerEntry);

            for (int i = 0; i < EntryManager.sortedPlayerEntries.Count; i++)
            {
                EntryManager.sortedPlayerEntries[i].gameObject.transform.SetParent(Constants.playerListLayout.transform.GetChild(i + 2));
                EntryManager.sortedPlayerEntries[i].gameObject.transform.localPosition = EntryManager.sortedPlayerEntries[i].gameObject.transform.localPosition.SetZ(0);
            }
        }
        public static void SortPlayer(PlayerEntry sortEntry)
        {
            // This function takes around 0.01 - 0.1 ms in a full BC (excluding garbage collection)
            int oldIndex = EntryManager.sortedPlayerEntries.IndexOf(sortEntry);
            if (oldIndex < 0)
                return;

            int finalIndex = sortStartIndex;
            int sortResult;
            for (int i = sortStartIndex; i < EntryManager.sortedPlayerEntries.Count; i++)
            {
                if (i == oldIndex)
                    continue;

                sortResult = sort(sortEntry, EntryManager.sortedPlayerEntries[i]);
                if (sortResult > 0)
                    finalIndex += sortResult;
                else
                    break;
            }

            EntryManager.sortedPlayerEntries.RemoveAt(oldIndex);
            if (finalIndex < EntryManager.sortedPlayerEntries.Count)
                EntryManager.sortedPlayerEntries.Insert(finalIndex, sortEntry);
            else
                EntryManager.sortedPlayerEntries.Add(sortEntry);

            // Believe it or not but this is faster than changing the text of all the components every sort
            for (int i = Math.Min(finalIndex, oldIndex); i < EntryManager.sortedPlayerEntries.Count; i++)
            { 
                EntryManager.sortedPlayerEntries[i].gameObject.transform.SetParent(Constants.playerListLayout.transform.GetChild(i + 2));
                EntryManager.sortedPlayerEntries[i].gameObject.transform.localPosition = EntryManager.sortedPlayerEntries[i].gameObject.transform.localPosition.SetZ(0);
            }
        }

        public static bool IsSortTypeInUse(SortType sortType)
        {
            return PlayerListConfig.CurrentBaseSortType == sortType || PlayerListConfig.CurrentUpperSortType == sortType || PlayerListConfig.CurrentHighestSortType == sortType;
        }

        public enum SortType
        {
            None,
            Default,
            Alphabetical,
            AvatarPerf,
            Distance,
            Friends,
            NameColor,
            Ping
        }
        public enum FilterType
        {
            NonFriends,
            Friends,
            Self
        }
    }
}
