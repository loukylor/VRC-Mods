using UnityEngine;

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

        public static void SelectPlayer(VRCPlayer player)
        {
            CurrentCursor.Method_Public_Void_VRCPlayer_PDM_0(player);
        }
    }
}
