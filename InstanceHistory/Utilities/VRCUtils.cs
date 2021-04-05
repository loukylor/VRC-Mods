using System;
using System.Linq;
using System.Reflection;

namespace InstanceHistory.Utilities
{
    class VRCUtils
    {
        private static MethodInfo popupV2;
        private static MethodInfo closePopup;
        public static void Init()
        {
            popupV2 = typeof(VRCUiPopupManager).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2")).First();
            closePopup = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_") && mb.Name.Length <= 21 && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "POPUP"));
        }
        public static void OpenPopupV2(string title, string text, string buttonText, Action onButtonClick)
        {
            popupV2.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[5] { title, text, buttonText, (Il2CppSystem.Action)onButtonClick, null });
        }
        public static void ClosePopup()
        {
            closePopup.Invoke(VRCUiPopupManager.field_Private_Static_VRCUiPopupManager_0, null);
        }
    }
}
