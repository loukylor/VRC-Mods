using System;
using PlayerList.Utilities;
using UIExpansionKit.Components;
using UIExpansionKit.API;

namespace PlayerList
{
    class UIXManager
    {
        public static void AddMethodToUIInit()
        {
            ExpansionKitApi.OnUiManagerInit += PlayerListMod.Instance.OnUiManagerInit;
        }

        public static void AddListenerToShortcutMenu(Action onEnable, Action onDisable)
        {
            EnableDisableListener shortcutMenuListener = Constants.shortcutMenu.GetComponent<EnableDisableListener>();
            if (shortcutMenuListener == null)
                shortcutMenuListener = Constants.shortcutMenu.AddComponent<EnableDisableListener>();

            shortcutMenuListener.OnEnabled += onEnable;
            shortcutMenuListener.OnDisabled += onDisable;
        }
    }
}
