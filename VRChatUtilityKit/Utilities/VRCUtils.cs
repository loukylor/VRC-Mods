using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.Management;
using VRC.UI;

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// A set of utilities pertaining to VRChat itself.
    /// </summary>
    public static class VRCUtils
    {
        /// <summary>
        /// Calls when the VRChat UiManager is initialized
        /// </summary>
        public static Action OnUiManagerInit;

        /// <summary>
        /// Calls when the emm world check finishes.
        /// </summary>
        public static Action<bool> OnEmmWorldCheckCompleted;

        /// <summary>
        /// Returns whether risky functions are allowed in the current world. 
        /// It is recommended to use OnEmmWorldCheck over this property.
        /// </summary>
        public static bool AreRiskyFunctionsAllowed 
        {
            get => _areRiskyFunctionsAllowed;
            private set { _areRiskyFunctionsAllowed = value; OnEmmWorldCheckCompleted?.DelegateSafeInvoke(value); } 
        }
        private static bool _areRiskyFunctionsAllowed;

        /// <summary>
        /// Returns whether UIExpansionKit is loaded.
        /// </summary>
        public static bool IsUIXPresent => MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit"));

        /// <summary>
        /// Returns the instance of the MenuController.
        /// </summary>
        public static MenuController MenuControllerInstance { get; private set; }

        /// <summary>
        /// Returns the instance of the WorldInfo component.
        /// </summary>
        public static PageWorldInfo WorldInfoInstance { get; private set; }
        /// <summary>
        /// Returns the instance of the UserInfo component.
        /// </summary>
        public static PageUserInfo UserInfoInstance { get; private set; }

        /// <summary>
        /// Returns the active user in the user info menu.
        /// </summary>
        public static APIUser ActiveUserInUserInfoMenu => MenuControllerInstance.activeUser;
        /// <summary>
        /// Returns the active user in the QuickMenu.
        /// </summary>
        public static APIUser ActiveUserInQuickMenu => QuickMenu.prop_QuickMenu_0.prop_APIUser_0;
        /// <summary>
        /// Returns the active player in the QuickMenu.
        /// </summary>
        public static Player ActivePlayerInQuickMenu => QuickMenu.prop_QuickMenu_0.field_Private_Player_0; 

        private static MethodInfo _reloadAvatarMethod;
        private static MethodInfo _reloadAllAvatarsMethod;

        internal static void Init()
        {
            NetworkEvents.OnInstanceChanged += new Action<ApiWorld, ApiWorldInstance>((world, instance) => MelonCoroutines.Start(StartEmmCheck(world)));
            MelonCoroutines.Start(UiInitCoroutine());

            _reloadAvatarMethod = typeof(VRCPlayer).GetMethods().First(mi => mi.Name.StartsWith("Method_Private_Void_Boolean_") && mi.Name.Length < 31 && mi.GetParameters().Any(pi => pi.IsOptional));
            _reloadAllAvatarsMethod = typeof(VRCPlayer).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Void_Boolean_") && mi.Name.Length < 30 && mi.GetParameters().Any(pi => pi.IsOptional));// Both methods seem to do the same thing;
        }

        private static IEnumerator UiInitCoroutine()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null)
                yield return null;

            OnUiManagerInit.Invoke();
        }

        internal static void UiInit()
        {
            MenuControllerInstance = QuickMenu.prop_QuickMenu_0.field_Public_MenuController_0;
            WorldInfoInstance = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo").GetComponent<PageWorldInfo>();
            UserInfoInstance = GameObject.Find("UserInterface/MenuContent/Screens/UserInfo").GetComponent<PageUserInfo>();
        }

        // Completely stolen from Psychloor's PlayerRotator (https://github.com/Psychloor/PlayerRotater)
        private static IEnumerator StartEmmCheck(ApiWorld world)
        {
            // Check if black/whitelisted from emmVRC - thanks Emilia and the rest of emmVRC Staff
            WWW www = new WWW($"https://dl.emmvrc.com/riskyfuncs.php?worldid={world.id}", null, new Dictionary<string, string>());
            while (!www.isDone)
                yield return null;
            string result = www.text?.Trim().ToLower();
            www.Dispose();
            if (!string.IsNullOrWhiteSpace(result))
            {
                switch (result)
                {
                    case "allowed":
                        MelonLogger.Msg("World allowed to use risky functions");
                        AreRiskyFunctionsAllowed = true;
                        yield break;

                    case "denied":
                        MelonLogger.Msg("World NOT allowed to use risky functions");
                        AreRiskyFunctionsAllowed = false;
                        yield break;
                }
            }

            // no result from server or they're currently down
            // Check tags/GameObjects then.
            if (GameObject.Find("eVRCRiskFuncEnable") != null)
            {
                AreRiskyFunctionsAllowed = true;
                yield break;
            }
            else if (GameObject.Find("eVRCRiskFuncDisable") != null)
            {
                AreRiskyFunctionsAllowed = false;
                yield break;
            }

            foreach (string worldTag in world.tags)
            {
                if (worldTag.ToLower().Contains("game") || worldTag.ToLower().Contains("club"))
                {
                    MelonLogger.Msg("World NOT allowed to use risky functions");
                    AreRiskyFunctionsAllowed = false;
                    yield break;
                }
            }

            MelonLogger.Msg("World allowed to use risky functions");
            AreRiskyFunctionsAllowed = true;
        }

        /// <summary>
        /// Returns whether the given user's avatar is explicity shown.
        /// </summary>
        /// <param name="user">The user to check</param>
        /// <returns></returns>
        public static bool IsAvatarExplcitlyShown(APIUser user)
        {
            if (ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0.ContainsKey(user.id))
                foreach (ApiPlayerModeration moderation in ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0[user.id])
                    if (moderation.moderationType == ApiPlayerModeration.ModerationType.ShowAvatar)
                        return true;

            return false;
        }

        /// <summary>
        /// Returns whether the given user's avatar is explicity hidden.
        /// </summary>
        /// <param name="user">The user to check</param>
        /// <returns></returns>
        public static bool IsAvatarExplcitlyHidden(APIUser user)
        {
            if (ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0.ContainsKey(user.id))
                foreach (ApiPlayerModeration moderation in ModerationManager.prop_ModerationManager_0.field_Private_Dictionary_2_String_List_1_ApiPlayerModeration_0[user.id])
                    if (moderation.moderationType == ApiPlayerModeration.ModerationType.HideAvatar)
                        return true;

            return false;
        }

        /// <summary>
        /// Reloads all avatars.
        /// </summary>
        public static void ReloadAllAvatars()
        {
            _reloadAllAvatarsMethod.Invoke(VRCPlayer.field_Internal_Static_VRCPlayer_0, new object[] { true });
            _reloadAvatarMethod.Invoke(VRCPlayer.field_Internal_Static_VRCPlayer_0, new object[] { true }); // Ensure self is also reloaded (reload is not networked)
        }

        /// <summary>
        /// Reloads the given player's avatar.
        /// </summary>
        /// <param name="player">The given player</param>
        public static void ReloadAvatar(VRCPlayer player)
        {
            _reloadAvatarMethod.Invoke(player, new object[] { true }); // Ensure self is also reloaded (reload is not networked)
        }
    }
}
