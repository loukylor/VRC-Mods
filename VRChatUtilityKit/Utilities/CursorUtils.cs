using UnityEngine;

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// A set of utilities pertaining to VRChat's cursor
    /// </summary>
    public static class CursorUtils
    {
        /// <summary>
        /// Returns the current cursor being used by VRChat.
        /// </summary>
        public static VRCUiCursor CurrentCursor
        {
            get { return VRCUiCursorManager.Method_Public_Static_VRCUiCursor_0(); }
        }

        /// <summary>
        /// Returns whether the use input is being pressed on the current cursor.
        /// </summary>
        public static bool IsUseInputPressed => CurrentCursor.field_Private_VRCInput_0.field_Private_Boolean_0 && CurrentCursor.gameObject.active;

        /// <summary>
        /// Returns where the raycast of the current cursor is colliding.
        /// </summary>
        public static Vector3 HitPosition
        {
            get
            {
                if (CurrentCursor.gameObject.active)
                    return CurrentCursor.field_Public_Vector3_0;

                return Vector3.zero;
            }
        }
    }
}
