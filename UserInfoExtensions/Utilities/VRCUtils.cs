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
        private static MethodInfo closePopup;

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
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2")).First();
            popupV1 = typeof(VRCUiPopupManager).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopup") && !Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2")).First();
            closePopup = typeof(VRCUiPopupManager).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_") && mb.Name.Length <= 28 && !mb.Name.Contains("PDM") && Xref.CheckUsed(mb, "Close")).First();
        }

        public static void UiInit()
        {
            menuController = QuickMenu.prop_QuickMenu_0.field_Public_MenuController_0;
            worldInfo = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo").GetComponent<PageWorldInfo>();
            userInfo = GameObject.Find("UserInterface/MenuContent/Screens/UserInfo").GetComponent<PageUserInfo>();
        }

        public static void TryExecuteMethod(MethodInfo method, object instance = null, object[] parameters = null)
        {
            try
            {
                method.Invoke(instance, parameters);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error while trying to run method \"{method.Name}\" in module \"{method.DeclaringType}:\"\n" + ex.ToString());
            }
        }

        public static async void OpenPopupV2(string title, string text, string buttonText, Action onButtonClick)
        {
            await AsyncUtils.YieldToMainThread();
            popupV2.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[5] { title, text, buttonText, (Il2CppSystem.Action)onButtonClick, null });
        }
        public static async void OpenPopupV1(string title, string text, string buttonText, Action onButtonClick)
        {
            await AsyncUtils.YieldToMainThread();
            popupV1.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[5] { title, text, buttonText, (Il2CppSystem.Action)onButtonClick, null });
        }
        public static async void ClosePopup()
        {
            await AsyncUtils.YieldToMainThread();
            closePopup.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[] { "POPUP" });
        }
    }
}
