using UnityEngine;

namespace PlayerList.Utilities
{
    class Constants
    {
        public static GameObject shortcutMenu;
        public static GameObject quickMenu;

        public static void UIInit()
        {
            shortcutMenu = GameObject.Find("UserInterface/QuickMenu/ShortcutMenu");
            quickMenu = QuickMenu.prop_QuickMenu_0.gameObject;
        }
    }
}
