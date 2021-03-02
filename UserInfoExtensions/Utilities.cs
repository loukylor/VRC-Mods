using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using VRC.Core;
using VRC.UI;

namespace UserInfoExtentions
{
    class Utilities
    {
        private static MethodInfo popupV2;
        private static MethodInfo popupV1;
        private static MethodInfo closePopup;

        public static ConcurrentQueue<Action> ToMainThreadQueue = new ConcurrentQueue<Action>();

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
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2")).First();
            popupV1 = typeof(VRCUiPopupManager).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopup") && !CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2")).First();
            closePopup = typeof(VRCUiPopupManager).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_") && mb.Name.Length <= 28 && !mb.Name.Contains("PDM") && CheckUsed(mb, "Close")).First();
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
            await YieldToMainThread();
            popupV2.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[5] { title, text, buttonText, (Il2CppSystem.Action)onButtonClick, null });
        }
        public static async void OpenPopupV1(string title, string text, string buttonText, Action onButtonClick)
        {
            await YieldToMainThread();
            popupV1.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[5] { title, text, buttonText, (Il2CppSystem.Action)onButtonClick, null });
        }
        public static async void ClosePopup()
        {
            await YieldToMainThread();
            closePopup.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[] { "POPUP" });
        }

        // This method is practically stolen from https://github.com/BenjaminZehowlt/DynamicBonesSafety/blob/master/DynamicBonesSafetyMod.cs
        public static bool CheckMethod(MethodBase methodBase, string match)
        {
            try
            {
                return UnhollowerRuntimeLib.XrefScans.XrefScanner.XrefScan(methodBase)
                    .Where(instance => instance.Type == UnhollowerRuntimeLib.XrefScans.XrefType.Global && instance.ReadAsObject().ToString().Contains(match)).Any();
            }
            catch { }
            return false;
        }
        public static bool CheckUsed(MethodBase methodBase, string methodName)
        {
            try
            {
                return UnhollowerRuntimeLib.XrefScans.XrefScanner.UsedBy(methodBase)
                    .Where(instance => instance.TryResolve() != null && instance.TryResolve().Name.Contains(methodName)).Any();
            }
            catch { }
            return false;
        }

        // Thank you knah for teaching me this (this is literally his code copied and pasted so)
        public static MainThreadAwaitable YieldToMainThread()
        {
            return new MainThreadAwaitable();
        }

        public struct MainThreadAwaitable : INotifyCompletion
        {
            public bool IsCompleted => false;

            public MainThreadAwaitable GetAwaiter() => this;

            public void GetResult() { }

            public void OnCompleted(Action continuation)
            {
                ToMainThreadQueue.Enqueue(continuation);
            }
        }

    }
    public static class Extensions
    {
        public static Task<TResult> NoAwait<TResult>(this Task<TResult> task)
        {
            task.ContinueWith(tsk =>
            {
                if (tsk.IsFaulted)
                    MelonLoader.MelonLogger.Error($"Free-floating Task failed with exception: {tsk.Exception}");
            });
            return task;
        }
    }
}
