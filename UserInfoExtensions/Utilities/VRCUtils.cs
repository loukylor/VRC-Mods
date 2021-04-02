using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC.Core;
using VRC.UI;

namespace UserInfoExtentions.Utilities
{
    class VRCUtils
    {
        private static MethodInfo popupV2;
        private static MethodInfo popupV1;

        public static MenuController menuController;

        public static PageWorldInfo worldInfo;
        public static PageUserInfo userInfo;
        public static APIUser ActiveUser
        {
            get { return menuController.activeUser; }
        }

        public static void Init()
        {
            popupV2 = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2"));
                        var possibleMethods = typeof(VRCUiPopupManager).GetMethods().Where(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM"));
            popupV1 = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopup"));
        }

        public static void UiInit()
        {
            menuController = QuickMenu.prop_QuickMenu_0.field_Public_MenuController_0;
            worldInfo = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo").GetComponent<PageWorldInfo>();
            userInfo = GameObject.Find("UserInterface/MenuContent/Screens/UserInfo").GetComponent<PageUserInfo>();
        }

        public static void OpenPopupV2(string title, string text, string buttonText, Action onButtonClick)
        {
            popupV2.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[5] { title, text, buttonText, (Il2CppSystem.Action)onButtonClick, null });
        }
        public static void OpenPopupV1(string title, string text, string buttonText, Action onButtonClick)
        {
            popupV1.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[5] { title, text, buttonText, (Il2CppSystem.Action)onButtonClick, null });
        }
        public static void ClosePopup()
        {
            VRCUiPopup popup = VRCUiManager.prop_VRCUiManager_0.field_Internal_VRCUiPopup_0;
            if (popup == null) // If null try grabbing from the screen stack
            {
                foreach (VRCUiPage vrcUIPage in VRCUiManager.prop_VRCUiManager_0.field_Internal_Dictionary_2_String_VRCUiPage_0.Values)
                    popup = vrcUIPage.TryCast<VRCUiPopup>();

                if (popup == null)
                    return;
            }

            popup.Close();
        }
    }
}
