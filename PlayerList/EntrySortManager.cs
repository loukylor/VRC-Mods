using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayerList.Config;
using PlayerList.Entries;
using VRChatUtilityKit.Utilities;

namespace PlayerList
{
    public class EntrySortManager
    {
        public static int sortStartIndex = 0;

        public static FieldInfo baseComparisonProperty;
        public static FieldInfo upperComparisonProperty;
        public static FieldInfo highestComparisonProperty;
        public static FieldInfo currentComparisonProperty;

        public static Comparison<PlayerEntry> currentBaseComparison;
        public static Comparison<PlayerEntry> currentUpperComparison;
        public static Comparison<PlayerEntry> currentHighestComparison;
        public static int reverseBase = 1; // 1 = regular, -1 = reverse
        public static int reverseUpper = 1;
        public static int reverseHighest = 1;
        private static readonly Comparison<PlayerEntry> alphabeticalSort = (lEntry, rEntry) =>
        {
            return lEntry.player.prop_APIUser_0.displayName.CompareTo(rEntry.player.prop_APIUser_0.displayName);
        };
        private static readonly Comparison<PlayerEntry> avatarPerfSort = (lEntry, rEntry) =>
        {
            return lEntry.perf.CompareTo(rEntry.perf);
        };
        private static readonly Comparison<PlayerEntry> defaultSort = (lEntry, rEntry) =>
        {
            return lEntry.player.prop_PlayerNet_0.prop_PhotonView_0.field_Private_Int32_0.CompareTo(rEntry.player.prop_PlayerNet_0.prop_VRCPlayer_0.prop_PhotonView_0.field_Private_Int32_0);
        };
        private static readonly Comparison<PlayerEntry> distanceSort = (lEntry, rEntry) =>
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
            int currentComparison = 0;
            if (currentHighestComparison != null)
                currentComparison = currentHighestComparison(lEntry, rEntry) * reverseHighest;

            if (currentComparison == 0)
            {
                if (currentUpperComparison != null)
                    currentComparison = currentUpperComparison(lEntry, rEntry) * reverseUpper;

                if (currentComparison == 0 && currentBaseComparison != null)
                    return currentBaseComparison(lEntry, rEntry) * reverseBase;
            }
            return currentComparison;
        };

        public static void Init()
        {
            baseComparisonProperty = typeof(PlayerListConfig).GetField(nameof(PlayerListConfig.currentBaseSort));
            upperComparisonProperty = typeof(PlayerListConfig).GetField(nameof(PlayerListConfig.currentUpperSort));
            highestComparisonProperty = typeof(PlayerListConfig).GetField(nameof(PlayerListConfig.currentHighestSort));

            PlayerListConfig.OnConfigChanged += OnStaticConfigChange;
            VRCUtils.OnEmmWorldCheckCompleted += OnEmmWorldCheckCompleted;
        }
        private static void OnStaticConfigChange()
        {
            switch (PlayerListConfig.currentBaseSort.Value)
            {
                case SortType.None:
                    currentBaseComparison = null;
                    break;
                case SortType.Alphabetical:
                    currentBaseComparison = alphabeticalSort;
                    break;
                case SortType.AvatarPerf:
                    currentBaseComparison = avatarPerfSort;
                    break;
                case SortType.Distance:
                    if (VRCUtils.AreRiskyFunctionsAllowed)
                        currentBaseComparison = distanceSort;
                    else
                        currentBaseComparison = null;
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

            switch (PlayerListConfig.currentUpperSort.Value)
            {
                case SortType.None:
                    currentUpperComparison = null;
                    break;
                case SortType.Alphabetical:
                    currentUpperComparison = alphabeticalSort;
                    break;
                case SortType.AvatarPerf:
                    currentUpperComparison = avatarPerfSort;
                    break;
                case SortType.Distance:
                    if (VRCUtils.AreRiskyFunctionsAllowed)
                        currentUpperComparison = distanceSort;
                    else
                        currentUpperComparison = null;
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

            switch (PlayerListConfig.currentHighestSort.Value)
            {
                case SortType.None:
                    currentHighestComparison = null;
                    break;
                case SortType.Alphabetical:
                    currentHighestComparison = alphabeticalSort;
                    break;
                case SortType.AvatarPerf:
                    currentHighestComparison = avatarPerfSort;
                    break;
                case SortType.Distance:
                    if (VRCUtils.AreRiskyFunctionsAllowed)
                        currentHighestComparison = distanceSort;
                    else
                        currentHighestComparison = null;
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
                if (EntryManager.playerLeftPairsEntries.Count != 0)
                    PlayerLeftPairEntry.SwapPlayerEntries(EntryManager.playerLeftPairsEntries[0], EntryManager.localPlayerEntry.playerLeftPairEntry);

                sortStartIndex = 1;
            }
            else
            {
                sortStartIndex = 0;
            }

            SortAllPlayers();
        }
        private static void OnEmmWorldCheckCompleted(bool areRiskyFuncsAllowed)
        {
            if (areRiskyFuncsAllowed)
            { 
                if (PlayerListConfig.currentBaseSort.Value == SortType.Distance)
                    currentBaseComparison = distanceSort;
                if (PlayerListConfig.currentUpperSort.Value == SortType.Distance)
                    currentUpperComparison = distanceSort;
                if (PlayerListConfig.currentHighestSort.Value == SortType.Distance)
                    currentHighestComparison = distanceSort;
            }
            else
            {
                if (PlayerListConfig.currentBaseSort.Value == SortType.Distance)
                    currentBaseComparison = null;
                if (PlayerListConfig.currentUpperSort.Value == SortType.Distance)
                    currentUpperComparison = null;
                if (PlayerListConfig.currentHighestSort.Value == SortType.Distance)
                    currentHighestComparison = null;
            }
        }

        public static void SortAllPlayers() // This only runs when sort type is changed and when the qm is opened (takes around 1-2ms in a full BClub)
        {
            if (EntryManager.playerLeftPairsEntries.Count == 0)
                return;

            if (IsSortTypeInAllUses(SortType.None))
                return;

            List<PlayerEntry> playerEntries = EntryManager.playerLeftPairsEntries.Select(pairEntry => pairEntry.playerEntry).ToList();

            int num = 0;
            if (PlayerListConfig.showSelfAtTop.Value)
            {
                PlayerLeftPairEntry.SwapPlayerEntries(EntryManager.playerLeftPairsEntries[0], EntryManager.localPlayerEntry.playerLeftPairEntry);
                playerEntries.Remove(EntryManager.localPlayerEntry);
                num = 1;
            }

            playerEntries.Sort(sort);
            for (int i = 0; i < playerEntries.Count; i++)
                EntryManager.playerLeftPairsEntries[i + num].playerEntry = playerEntries[i];
        }
        public static void SortPlayer(PlayerLeftPairEntry sortEntry)
        {
            if (IsSortTypeInAllUses(SortType.None) ||
                (PlayerListConfig.showSelfAtTop.Value && sortEntry.playerEntry.isSelf) ||
                (PlayerListConfig.freezeSortWhenVisible.Value && MenuManager.playerList.active))
                return;

            // This method is good like 0.2ms
            int oldIndex = EntryManager.playerLeftPairsEntries.IndexOf(sortEntry);
            if (oldIndex < 0)
                return;

            int forwardSort = oldIndex < EntryManager.playerLeftPairsEntries.Count - 1 ? sort(sortEntry.playerEntry, EntryManager.playerLeftPairsEntries[oldIndex + 1].playerEntry) : 0;
            int backwardSort = oldIndex > sortStartIndex ? sort(EntryManager.playerLeftPairsEntries[oldIndex - 1].playerEntry, sortEntry.playerEntry) : 0;

            int sortResult;
            if (backwardSort == 1)
            {
                PlayerLeftPairEntry.SwapPlayerEntries(sortEntry, EntryManager.playerLeftPairsEntries[oldIndex - 1]);

                for (int i = oldIndex - 1; i > sortStartIndex; i--)
                {
                    sortResult = sort(EntryManager.playerLeftPairsEntries[i - 1].playerEntry, EntryManager.playerLeftPairsEntries[i].playerEntry);
                    if (sortResult == 1)
                        PlayerLeftPairEntry.SwapPlayerEntries(EntryManager.playerLeftPairsEntries[i], EntryManager.playerLeftPairsEntries[i - 1]);
                    else
                        break;
                }
            }
            else if (forwardSort == 1)
            {
                PlayerLeftPairEntry.SwapPlayerEntries(sortEntry, EntryManager.playerLeftPairsEntries[oldIndex + 1]);

                for (int i = oldIndex + 1; i < EntryManager.playerLeftPairsEntries.Count - 1; i++)
                {
                    sortResult = sort(EntryManager.playerLeftPairsEntries[i].playerEntry, EntryManager.playerLeftPairsEntries[i + 1].playerEntry);
                    if (sortResult == 1)
                        PlayerLeftPairEntry.SwapPlayerEntries(EntryManager.playerLeftPairsEntries[i], EntryManager.playerLeftPairsEntries[i + 1]);
                    else
                        break;
                }
            }
        }

        public static bool IsSortTypeInUse(SortType sortType) => PlayerListConfig.currentBaseSort.Value == sortType || PlayerListConfig.currentUpperSort.Value == sortType || PlayerListConfig.currentHighestSort.Value == sortType;
        public static bool IsSortTypeInAllUses(SortType sortType) => PlayerListConfig.currentBaseSort.Value == sortType && PlayerListConfig.currentUpperSort.Value == sortType && PlayerListConfig.currentHighestSort.Value == sortType;

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
