using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.DataModel;
using VRC.UI;
using VRC.UI.Core;
using VRC.UI.Elements;
using VRChatUtilityKit.Utilities;

namespace VRChatUtilityKit.Ui
{
    // This "Button API", if you can it that, is based off of RubyButtonAPI, by DubyaDude (dooba lol) (https://github.com/DubyaDude)
    /// <summary>
    /// A UiManager that contains many utilites pertaining to VRChat's UI.
    /// </summary>
    public class UiManager
    {
        private static MethodInfo _popupV2;
        private static MethodInfo _popupV2Small;

        /// <summary>
        /// Called when the big menu is opened.
        /// </summary>
        public static event Action OnBigMenuOpened;
        /// <summary>
        /// Called when the big menu is closed.
        /// </summary>
        public static event Action OnBigMenuClosed;

        private static MethodInfo _closeBigMenu;
        private static MethodBase _openBigMenu;
        private static MethodInfo _mainMenu;
        private static bool _shouldSkipPlaceUiAfterPause;
        private static bool _shouldChangeScreenStackValue;
        private static bool _newScreenStackValue;
        /// <summary>
        /// The type of the enum that is used for the big menu index.
        /// </summary>
        public static Type BigMenuIndexEnum { get; private set; }
        /// <summary>
        /// A table that will convert the big menu index to the path of the page.
        /// </summary>
        public static Dictionary<int, string> BigMenuIndexToPathTable { get => _bigMenuIndexToPathTable; }
        private static readonly Dictionary<int, string> _bigMenuIndexToPathTable = new Dictionary<int, string>()
        {
            { -1, "" },
            { 0, "UserInterface/MenuContent/Screens/WorldInfo" },
            { 1, "UserInterface/MenuContent/Screens/Avatar" },
            { 2, "UserInterface/MenuContent/Screens/Social" },
            { 3, "UserInterface/MenuContent/Screens/Settings" },
            { 4, "UserInterface/MenuContent/Screens/UserInfo" },
            { 5, "UserInterface/MenuContent/Screens/ImageDetails" },
            { 6, "UserInterface/MenuContent/Screens/Settings_Safety" },
            { 7, "UserInterface/MenuContent/Screens/Playlists" },
            { 8, "UserInterface/MenuContent/Screens/Playlists" },
            { 9, "UserInterface/MenuContent/Screens/VRC+" },
            { 10, "UserInterface/MenuContent/Screens/Gallery" },
        };

        /// <summary>
        /// Called when the user info menu is opened.
        /// </summary>
        public static event Action OnUserInfoMenuOpened;
        /// <summary>
        /// Called when the user info menu is closed.
        /// </summary>
        public static event Action OnUserInfoMenuClosed;

        /// <summary>
        /// Called when the QuickMenu is opened.
        /// </summary>
        public static event Action OnQuickMenuOpened;
        /// <summary>
        /// Called when the QuickMenu is closed.
        /// </summary>
        public static event Action OnQuickMenuClosed;

        /// <summary>
        /// Called when a UIPage is shown in the QuickMenu.
        /// The bool in the event is whether the page was shown or hidden.
        /// </summary>
        public static event Action<UIPage, bool> OnUIPageToggled;

        private static MethodInfo _selectUserMethod;
        private static MethodInfo _selectUserForMoreInfoMethod;
        internal static MethodInfo _pushPageMethod;
        internal static MethodInfo _removePageMethod;

        private static MethodInfo _openQuickMenuPageMethod;
        private static MethodInfo _openQuickMenuMethod;

        private static MethodInfo _closeMenuMethod;
        private static MethodInfo _closeQuickMenuMethod;

        private static PropertyInfo _quickMenuEnumProperty;
        /// <summary>
        /// The type of the enum that is used for the QuickMenu index.
        /// </summary>
        public static Type QuickMenuIndexEnum { get; private set; }

        internal static Transform tempUIParent;

        /// <summary>
        /// The filled in button sprite.
        /// </summary>
        public static Sprite FullOnButtonSprite { get; private set; }
        /// <summary>
        /// The regular button sprite.
        /// </summary>
        public static Sprite RegularButtonSprite { get; private set; }

        /// <summary>
        /// The QuickMenu MenuStateController used by VRChat
        /// </summary>
        public static MenuStateController QMStateController { get; private set; }

        internal static void Init()
        {
            List<Type> quickMenuNestedEnums = typeof(QuickMenu).GetNestedTypes().Where(type => type.IsEnum).ToList();
            _quickMenuEnumProperty = typeof(QuickMenu).GetProperties()
                .First(pi => pi.PropertyType.IsEnum && quickMenuNestedEnums.Contains(pi.PropertyType));
            QuickMenuIndexEnum = _quickMenuEnumProperty.PropertyType;

            BigMenuIndexEnum = quickMenuNestedEnums.First(type => type.IsEnum && type != QuickMenuIndexEnum);
            _closeBigMenu = typeof(VRCUiManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && !mb.Name.Contains("_PDM_") && XrefUtils.CheckUsedBy(mb, "Method_Public_Void_Player_"));
            _mainMenu = typeof(QuickMenu).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Enum") && mb.GetParameters().Length == 2 && mb.GetParameters()[0].ParameterType == BigMenuIndexEnum && mb.GetParameters()[1].ParameterType == typeof(bool));
            foreach (XrefInstance instance in XrefScanner.XrefScan(_mainMenu))
            {
                if (instance.Type != XrefType.Method || instance.TryResolve() == null)
                    continue;

                if (_openBigMenu == null && instance.TryResolve().Name.StartsWith("Method_Public_Void_Boolean_Boolean_"))
                {
                    _openBigMenu = instance.TryResolve();
                    break;
                }
            }

            MethodInfo _placeUiAfterPause = typeof(QuickMenu).GetNestedTypes().First(type => type.Name.Contains("IEnumerator")).GetMethod("MoveNext");

            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(UIPage).GetMethod("Method_Public_Void_Boolean_TransitionType_0"), new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnUIPageToggle), BindingFlags.NonPublic | BindingFlags.Static)));

            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_openBigMenu, null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnBigMenuOpen), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_closeBigMenu, null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnBigMenuClose), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_placeUiAfterPause, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnPlaceUiAfterPause), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(VRCUiManager).GetMethod("Method_Public_Void_String_Boolean_0"), new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnShowScreen), BindingFlags.NonPublic | BindingFlags.Static)));

            foreach (MethodInfo method in typeof(MenuController).GetMethods().Where(mi => mi.Name.StartsWith("Method_Public_Void_APIUser_") && !mi.Name.Contains("_PDM_")))
                VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(method, postfix: new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnUserInfoOpen), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(AccessTools.Method(typeof(PageUserInfo), "Back"), postfix: new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnUserInfoClose), BindingFlags.NonPublic | BindingFlags.Static)));

            _closeMenuMethod = typeof(UIManagerImpl).GetMethods()
                .First(method => method.Name.StartsWith("Method_Public_Virtual_Final_New_Void_") && XrefScanner.XrefScan(method).Count() == 2);
            _closeQuickMenuMethod = typeof(UIManagerImpl).GetMethods()
                .First(method => method.Name.StartsWith("Method_Public_Void_Boolean_") && XrefUtils.CheckUsedBy(method, _closeMenuMethod.Name));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_closeQuickMenuMethod, null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnQuickMenuClose), BindingFlags.NonPublic | BindingFlags.Static)));

            _openQuickMenuMethod = typeof(UIManagerImpl).GetMethods()
                .First(method => method.Name.StartsWith("Method_Public_Void_") && method.Name.Length <= 21 && XrefUtils.CheckUsedBy(method, "RequestInvitation"));
            _openQuickMenuPageMethod = typeof(UIManagerImpl).GetMethods()
                .First(method => method.Name.StartsWith("Method_Public_Virtual_Final_New_Void_String_") && XrefUtils.CheckUsing(method, _openQuickMenuMethod.Name, _openQuickMenuMethod.DeclaringType));

            // Patching the other method doesn't work for some reason you have to patch this
            MethodInfo _onQuickMenuOpenedMethod = typeof(UIManagerImpl).GetMethods()
                .First(method => method.Name.StartsWith("Method_Private_Void_Boolean_") && !method.Name.Contains("_PDM_") && XrefUtils.CheckUsedBy(method, _openQuickMenuMethod.Name));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_onQuickMenuOpenedMethod, null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnQuickMenuOpen), BindingFlags.NonPublic | BindingFlags.Static)));

            _popupV2Small = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && XrefUtils.CheckStrings(mb, "UserInterface/MenuContent/Popups/StandardPopupV2") && XrefUtils.CheckUsedBy(mb, "OpenSaveSearchPopup"));
            _popupV2 = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && XrefUtils.CheckStrings(mb, "UserInterface/MenuContent/Popups/StandardPopupV2"));
        }

        internal static void UiInit()
        {
            tempUIParent = new GameObject("VRChatUtilityKitTempUIParent").transform;
            GameObject.DontDestroyOnLoad(tempUIParent.gameObject);

            QMStateController = GameObject.Find("UserInterface").transform.Find("Canvas_QuickMenu(Clone)").GetComponent<MenuStateController>();

            // index 0 works because transform doesn't inherit from monobehavior
            Type selectedUserManager = XrefUtils.GetTypeFromObfuscatedName(GameObject.Find("_Application/UIManager/SelectedUserManager").GetComponents<MonoBehaviour>()[0].GetIl2CppType().Name);
            MethodInfo[] selectedUserMethods = selectedUserManager.GetMethods()
                .Where(method => method.Name.StartsWith("Method_Public_Void_APIUser_"))
                .OrderBy(method => method.GetCustomAttribute<CallerCountAttribute>().Count)
                .ToArray();
            _selectUserMethod = selectedUserMethods[0];
            _selectUserForMoreInfoMethod = selectedUserMethods[1]; // This one has higher caller count

            MethodInfo[] pageMethods = typeof(UIPage).GetMethods()
                .Where(method => method.Name.StartsWith("Method_Public_Void_UIPage_") && !method.Name.Contains("_PDM_"))
                .ToArray();
            _pushPageMethod = pageMethods.First(method => XrefUtils.CheckUsing(method, "Add"));
            _removePageMethod = pageMethods.First(method => method != _pushPageMethod);

            ButtonReaction buttonReaction = GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton").GetComponent<ButtonReaction>();
            FullOnButtonSprite = buttonReaction.field_Public_Sprite_0.name.Contains("Full_ON") ? buttonReaction.field_Public_Sprite_0 : buttonReaction.field_Public_Sprite_1;
            RegularButtonSprite = buttonReaction.field_Public_Sprite_0.name.Contains("White") ? buttonReaction.field_Public_Sprite_0 : buttonReaction.field_Public_Sprite_1;
        }

        private static void OnBigMenuOpen() => OnBigMenuOpened?.DelegateSafeInvoke();
        private static void OnBigMenuClose() => OnBigMenuClosed?.DelegateSafeInvoke();
        private static bool OnPlaceUiAfterPause(ref bool __result)
        {
            if (!_shouldSkipPlaceUiAfterPause)
                return true;

            _shouldSkipPlaceUiAfterPause = false;
            __result = false;
            return false;
        }
        private static void OnShowScreen(ref bool __1)
        {
            if (_shouldChangeScreenStackValue)
            {
                __1 = _newScreenStackValue;
                _shouldChangeScreenStackValue = false;
            }
        }
        /// <summary>
        /// Sets the index of the big menu.
        /// </summary>
        /// <param name="index">The index to set it to</param>
        public static void MainMenu(int index) => MainMenu(index, true, true, true);
        /// <summary>
        /// Sets the index of the big menu.
        /// </summary>
        /// <param name="index">The index to set it to</param>
        /// <param name="openUi">Whether to open the Ui along with setting the index</param>
        public static void MainMenu(int index, bool openUi) => MainMenu(index, openUi, true, true);
        /// <summary>
        /// Sets the index of the big menu.
        /// </summary>
        /// <param name="index">The index to set it to</param>
        /// <param name="openUi">Whether to open the Ui along with setting the index</param>
        /// <param name="addToScreenStack">Whether the new screen opened should be added to the screen stack</param>
        public static void MainMenu(int index, bool openUi, bool addToScreenStack) => MainMenu(index, addToScreenStack, openUi, true);
        /// <summary>
        /// Sets the index of the big menu.
        /// </summary>
        /// <param name="index">The index to set it to</param>
        /// <param name="openUi">Whether to open the Ui along with setting the index</param>
        /// <param name="addToScreenStack">Whether the new screen opened should be added to the screen stack</param>
        /// <param name="rePlaceUi">Whether to recalculate and reposition the UI</param>
        public static void MainMenu(int index, bool openUi, bool addToScreenStack, bool rePlaceUi)
        {
            _shouldChangeScreenStackValue = true;
            _newScreenStackValue = addToScreenStack;
            _shouldSkipPlaceUiAfterPause = !rePlaceUi;
            _mainMenu.Invoke(QuickMenu.prop_QuickMenu_0, new object[2] { index, !openUi });
        }
        /// <summary>
        /// Opens the given user in the user info page. 
        /// Does not open with big menu along with the page.
        /// </summary>
        /// <param name="user">The user to open</param>
        public static void OpenUserInUserInfoPage(APIUser user)
        {
            QuickMenu.prop_QuickMenu_0.prop_APIUser_0 = user ?? throw new ArgumentNullException("Given APIUser was null.");
            MainMenu(4, false, false, false);
        }
        /// <summary>
        /// Opens the given user in the user info page. 
        /// Does not open with big menu along with the page.
        /// </summary>
        /// <param name="user">The user to open</param>
        /// <param name="addToScreenStack"></param>
        public static void OpenUserInUserInfoPage(APIUser user, bool addToScreenStack)
        {
            QuickMenu.prop_QuickMenu_0.prop_APIUser_0 = user ?? throw new ArgumentNullException("Given APIUser was null.");
            MainMenu(4, false, addToScreenStack, false);
        }
        /// <summary>
        /// Closes the big menu.
        /// </summary>
        public static void CloseBigMenu() => _closeBigMenu.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[2] { true, false });
        /// <summary>
        /// Opens the big menu.
        /// </summary>
        public static void OpenBigMenu() => OpenBigMenu(true);
        /// <summary>
        /// Opens the big menu
        /// </summary>
        /// <param name="showDefaultScreen">Whether to show the world menu after opening the big menu</param>
        public static void OpenBigMenu(bool showDefaultScreen) => _openBigMenu.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[2] { showDefaultScreen, true });

        private static void OnUserInfoOpen() => OnUserInfoMenuOpened?.DelegateSafeInvoke();
        private static void OnUserInfoClose() => OnUserInfoMenuClosed?.DelegateSafeInvoke();

        private static void OnQuickMenuOpen() => OnQuickMenuOpened?.DelegateSafeInvoke();
        private static void OnQuickMenuClose() => OnQuickMenuClosed?.DelegateSafeInvoke(); 

        private static void OnUIPageToggle(UIPage __instance, bool __0) => OnUIPageToggled.DelegateSafeInvoke(__instance, __0);

        private static Exception OnQuickMenuIndexAssignedErrorSuppressor(Exception __exception)
        {
            // There's actually no better way to do this https://github.com/knah/Il2CppAssemblyUnhollower/blob/master/UnhollowerBaseLib/Il2CppException.cs
            if (__exception is NullReferenceException || __exception.Message.Contains("System.NullReferenceException"))
                return null;
            else
                return __exception;
        }

        /// <summary>
        /// Opens given user in the QuickMenu.
        /// </summary>
        /// <param name="playerToSelect">The player to select</param>
        public static void OpenUserInQuickMenu(APIUser playerToSelect) => _selectUserMethod.Invoke(VRCData.field_Public_Static_IUserSelection_0, new object[1] { playerToSelect });

        /// <summary>
        /// Opens given user in the Open User For More Info QuickMenu.
        /// </summary>
        /// <param name="playerToSelect">The player to select</param>
        public static void OpenUserForMoreInfoInQuickMenu(APIUser playerToSelect) => _selectUserForMoreInfoMethod.Invoke(VRCData.field_Public_Static_IUserSelection_0, new object[1] { playerToSelect });

        /// <summary>
        /// Opens the QuickMenu.
        /// </summary>
        public static void OpenQuickMenu() => _openQuickMenuMethod?.Invoke(UIManagerImpl.prop_UIManagerImpl_0, null);

        /// <summary>
        /// Closes the QuickMenu.
        /// </summary>
        public static void CloseQuickMenu() => _closeQuickMenuMethod?.Invoke(UIManagerImpl.prop_UIManagerImpl_0, new object[1] { false });

        /// <summary>
        /// Closes all open menus.
        /// </summary>
        public static void CloseMenu() => _closeMenuMethod?.Invoke(UIManagerImpl.prop_UIManagerImpl_0, null);

        /// <summary>
        /// Closes the current open popup
        /// </summary>
        public static void ClosePopup() => VRCUiManager.prop_VRCUiManager_0.HideScreen("POPUP");
        /// <summary>
        /// Opens a small popup v2.
        /// </summary>
        /// <param name="title">The title of the popup</param>
        /// <param name="description">The description of the popup</param>
        /// <param name="buttonText">The text of the center button</param>
        /// <param name="onButtonClick">The onClick of the center button</param>
        /// <param name="additionalSetup">A callback called when the popup is initialized</param>
        public static void OpenSmallPopup(string title, string description, string buttonText, Action onButtonClick, Action<VRCUiPopup> additionalSetup = null) => _popupV2Small.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[5] { title, description, buttonText, (Il2CppSystem.Action)onButtonClick, (Il2CppSystem.Action<VRCUiPopup>)additionalSetup });
        /// <summary>
        /// Opens a small popup v2 with the title "Error!".
        /// </summary>
        /// <param name="description">The description of the popup</param>
        public static void OpenErrorPopup(string description) => OpenSmallPopup("Error!", description, "Ok", new Action(ClosePopup));
        /// <summary>
        /// Opens a small popup v2 with the title "Alert!".
        /// </summary>
        /// <param name="description">The description of the popup</param>
        public static void OpenAlertPopup(string description) => OpenSmallPopup("Alert!", description, "Ok", new Action(ClosePopup));
        /// <summary>
        /// Opens a small popup v2.
        /// </summary>
        /// <param name="title">The title of the popup</param>
        /// <param name="description">The description of the popup</param>
        /// <param name="leftButtonText">The text of the left button</param>
        /// <param name="leftButtonClick">The onClick of the left button</param>
        /// <param name="rightButtonText">The text of the right button</param>
        /// <param name="rightButtonClick">The onClick of the right button</param>
        /// <param name="additionalSetup">A callback called when the popup is initialized</param>
        public static void OpenPopup(string title, string description, string leftButtonText, Action leftButtonClick, string rightButtonText, Action rightButtonClick, Action<VRCUiPopup> additionalSetup = null) => _popupV2.Invoke(VRCUiPopupManager.prop_VRCUiPopupManager_0, new object[7] { title, description, leftButtonText, (Il2CppSystem.Action)leftButtonClick, rightButtonText, (Il2CppSystem.Action)rightButtonClick, (Il2CppSystem.Action<VRCUiPopup>)additionalSetup });
    }
}
