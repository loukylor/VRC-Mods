using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UserInfoExtensions;
using VRC.Core;
using VRC.UI;

namespace UserInfoExtentions.Utilities
{
    class VRCUtils
    {
        public static bool canMakeRequest = true;

        private static MethodInfo popupV2;
        private static MethodInfo popupV1;

        public static MenuController menuController;

        public static PageWorldInfo worldInfo;
        public static PageUserInfo userInfo;
        public static APIUser ActiveUser
        {
            get { return menuController.activeUser; }
        }
        public static CacheManager.UserInfoExtensionsAPIUser ActiveUIEUser
        {
            get { return CacheManager.cachedUsers[ActiveUser.id]; }
        }

        public static void Init()
        {
            popupV2 = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2") && Xref.CheckUsed(mb, "OpenSaveSearchPopup"));
            popupV1 = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopup") && Xref.CheckUsed(mb, "UpdatePressed"));
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
            VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP");
        }

        public static bool StartRequestTimer(Action tooQuickCallback = null, Action canMakeRequestCallback = null)
        {
            if (canMakeRequest) // This bool can double as a check for if the coroutine is running
            {
                MelonCoroutines.Start(GetStartTimerCoroutine(canMakeRequestCallback));
                return true;
            }
            else
            {
                if (tooQuickCallback == null)
                    OpenPopupV2("Slow down!", "Please wait a little in between button presses", "Close", new Action(ClosePopup));
                else
                    tooQuickCallback.Invoke();
                return false;
            }
        }
        private static IEnumerator GetStartTimerCoroutine(Action canMakeRequestCallback)
        {
            canMakeRequest = false;

            float endTime = Time.time + 3.5f;

            while (Time.time < endTime)
            {
                yield return null;
            }

            canMakeRequest = true;
            canMakeRequestCallback?.Invoke();
            yield break;
        }
    }
}
