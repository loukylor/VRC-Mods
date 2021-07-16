using System;
using System.Collections;
using MelonLoader;
using UnityEngine;
using UserInfoExtensions;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;

namespace UserInfoExtentions
{
    class Utils
    {
        internal static bool _canMakeRequest = true;

        internal static CacheManager.UserInfoExtensionsAPIUser ActiveUIEUser
        {
            get 
            { 
                if (CacheManager.cachedUsers.ContainsKey(VRCUtils.ActiveUserInUserInfoMenu.id))
                    return CacheManager.cachedUsers[VRCUtils.ActiveUserInUserInfoMenu.id];
                return null;
            }
        }

        internal static bool StartRequestTimer(Action tooQuickCallback = null, Action canMakeRequestCallback = null)
        {
            if (_canMakeRequest) // This bool can double as a check for if the coroutine is running
            {
                _canMakeRequest = false;
                MelonCoroutines.Start(GetStartTimerCoroutine(canMakeRequestCallback));
                return true;
            }
            else
            {
                if (tooQuickCallback == null)
                    UiManager.OpenSmallPopup("Slow down!", "Please wait a little in between button presses", "Close", new Action(UiManager.ClosePopup));
                else
                    tooQuickCallback.Invoke();
                return false;
            }
        }
        private static IEnumerator GetStartTimerCoroutine(Action canMakeRequestCallback)
        {
            _canMakeRequest = false;

            float endTime = Time.time + 3.5f;

            while (Time.time < endTime)
            {
                yield return null;
            }

            _canMakeRequest = true;
            canMakeRequestCallback?.Invoke();
            yield break;
        }
    }
}
