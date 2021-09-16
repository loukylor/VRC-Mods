using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
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
            private set { _areRiskyFunctionsAllowed = value; AsyncUtils._toMainThreadQueue.Enqueue(new Action(() => OnEmmWorldCheckCompleted?.DelegateSafeInvoke(value))); } 
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

        private static MethodInfo _loadAvatarMethod;
        private static MethodInfo _reloadAllAvatarsMethod;

        internal static void Init()
        {
            NetworkEvents.OnInstanceChanged += new Action<ApiWorld, ApiWorldInstance>((world, instance) => StartEmmCheck(world));
            MelonCoroutines.Start(UiInitCoroutine());

            _loadAvatarMethod = typeof(VRCPlayer).GetMethods().First(mi => mi.Name.StartsWith("Method_Private_Void_Boolean_") && mi.Name.Length < 31 && mi.GetParameters().Any(pi => pi.IsOptional) && XrefUtils.CheckUsedBy(mi, "ReloadAvatarNetworkedRPC"));
            _reloadAllAvatarsMethod = typeof(VRCPlayer).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Void_Boolean_") && mi.Name.Length < 30 && mi.GetParameters().All(pi => pi.IsOptional) && XrefUtils.CheckUsedBy(mi, "Method_Public_Void_", typeof(FeaturePermissionManager)));// Both methods seem to do the same thing;
        }

        private static IEnumerator UiInitCoroutine()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null)
                yield return null;

            OnUiManagerInit?.Invoke();
        }

        internal static void UiInit()
        {
            MenuControllerInstance = QuickMenu.prop_QuickMenu_0.field_Public_MenuController_0;
            WorldInfoInstance = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo").GetComponent<PageWorldInfo>();
            UserInfoInstance = GameObject.Find("UserInterface/MenuContent/Screens/UserInfo").GetComponent<PageUserInfo>();
        }

        // Completely stolen from Psychloor's PlayerRotator (https://github.com/Psychloor/PlayerRotater)
        private static void StartEmmCheck(ApiWorld world)
        {
            // Check if black/whitelisted from emmVRC - thanks Emilia and the rest of emmVRC Staff
            HttpWebRequest request = WebRequest.CreateHttp($"https://dl.emmvrc.com/riskyfuncs.php?worldid={world.id}");
            request.BeginGetResponse(new AsyncCallback(EndEmmCheck), new Tuple<ApiWorld, HttpWebRequest>(world, request));
        }

        private static void EndEmmCheck(IAsyncResult asyncResult)
        {
            Tuple<ApiWorld, HttpWebRequest> state = (Tuple<ApiWorld, HttpWebRequest>)asyncResult.AsyncState;
            string result;
            using (WebResponse response = state.Item2.EndGetResponse(asyncResult))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
                result = reader.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(result))
            {
                switch (result)
                {
                    case "allowed":
                        MelonLogger.Msg("World allowed to use risky functions");
                        AreRiskyFunctionsAllowed = true;
                        return;

                    case "denied":
                        MelonLogger.Msg("World NOT allowed to use risky functions");
                        AreRiskyFunctionsAllowed = false;
                        return;
                }
            }

            // Fuck it i cant be fucked right now
            AsyncUtils._toMainThreadQueue.Enqueue(new Action(() => CheckWorld(state.Item1)));
        }

        private static void CheckWorld(ApiWorld world)
        {
            // no result from server or they're currently down
            // Check tags/GameObjects then.
            if (GameObject.Find("eVRCRiskFuncEnable") != null)
            {
                AreRiskyFunctionsAllowed = true;
                return;
            }
            else if (GameObject.Find("eVRCRiskFuncDisable") != null)
            {
                AreRiskyFunctionsAllowed = false;
                return;
            }

            foreach (string worldTag in world.tags)
            {
                if (worldTag.ToLower().Contains("game") || worldTag.ToLower().Contains("club"))
                {
                    MelonLogger.Msg("World NOT allowed to use risky functions");
                    AreRiskyFunctionsAllowed = false;
                    return;
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
            _reloadAllAvatarsMethod.Invoke(VRCPlayer.field_Internal_Static_VRCPlayer_0, new object[] { false });
        }

        /// <summary>
        /// Reloads all avatars.
        /// </summary>
        /// <param name="excludeSelf">Whether or not to exclude the local player from the reload.</param>
        public static void ReloadAllAvatars(bool excludeSelf = false)
        {
            _reloadAllAvatarsMethod.Invoke(VRCPlayer.field_Internal_Static_VRCPlayer_0, new object[] { excludeSelf });
        }

        /// <summary>
        /// Reloads the given player's avatar.
        /// </summary>
        /// <param name="player">The given player</param>
        public static void ReloadAvatar(VRCPlayer player)
        {
            _loadAvatarMethod.Invoke(player, new object[] { true }); 
        }
    }
}
