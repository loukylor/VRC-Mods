using UnityEngine;
using UnityEngine.UI;

namespace PlayerList.Utilities
{
    class Constants
    {
        public static GameObject shortcutMenu;
        public static GameObject quickMenu;
        public static VerticalLayoutGroup playerListLayout;
        public static VerticalLayoutGroup generalInfoLayout;
        public static Vector2 quickMenuColliderSize;

        public static void UIInit()
        {
            shortcutMenu = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu");
            quickMenu = QuickMenu.prop_QuickMenu_0.gameObject;
        }

        public static void OnSceneWasLoaded()
        {
            if (quickMenuColliderSize != null)
            {
                quickMenuColliderSize = quickMenu.GetComponent<BoxCollider>().size;
                ListPositionManager.CombineQMColliderAndPlayerListRect();
            }
        }
    }
}
