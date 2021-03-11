using UnityEngine;

namespace PlayerList
{
    class InputManager
    {
        public static bool IsUseInputPressed
        {
            get
            {
                if (mouseCursor.gameObject.active && mouseCursor.field_Private_VRCInput_0.field_Private_Boolean_0) return true;
                if (rightCursor.gameObject.active && rightCursor.field_Private_VRCInput_0.field_Private_Boolean_0) return true;
                if (leftCursor.gameObject.active && leftCursor.field_Private_VRCInput_0.field_Private_Boolean_0) return true;

                return false;
            }
        }
        public static Vector3 HitPosition
        {
            get
            {
                if (mouseCursor.gameObject.active) return mouseCursor.field_Public_Vector3_0;
                if (rightCursor.gameObject.active) return rightCursor.field_Public_Vector3_0;
                if (leftCursor.gameObject.active) return leftCursor.field_Public_Vector3_0;

                return Vector3.zero;
            }
        }

        public static VRCSpaceUiCursor mouseCursor;
        public static VRCUiCursor rightCursor;
        public static VRCUiCursor leftCursor;

        public static void UiInit()
        {
            mouseCursor = GameObject.Find("_Application/CursorManager/BlueFireballMouse").GetComponent<VRCSpaceUiCursor>();
            rightCursor = GameObject.Find("_Application/CursorManager/RightHandBeam").GetComponent<VRCUiCursor>();
            leftCursor = GameObject.Find("_Application/CursorManager/LeftHandBeam").GetComponent<VRCUiCursor>();
        }
    }
}
