using System;
using PlayerList.Utilities;
using UIExpansionKit.Components;

namespace PlayerList
{
    class UIXManager
    {
        public static void AddListenerToShortcutMenu(Action onEnable, Action onDisable)
        {
            EnableDisableListener shortcutMenuListener = Constants.shortcutMenu.GetComponent<EnableDisableListener>();
            if (shortcutMenuListener == null)
                shortcutMenuListener = Constants.shortcutMenu.AddComponent<EnableDisableListener>();

            shortcutMenuListener.OnEnabled += onDisable;
            shortcutMenuListener.OnDisabled += onEnable;
        }
    }
}
