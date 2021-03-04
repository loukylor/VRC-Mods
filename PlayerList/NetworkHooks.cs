using System;
using VRC;
using VRC.Core;

namespace PlayerList
{
    // Literally copied from knah's JoinNotifier (https://github.com/knah/VRCMods/tree/master/JoinNotifier)
    class NetworkHooks
    {
        private static bool playerHandlerFired;
        private static bool playerHandlerAFiredFirst;

        public static event Action<Player> OnPlayerLeave;
        public static event Action<Player> OnPlayerJoin;

        private static void PlayerHandlerA(Player player)
        {
            if (!playerHandlerFired)
            {
                playerHandlerFired = true;
                playerHandlerAFiredFirst = true;
            }

            if (player == null) return;

            (playerHandlerAFiredFirst ? OnPlayerJoin : OnPlayerLeave)?.Invoke(player);
        }
        private static void PlayerHandlerB(Player player)
        {
            if (!playerHandlerFired)
            {
                playerHandlerFired = true;
                playerHandlerAFiredFirst = false;
            }

            if (player == null) return;

            (playerHandlerAFiredFirst ? OnPlayerLeave : OnPlayerJoin)?.Invoke(player);
        }

        public static void NetworkInit()
        {
            if (NetworkManager.field_Internal_Static_NetworkManager_0 == null) return;

            var field0 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var field1 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;

            field0.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>(PlayerHandlerA));
            field1.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>(PlayerHandlerB));
        }
    }
}
