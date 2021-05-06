using System;
using AvatarHider.DataTypes;
using UnityEngine;
using VRC;

namespace AvatarHider
{
    public static class RefreshManager
    {
        private static Action<AvatarHiderPlayer, Vector3> refreshDelegate;

        private static readonly Action<AvatarHiderPlayer, Vector3> normalRefreshDelegate = (playerProp, myPos) =>
        {
            float distance = (playerProp.Position - myPos).magnitude;
            if (distance > Config.HideDistance.Value)
                playerProp.SetInActive();
            else
                playerProp.SetActive();
        };

        public static void Init()
        {
            Config.OnConfigChanged += OnConfigChanged;
        }

        private static void OnConfigChanged()
        {
            refreshDelegate = null;

            foreach (AvatarHiderPlayer playerProp in PlayerManager.RefreshFilteredList())
                PlayerManager.HideOrShowAvatar(playerProp);

            if (Config.HideAvatars.Value)
            { 
                refreshDelegate = normalRefreshDelegate;
            }
            else
            { 
                refreshDelegate = null;
                foreach (AvatarHiderPlayer playerProp in PlayerManager.players.Values)
                    playerProp.SetActive();
            }
        }

        public static void Refresh()
        {
            if (Player.prop_Player_0 == null)
                return;

            Vector3 myPos = Player.prop_Player_0.transform.position;
            foreach (AvatarHiderPlayer playerProp in PlayerManager.filteredPlayers.Values)
                RefreshPlayer(playerProp, myPos);
        }
        public static void RefreshPlayer(AvatarHiderPlayer playerProp, Vector3 myPos)
        {
            refreshDelegate?.Invoke(playerProp, myPos);
        }
    }
}
