using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using PlayerList.Config;
using PlayerList.Entries;
using PlayerList.Utilities;
using UnityEngine;

namespace PlayerList
{
    class EntrySortManager
    {
        public static bool shouldSort;

        public static PropertyInfo baseComparisonProperty;
        public static PropertyInfo upperComparisonProperty;
        public static PropertyInfo currentcomparisonProperty;

        public static Comparison<PlayerEntry> currentBaseComparison;
        public static Comparison<PlayerEntry> currentUpperComparison;
        public static int reverseBase = 1; // 1 = regular, -1 = reverse
        public static int reverseUpper = 1;
        private static Comparison<PlayerEntry> alphabeticalSort = (lEntry, rEntry) =>
        {
            return lEntry.player.field_Private_APIUser_0.displayName.CompareTo(rEntry.player.field_Private_APIUser_0.displayName);
        };
        private static Comparison<PlayerEntry> avatarPerfSort = (lEntry, rEntry) =>
        {
            return lEntry.perf.CompareTo(rEntry.perf);
        };
        private static Comparison<PlayerEntry> defaultSort = (lEntry, rEntry) =>
        {
            return lEntry.player.field_Internal_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0.CompareTo(rEntry.player.field_Internal_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0);
        };
        private static Comparison<PlayerEntry> distanceSort = (lEntry, rEntry) =>
        {
            return lEntry.distance.CompareTo(rEntry.distance);
        };
        private static Comparison<PlayerEntry> fpsSort = (lEntry, rEntry) =>
        {
            return lEntry.fps.CompareTo(rEntry.fps);
        };
        private static Comparison<PlayerEntry> friendsSort = (lEntry, rEntry) =>
        {
            if (!lEntry.isFriend && rEntry.isFriend)
                return -1;
            else if (lEntry.isFriend == rEntry.isFriend)
                return 0;
            else
                return 1;
        };
        private static Comparison<PlayerEntry> pingSort = (lEntry, rEntry) =>
        {
            return lEntry.ping.CompareTo(rEntry.ping);
        };

        private static Comparison<PlayerEntry> sort = (lEntry, rEntry) =>
        {
            int comparison = currentUpperComparison(lEntry, rEntry) * reverseUpper;
            if (comparison == 0)
                return currentBaseComparison(lEntry, rEntry) * reverseBase;
            return comparison;
        };

        public static void Init()
        {
            baseComparisonProperty = typeof(PlayerListConfig).GetProperty(nameof(PlayerListConfig.CurrentBaseSortType));
            upperComparisonProperty = typeof(PlayerListConfig).GetProperty(nameof(PlayerListConfig.CurrentUpperSortType));

            PlayerListConfig.OnConfigChangedEvent += OnStaticConfigChange;
        }
        private static void OnStaticConfigChange()
        {
            switch (PlayerListConfig.CurrentBaseSortType)
            {
                case SortType.Alphabetical:
                    currentBaseComparison = alphabeticalSort;
                    break;
                case SortType.AvatarPerf:
                    currentBaseComparison = avatarPerfSort;
                    break;
                case SortType.Distance:
                    currentBaseComparison = distanceSort;
                    break;
                case SortType.Fps:
                    currentBaseComparison = fpsSort;
                    break;
                case SortType.Friends:
                    currentBaseComparison = friendsSort;
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
                case SortType.Alphabetical:
                    currentUpperComparison = alphabeticalSort;
                    break;
                case SortType.AvatarPerf:
                    currentUpperComparison = avatarPerfSort;
                    break;
                case SortType.Distance:
                    currentUpperComparison = distanceSort;
                    break;
                case SortType.Fps:
                    currentUpperComparison = fpsSort;
                    break;
                case SortType.Friends:
                    currentUpperComparison = friendsSort;
                    break;
                case SortType.Ping:
                    currentUpperComparison = pingSort;
                    break;
                default:
                    currentUpperComparison = defaultSort;
                    break;
            }
        }

        public static void SortAllPlayers() // This is slow asf but it shouldn't matter too much
        {
            MelonLogger.Msg("SORTING");

            EntryManager.playerEntriesWithLocal.Sort(sort);

            for (int i = 0; i < EntryManager.playerEntriesWithLocal.Count; i++)
                EntryManager.playerEntriesWithLocal[i].gameObject.transform.SetSiblingIndex(i + 2);
        }
        public static void SortPlayer(PlayerEntry sortEntry)
        {
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
        }

        public enum SortType
        {
            Default,
            Alphabetical,
            AvatarPerf,
            Distance,
            Fps,
            Friends,
            Ping,
            Trust,
        }
        public enum FilterType
        {
            NonFriends,
            Friends,
            Self
        }
    }
}
