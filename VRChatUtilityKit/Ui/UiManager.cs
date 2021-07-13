using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;
using VRC.UI;
using VRChatUtilityKit.Utilities;

using UnhollowerRuntimeLib.XrefScans;

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
        /// Called when the QuickMenu index is set.
        /// </summary>
        public static event Action<int> OnQuickMenuIndexSet;

        private static MethodInfo _setQuickMenuIndex;
        private static MethodInfo _openQuickMenu;
        private static MethodInfo _openQuickMenuNoBool;
        private static MethodInfo _closeQuickMenu;
        private static PropertyInfo _quickMenuEnumProperty;
        /// <summary>
        /// The type of the enum that is used for the QuickMenu index.
        /// </summary>
        public static Type QuickMenuIndexEnum { get; private set; }

        private static Il2CppSystem.Reflection.MethodInfo _showTabContentMethod;
        private static Il2CppSystem.Reflection.FieldInfo _setIndexOfTab;
        private static Il2CppSystem.Reflection.FieldInfo _setTabIndexInQuickMenu;
        internal static Il2CppSystem.Type _tabDescriptorType;
        private static Component _tabContainerComponent;
        private static Il2CppSystem.Reflection.FieldInfo _existingTabsField;
        /// <summary>
        /// The current list of tabs registered by the game.
        /// </summary>
        public static List<GameObject> ExistingTabs
        {
            get { return _existingTabsField.GetValue(_tabContainerComponent).Cast<Il2CppReferenceArray<GameObject>>().ToList(); }
            set { _existingTabsField.SetValue(_tabContainerComponent, new Il2CppReferenceArray<GameObject>(value.ToArray()).Cast<Il2CppSystem.Object>()); }
        }

        /// <summary>
        /// The filled in button sprite.
        /// </summary>
        public static Sprite FullOnButtonSprite { get; private set; }
        /// <summary>
        /// The regular button sprite.
        /// </summary>
        public static Sprite RegularButtonSprite { get; private set; }

        /// <summary>
        /// The current menu stored by VRChat.
        /// </summary>
        public static GameObject CurrentMenu
        {
            get
            {
                if (_currentMenuField == null)
                    return null;
                else
                    return (GameObject)_currentMenuField.GetValue(QuickMenu.prop_QuickMenu_0);
            }
            set { _currentMenuField?.SetValue(QuickMenu.prop_QuickMenu_0, value); }
        }
        private static PropertyInfo _currentMenuField;

        /// <summary>
        /// The current tab menu stored by VRChat.
        /// </summary>
        public static GameObject CurrentTabMenu
        {
            get
            {
                if (currentTabMenuField == null)
                    return null;
                else
                    return (GameObject)currentTabMenuField.GetValue(QuickMenu.prop_QuickMenu_0);
            }
            set { currentTabMenuField?.SetValue(QuickMenu.prop_QuickMenu_0, value); }
        }
        private static PropertyInfo currentTabMenuField;

        private static Type _quickMenuContextualDisplayEnum;
        private static MethodBase _setContextMethod;

        internal static void Init()
        {
            List<Type> quickMenuNestedEnums = typeof(QuickMenu).GetNestedTypes().Where(type => type.IsEnum).ToList();
            _quickMenuEnumProperty = typeof(QuickMenu).GetProperties()
                .First(pi => pi.PropertyType.IsEnum && quickMenuNestedEnums.Contains(pi.PropertyType));
            QuickMenuIndexEnum = _quickMenuEnumProperty.PropertyType;
            _setQuickMenuIndex = typeof(QuickMenu).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Enum") && !mb.Name.Contains("_PDM_") && mb.GetParameters().Length == 1 && mb.GetParameters()[0].ParameterType == QuickMenuIndexEnum);
            _openQuickMenu = typeof(QuickMenu).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && mb.Name.Length <= 29 && XrefUtils.CheckUsing(mb, _setQuickMenuIndex.Name, typeof(QuickMenu)));
            _openQuickMenuNoBool = (MethodInfo)XrefScanner.UsedBy(_openQuickMenu)
                .First(instance => instance.Type == XrefType.Method && instance.TryResolve() != null && XrefUtils.CheckUsedBy((MethodInfo)instance.TryResolve(), "OpenQuickMenuAddMessagePhotoCamera")).TryResolve();
            _closeQuickMenu = typeof(QuickMenu).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && mb.Name.Length <= 29 && XrefUtils.CheckUsedBy(mb, _setQuickMenuIndex.Name));

            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_openQuickMenu, null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnQuickMenuOpen), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_closeQuickMenu, null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnQuickMenuClose), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_setQuickMenuIndex, null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnQuickMenuIndexAssigned), BindingFlags.NonPublic | BindingFlags.Static)), null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnQuickMenuIndexAssignedErrorSuppressor), BindingFlags.NonPublic | BindingFlags.Static)), null);

            BigMenuIndexEnum = quickMenuNestedEnums.First(type => type.IsEnum && type != QuickMenuIndexEnum);
            _closeBigMenu = typeof(VRCUiManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && !mb.Name.Contains("_PDM_") && XrefUtils.CheckUsedBy(mb, "ExitStation"));
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

            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_openBigMenu, null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnBigMenuOpen), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_closeBigMenu, null, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnBigMenuClose), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(_placeUiAfterPause, new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnPlaceUiAfterPause), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(VRCUiManager).GetMethod("Method_Public_Void_String_Boolean_0"), new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnShowScreen), BindingFlags.NonPublic | BindingFlags.Static)));

            foreach (MethodInfo method in typeof(MenuController).GetMethods().Where(mi => mi.Name.StartsWith("Method_Public_Void_APIUser_") && !mi.Name.Contains("_PDM_")))
                VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(method, postfix: new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnUserInfoOpen), BindingFlags.NonPublic | BindingFlags.Static)));
            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(AccessTools.Method(typeof(PageUserInfo), "Back"), postfix: new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnUserInfoClose), BindingFlags.NonPublic | BindingFlags.Static)));

            _quickMenuContextualDisplayEnum = typeof(QuickMenuContextualDisplay).GetNestedTypes()
                .First(type => type.Name.StartsWith("Enum"));
            _setContextMethod = typeof(QuickMenuContextualDisplay).GetMethod($"Method_Public_Void_{_quickMenuContextualDisplayEnum.Name}_APIUser_String_0");

            _popupV2Small = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && XrefUtils.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2") && XrefUtils.CheckUsedBy(mb, "OpenSaveSearchPopup"));
            _popupV2 = typeof(VRCUiPopupManager).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && XrefUtils.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopupV2"));
        }
        internal static void UiInit()
        {
            GameObject tabContainer = GameObject.Find("UserInterface/QuickMenu/QuickModeTabs");
            foreach (Component component in tabContainer.GetComponents<Component>())
            {
                if (!component.GetIl2CppType().FullName.Contains("UnityEngine") && component.GetIl2CppType().GetMethods().Any(mi => mi.Name == "ShowTabContent"))
                {
                    _tabContainerComponent = component;
                    _setTabIndexInQuickMenu = component.GetIl2CppType().GetFields(Il2CppSystem.Reflection.BindingFlags.NonPublic | Il2CppSystem.Reflection.BindingFlags.Instance).First(f => f.FieldType.IsEnum);
                    foreach (Il2CppSystem.Reflection.FieldInfo field in component.GetIl2CppType().GetFields())
                    { 
                        if (field.FieldType.IsArray)
                        { 
                            _existingTabsField = field;
                            break;
                        }
                    }
                    break;
                }
            }

            MonoBehaviour tabDescriptor = tabContainer.transform.GetChild(0).gameObject.GetComponents<MonoBehaviour>().First(c => c.GetIl2CppType().GetMethod("ShowTabContent") != null);

            _tabDescriptorType = tabDescriptor.GetIl2CppType();
            _showTabContentMethod = _tabDescriptorType.GetMethod("ShowTabContent");
            _setIndexOfTab = _tabDescriptorType.GetFields().First(f => f.FieldType.IsEnum);

            ButtonReaction buttonReaction = GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton").GetComponent<ButtonReaction>();
            FullOnButtonSprite = buttonReaction.field_Public_Sprite_0.name.Contains("Full_ON") ? buttonReaction.field_Public_Sprite_0 : buttonReaction.field_Public_Sprite_1;
            RegularButtonSprite = buttonReaction.field_Public_Sprite_0.name.Contains("White") ? buttonReaction.field_Public_Sprite_0 : buttonReaction.field_Public_Sprite_1;

            if (_currentMenuField != null) return;

            // Check which fields return null
            List<PropertyInfo> possibleProps = new List<PropertyInfo>();
            foreach (PropertyInfo prop in typeof(QuickMenu).GetProperties().Where(pi => pi.Name.StartsWith("field_Private_GameObject_")))
            {
                GameObject value = (GameObject)prop.GetValue(QuickMenu.prop_QuickMenu_0);
                if (value == null)
                    possibleProps.Add(prop);
            }

            // Open QuickMenu to set current menu
            try
            {
                _setQuickMenuIndex.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { 8 });
            }
            catch { } // Ignore error cuz it still sets the menu

            // Find out which menu actually got set
            foreach (PropertyInfo prop in possibleProps)
                if (prop.GetValue(QuickMenu.prop_QuickMenu_0) != null)
                    _currentMenuField = prop;

            if (_currentMenuField == null) MelonLogger.Error("Something went wrong. In technical speak: after attempting to determine the current menu field info, it was null. The mod will likely not function properly");

            CurrentMenu.SetActive(false);

            MonoBehaviour tabManager = GameObject.Find("UserInterface/QuickMenu/QuickModeTabs").GetComponents<MonoBehaviour>().First(monoBehaviour => monoBehaviour.GetIl2CppType().GetMethods().Any(mb => mb.Name.StartsWith("ShowTabContent")));
            Il2CppSystem.Reflection.PropertyInfo tabManagerSingleton = tabManager.GetIl2CppType().GetProperties().First(pi => pi.PropertyType == tabManager.GetIl2CppType());
            tabManagerSingleton.SetValue(null, tabManager, null); // Singleton is null until QM is opened. Set it to a value so that the next line won't error

            try
            {
                GameObject.Find("UserInterface/QuickMenu/QuickModeTabs/NotificationsTab").GetComponent<Button>().onClick.Invoke(); // Force a button click to set notifs menu as current tab menu
            }
            catch
            {
                MelonLogger.Error("Could not find NotificiationsTab button. The mod will likely not function properly");
            }

            tabManagerSingleton.SetValue(null, null, null); // Set singleton back to null as to not change values willy nilly xD

            foreach (PropertyInfo prop in possibleProps)
                if (prop.Name != _currentMenuField.Name && prop.GetValue(QuickMenu.prop_QuickMenu_0) != null)
                    currentTabMenuField = prop;

            if (_currentMenuField == null) MelonLogger.Error("Something went wrong. In technical speak: after attempting to determine the current tab field info, it was null. The mod will likely not function properly");

            CurrentTabMenu.SetActive(false);

            // Set to null as to not change values unexpectedly 
            foreach (PropertyInfo prop in possibleProps)
                prop.SetValue(QuickMenu.prop_QuickMenu_0, null);
        }

        private static void OpenMenuInternal(GameObject page, bool setCurrentMenu = true, bool setCurrentTab = true)
        {
            if (page == null)
                throw new ArgumentNullException($"Page given is null");

            CurrentMenu?.SetActive(false);

            if (page.name == "ShortcutMenu")
            {
                ShowTabContent(ExistingTabs[0]);
                SetQuickMenuIndex(0);
                SetTabIndexInQuickMenu(0);
            }
            else
            {
                _quickMenuEnumProperty.SetValue(QuickMenu.prop_QuickMenu_0, -1);
            }

            if (setCurrentMenu)
                CurrentMenu = page;

            CurrentTabMenu?.SetActive(false);
            if (setCurrentTab)
                CurrentTabMenu = page;

            QuickMenuContextualDisplay quickMenuContextualDisplay = QuickMenu.prop_QuickMenu_0.field_Private_QuickMenuContextualDisplay_0;
            _setContextMethod.Invoke(quickMenuContextualDisplay, new object[3] { 0, null, null });

            page.SetActive(true);
        }

        /// <summary>
        /// Opens the given page in the QuickMenu.
        /// </summary>
        /// <param name="page">The page to open</param>
        /// <param name="setCurrentMenu">Whether to set the current menu stored by VRChat to the page given</param>
        /// <param name="setCurrentTab">Whether to set the current tab menu stored by VRChat to the page given</param>
        public static void OpenSubMenu(SubMenu page, bool setCurrentMenu = true, bool setCurrentTab = true) => OpenSubMenu(page.gameObject, setCurrentMenu, setCurrentTab);
        /// <summary>
        /// Opens the given page in the QuickMenu.
        /// </summary>
        /// <param name="page">The page to open</param>
        /// <param name="setCurrentMenu">Whether to set the current menu stored by VRChat to the page given</param>
        /// <param name="setCurrentTab">Whether to set the current tab menu stored by VRChat to the page given</param>
        public static void OpenSubMenu(string page, bool setCurrentMenu = true, bool setCurrentTab = true) => OpenSubMenu(GameObject.Find(page), setCurrentMenu, setCurrentTab);
        /// <summary>
        /// Opens the given page in the QuickMenu.
        /// </summary>
        /// <param name="page">The page to open</param>
        /// <param name="setCurrentMenu">Whether to set the current menu stored by VRChat to the page given</param>
        /// <param name="setCurrentTab">Whether to set the current tab menu stored by VRChat to the page given</param>
        public static void OpenSubMenu(GameObject page, bool setCurrentMenu = true, bool setCurrentTab = true) => OpenMenuInternal(page, setCurrentMenu, setCurrentTab);

        /// <summary>
        /// Opens the given tab as a tab menu and highlights the given tab button.
        /// </summary>
        /// <param name="tabButton">The tab button to highlight</param>
        /// <param name="page">The page to open</param>
        /// <param name="setCurrentMenu">Whether to set the current menu stored by VRChat to the page given</param>
        /// <param name="setCurrentTab">Whether to set the current tab menu stored by VRChat to the page given</param>
        public static void OpenTabMenu(TabButton tabButton, GameObject page, bool setCurrentMenu = true, bool setCurrentTab = true)
        {
            ShowTabContent(tabButton.gameObject);
            OpenMenuInternal(page, setCurrentMenu, setCurrentTab);
            SetTabIndexInQuickMenu(tabButton.index);
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
            if (user == null)
                throw new ArgumentNullException("Given APIUser was null.");

            QuickMenu.prop_QuickMenu_0.prop_APIUser_0 = user;
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
            if (user == null)
                throw new ArgumentNullException("Given APIUser was null.");

            QuickMenu.prop_QuickMenu_0.prop_APIUser_0 = user;
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
        private static void OnQuickMenuIndexAssigned(int __0) => OnQuickMenuIndexSet?.DelegateSafeInvoke(__0);
        private static Exception OnQuickMenuIndexAssignedErrorSuppressor(Exception __exception) 
        {
            // There's actually no better way to do this https://github.com/knah/Il2CppAssemblyUnhollower/blob/master/UnhollowerBaseLib/Il2CppException.cs
            if (__exception is NullReferenceException || __exception.Message.Contains("System.NullReferenceException"))
                return null;
            else
                return __exception;
        }
        /// <summary>
        /// Sets the index of the QuickMenu.
        /// </summary>
        /// <param name="index">The index to set it to</param>
        public static void SetQuickMenuIndex(int index) => _setQuickMenuIndex.Invoke(QuickMenu.prop_QuickMenu_0, new object[1] { index });
        /// <summary>
        /// Opens given user in the QuickMenu.
        /// </summary>
        /// <param name="playerToSelect">The player to select</param>
        public static void OpenUserInQuickMenu(Player playerToSelect)
        {
            if (playerToSelect == null)
                throw new ArgumentNullException("Given Player was null.");
            if (playerToSelect.prop_APIUser_0 == null)
                throw new ArgumentNullException("Given Player's APIUser was null.");
            if (playerToSelect.prop_VRCPlayer_0 == null)
                throw new ArgumentNullException("Given Player's VRCPlayer was null.");

            QuickMenu.prop_QuickMenu_0.prop_APIUser_0 = playerToSelect.prop_APIUser_0;
            QuickMenu.prop_QuickMenu_0.field_Private_Player_0 = playerToSelect;
            CursorUtils.CurrentCursor.Method_Public_Void_VRCPlayer_PDM_0(playerToSelect.prop_VRCPlayer_0);
            QuickMenuContextualDisplay quickMenuContextualDisplay = QuickMenu.prop_QuickMenu_0.field_Private_QuickMenuContextualDisplay_0;
            QuickMenuContextualDisplay.Method_Public_Static_Void_VRCPlayer_0(playerToSelect.prop_VRCPlayer_0);
            _setContextMethod.Invoke(quickMenuContextualDisplay, new object[3] { 3, playerToSelect.prop_APIUser_0, null });
            SetQuickMenuIndex(3);
        }
        /// <summary>
        /// Opens the QuickMenu.
        /// </summary>
        public static void OpenQuickMenu() => _openQuickMenuNoBool.Invoke(QuickMenu.prop_QuickMenu_0, null);
        /// <summary>
        /// Opens the QuickMenu.
        /// </summary>
        /// <param name="useRight">Whether open the QuickMenu on the right hand</param>
        public static void OpenQuickMenu(bool useRight) => _openQuickMenu.Invoke(QuickMenu.prop_QuickMenu_0, new object[1] { useRight });
        /// <summary>
        /// Closes the QuickMenu.
        /// </summary>
        public static void CloseQuickMenu() => _closeQuickMenu.Invoke(QuickMenu.prop_QuickMenu_0, new object[1] { true });

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

        /// <summary>
        /// Sets the index of the given tab.
        /// </summary>
        /// <param name="tabButton">The tab button to set the index of</param>
        /// <param name="index">the index to set the tab button to</param>
        public static void SetIndexOfTab(TabButton tabButton, int index) => _setIndexOfTab.SetValue(tabButton._tabDescriptor, new Il2CppSystem.Int32 { m_value = index }.BoxIl2CppObject());
        /// <summary>
        /// Shows the content of the tab.
        /// </summary>
        /// <param name="tabButton">The tab button whose content is shown</param>
        public static void ShowTabContent(TabButton tabButton) => _showTabContentMethod.Invoke(tabButton._tabDescriptor, null);
        /// <summary>
        /// Shows the content of the tab.
        /// </summary>
        /// <param name="tabButton">The tab button whose content is shown</param>
        public static void ShowTabContent(GameObject tabButton) => _showTabContentMethod.Invoke(tabButton.GetComponent(_tabDescriptorType), null);
        /// <summary>
        /// Sets the tab index in the QuickMenu.
        /// </summary>
        /// <param name="index">The index to set in the QuickMenu</param>
        public static void SetTabIndexInQuickMenu(int index) => _setTabIndexInQuickMenu.SetValue(_tabContainerComponent, new Il2CppSystem.Int32 { m_value = index }.BoxIl2CppObject());
    }
}
