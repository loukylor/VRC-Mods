using System;
using System.Collections.Generic;
using System.Linq;
using VRC;
using VRC.Core;
using VRChatUtilityKit.Utilities;

namespace UserHistory
{
    public class UserManager
    {
        public static List<CachedPlayer> cachedPlayers = new List<CachedPlayer>();

        public static void Init()
        {
            NetworkEvents.OnPlayerJoined += OnPlayerJoined;
        }

        private static void OnPlayerJoined(Player player)
        {
            if (player.prop_APIUser_0 == null || (player.prop_APIUser_0.IsSelf && cachedPlayers.Any(user => user.id == APIUser.CurrentUser.id)))
                return;

            for (int i = 0; i < cachedPlayers.Count; i++)
                if ((cachedPlayers[i].timeJoined - DateTime.Now).TotalHours > 0)
                    cachedPlayers[i] = new CachedPlayer(cachedPlayers[i].id, cachedPlayers[i].name, null, cachedPlayers[i].timeJoined);

            cachedPlayers.Insert(0, new CachedPlayer(player.prop_APIUser_0.id, player.prop_APIUser_0.displayName, player.prop_APIUser_0, DateTime.Now));
        }

        public struct CachedPlayer
        {
            public string id;
            public string name;
            public APIUser user;
            public DateTime timeJoined;

            public CachedPlayer(string id, string name, APIUser user, DateTime timeJoined)
            {
                this.id = id;
                this.name = name;
                this.user = user;
                this.timeJoined = timeJoined;
            }
        }
    }
}
