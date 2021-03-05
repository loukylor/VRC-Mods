using System;
using VRC;

namespace PlayerList
{
    // Literally copied from knah's JoinNotifier (https://github.com/knah/VRCMods/tree/master/JoinNotifier)
    class NetworkHooks
    {
        public static event Action<Player> OnPlayerLeave;
        public static event Action<Player> OnPlayerJoin;

        public static void NetworkInit()
        {
            if (NetworkManager.field_Internal_Static_NetworkManager_0 == null) return;

            var field0 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_0;
            var field1 = NetworkManager.field_Internal_Static_NetworkManager_0.field_Internal_VRCEventDelegate_1_Player_1;

            field0.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => OnPlayerJoin?.Invoke(player)));
            field1.field_Private_HashSet_1_UnityAction_1_T_0.Add(new Action<Player>((player) => OnPlayerLeave?.Invoke(player)));
        }
    }
}
