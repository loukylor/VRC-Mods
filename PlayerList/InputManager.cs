using PlayerList.UI;
using UnityEngine;
using VRC;

namespace PlayerList
{
    class InputManager
    {
        public static VRCUiCursor CurrentCursor
        {
            get { return VRCUiCursorManager.Method_Public_Static_VRCUiCursor_0(); }
        }
        public static bool IsUseInputPressed
        {
            get
            {
                return CurrentCursor.field_Private_VRCInput_0.field_Private_Boolean_0 && CurrentCursor.gameObject.active;
            }
        }
        public static Vector3 HitPosition
        {
            get
            {
                if (CurrentCursor.gameObject.active)
                    return CurrentCursor.field_Public_Vector3_0;

                return Vector3.zero;
            }
        }

        public static void SelectPlayer(Player player)
        {
            QuickMenu.prop_QuickMenu_0.field_Private_Player_0 = player;
            QuickMenu.prop_QuickMenu_0.prop_APIUser_0 = player.prop_APIUser_0;
            UIManager.SetMenuIndex(3);
        }
    }
}
