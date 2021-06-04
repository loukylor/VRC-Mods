using System;
using System.Linq;
using System.Reflection;

namespace AskToPortal.Utilities
{
    class VRCUtils
    {
        private static MethodBase popupV2;
        private static MethodBase popupV2Small;
        private static MethodBase closeMenu;
        private static MethodBase openMenu;

        public static void Init()
        {
            popupV2Small = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2") && Xref.CheckUsed(mb, "OpenSaveSearchPopup"));
            popupV2 = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2"));
            closeMenu = typeof(VRCUiManager).GetMethods()
                 .First(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_Boolean") && Xref.CheckUsed(mb, "ExitStation"));
            openMenu = typeof(VRCUiManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_Boolean") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Backdrop/Backdrop"));
        }
        public static void ClosePopup() => VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP");
        public static void CloseMenu() => closeMenu.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[2] { true, false });
        public static void OpenMenu() => openMenu.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[2] { true, true });
        public static void OpenSmallPopup(string title, string description, string buttonText, Action onButtonClick, Action<VRCUiPopup> additionalSetup = null)
        {
            popupV2Small.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[5] { title, description, buttonText, (Il2CppSystem.Action)onButtonClick, (Il2CppSystem.Action<VRCUiPopup>)additionalSetup });
        }
        public static void OpenPopup(string title, string description, string leftButtonText, Action leftButtonClick, string rightButtonText, Action rightButtonClick, Action<VRCUiPopup> additionalSetup = null)
        {
            popupV2.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[7] { title, description, leftButtonText, (Il2CppSystem.Action)leftButtonClick, rightButtonText, (Il2CppSystem.Action)rightButtonClick, (Il2CppSystem.Action<VRCUiPopup>)additionalSetup });
        }
    }
}
