using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ClickFix.ClickFixMod), "ClickFix", "1.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ClickFix
{
    public class ClickFixMod : MelonMod
    {
        public static GameObject bigMenuBackDrop;
        public static GameObject quickMenuNewElements;

        private static bool hasInitialized = false;

        public override void VRChat_OnUiManagerInit()
        {
            bigMenuBackDrop = GameObject.Find("UserInterface/MenuContent/Backdrop/Backdrop");
            quickMenuNewElements = GameObject.Find("UserInterface/QuickMenu/QuickMenu_NewElements");
            hasInitialized = true;
        }
        public override void OnUpdate()
        {
            if (!hasInitialized) return;

            VRCUiCursor currentCursor = VRCUiCursorManager.Method_Public_Static_VRCUiCursor_0();
            if (currentCursor == null) return;
            
            if (!bigMenuBackDrop.active && quickMenuNewElements.active && currentCursor.gameObject.active && currentCursor.field_Private_VRCInput_0.prop_Boolean_0 && currentCursor.field_Private_VRCPlayer_0 != null && currentCursor.field_Public_EnumNPublicSealedvaNoUiWeUiInPiFlPlOtUnique_0 == VRCUiCursor.EnumNPublicSealedvaNoUiWeUiInPiFlPlOtUnique.Player)
            {
                currentCursor.Method_Public_VRCPlayer_PDM_0(); // Select highlighted player
            }
        }
    }
}
