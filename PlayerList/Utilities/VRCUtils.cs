using System;
using System.Collections;
using Il2CppSystem.Collections.Generic;
using PlayerList.Config;
using PlayerList.Entries;
using MelonLoader;
using UnityEngine;
using VRC.Core;

namespace PlayerList.Utilities
{
    class VRCUtils
    {
        // Completely stolen from Psychloor's PlayerRotator (https://github.com/Psychloor/PlayerRotater)
        public static IEnumerator CheckWorld(ApiWorld world)
        {
            string worldId = world.id;
            // Check if black/whitelisted from EmmVRC - thanks Emilia and the rest of EmmVRC Staff
            WWW www = new WWW($"https://thetrueyoshifan.com/RiskyFuncsCheck.php?worldid={worldId}", null, new Dictionary<string, string>());
            while (!www.isDone)
                yield return new WaitForEndOfFrame();
            string result = www.text?.Trim().ToLower();
            www.Dispose();
            if (!string.IsNullOrWhiteSpace(result))
            { 
                switch (result)
                {
                    case "allowed":
                        MelonLogger.Msg("World allowed to show player distance");
                        PlayerEntry.worldAllowed = true;
                        if (PlayerListConfig.CurrentBaseSortType == EntrySortManager.SortType.Distance)
                            EntrySortManager.currentBaseComparison = EntrySortManager.distanceSort;
                        if (PlayerListConfig.CurrentUpperSortType == EntrySortManager.SortType.Distance)
                            EntrySortManager.currentUpperComparison = EntrySortManager.distanceSort;
                        if (PlayerListConfig.CurrentHighestSortType == EntrySortManager.SortType.Distance)
                            EntrySortManager.currentHighestComparison = EntrySortManager.distanceSort;
                        yield break;

                    case "denied":
                        MelonLogger.Msg("World NOT allowed to show player distance");
                        PlayerEntry.worldAllowed = false;
                        if (PlayerListConfig.CurrentBaseSortType == EntrySortManager.SortType.Distance)
                            EntrySortManager.currentBaseComparison = EntrySortManager.noneSort;
                        if (PlayerListConfig.CurrentUpperSortType == EntrySortManager.SortType.Distance)
                            EntrySortManager.currentUpperComparison = EntrySortManager.noneSort;
                        if (PlayerListConfig.CurrentHighestSortType == EntrySortManager.SortType.Distance)
                            EntrySortManager.currentHighestComparison = EntrySortManager.noneSort;
                        yield break;
                }
            }

            // no result from server or they're currently down
            // Check tags then. should also be in cache as it just got downloaded
            API.Fetch<ApiWorld>(
                worldId,
                new Action<ApiContainer>(
                    container =>
                    {
                        ApiWorld apiWorld;
                        if ((apiWorld = container.Model.TryCast<ApiWorld>()) != null)
                        {
                            foreach (string worldTag in apiWorld.tags)
                            { 
                                if (worldTag.IndexOf("game", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    MelonLogger.Msg("World NOT allowed to show player distance");
                                    PlayerEntry.worldAllowed = false;
                                    return;
                                }
                            }

                            MelonLogger.Msg("World allowed to show player distance");
                            PlayerEntry.worldAllowed = true;
                            return;
                        }
                        else
                        {
                            MelonLogger.Error("Failed to cast ApiModel to ApiWorld");
                        }
                    }),
                disableCache: false);
        }
    }
}
