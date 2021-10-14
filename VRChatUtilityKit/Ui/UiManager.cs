using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
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
        /// The current MenuStateController used by VRChat
        /// </summary>
        public static MenuStateController CurrentMenuStateController { get; private set; }

        private static Type _quickMenuContextualDisplayEnum;
        private static MethodBase _setContextMethod;

        internal static void Init()
        {
            InitializeUIManagerUtils();

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

            VRChatUtilityKitMod.Instance.HarmonyInstance.Patch(typeof(UIPage).GetMethod("Show"), new HarmonyMethod(typeof(UiManager).GetMethod(nameof(OnUIPageToggle), BindingFlags.NonPublic | BindingFlags.Static)));

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

            CurrentMenuStateController = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)").GetComponent<MenuStateController>();

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
        public static void OpenUserInQuickMenu(IUser playerToSelect) => VRCData.UserSelection.SelectUser(playerToSelect);

        #region All the unobfuscated shit in the UIManager
        private static Type uIManagerType;

        // I'm lazy ok
        private static readonly Dictionary<string, MethodInfo> nameToMethodTable = new Dictionary<string, MethodInfo>();

        private static void AddToNameToMethodTable(string name)
        {
            MethodInfo method = uIManagerType.GetMethod(name);
            if (method == null)
            {
                MelonLoader.MelonLogger.Error($"Could not find unobfuscated method with name {name} in UIManager");
                return;
            }

            nameToMethodTable.Add(name, method);
        }

        private static void InitializeUIManagerUtils()
        {
            uIManagerType = typeof(Player).Assembly.GetTypes().First(type => type.IsSubclassOf(typeof(UIManager)));

            AddToNameToMethodTable("AskConfirmOpenURL");
            AddToNameToMethodTable("CloseMenu");
            AddToNameToMethodTable("GoToHomeWorld");
            AddToNameToMethodTable("OpenActionMenu");
            AddToNameToMethodTable("OpenAvatarsMenu");
            AddToNameToMethodTable("OpenCannedWorldSearch");
            AddToNameToMethodTable("OpenChangeStatusMenu");
            AddToNameToMethodTable("OpenCreateNewWorldInstanceMenu");
            AddToNameToMethodTable("OpenCurrentWorldMenu");
            AddToNameToMethodTable("OpenEmotesActionMenu");
            AddToNameToMethodTable("OpenGalleryCameraMenu");
            AddToNameToMethodTable("OpenGalleryMenu");
            AddToNameToMethodTable("OpenMainMenuTab");
            AddToNameToMethodTable("OpenProfileMenu");
            AddToNameToMethodTable("OpenQuickMenuTab");
            AddToNameToMethodTable("OpenSafetyMenu");
            AddToNameToMethodTable("OpenSettingsMenu");
            AddToNameToMethodTable("OpenSocialMenu");
            AddToNameToMethodTable("OpenUserIconCameraMenu");
            AddToNameToMethodTable("OpenViewWorldAuthorMenu");
            AddToNameToMethodTable("OpenVRCPlusMenu");
            AddToNameToMethodTable("OpenWorldsMenu");
            AddToNameToMethodTable("ShowAlert");
        }

        /// <summary>
        /// Opens the popup to confirm opening a link with the given link.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        /// <param name="link">The link to open</param>
        public static void AskConfirmOpenURL(string link) => nameToMethodTable[nameof(AskConfirmOpenURL)].Invoke(UIManager.Instance, new object[1] { link });

        /// <summary>
        /// Closes the QuickMenu or big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void CloseMenu() => nameToMethodTable[nameof(CloseMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Goes to the home world WITHOUT opening the confirmation popup.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void GoToHomeWorld() => nameToMethodTable[nameof(GoToHomeWorld)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the ActionMenu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenActionMenu() => nameToMethodTable[nameof(OpenActionMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the Avatar big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenAvatarsMenu() => nameToMethodTable[nameof(OpenAvatarsMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the World Search big menu and searches the given searchTerm.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        /// <param name="searchTerm">The term to search</param>
        public static void OpenCannedWorldSearch(string searchTerm) => nameToMethodTable[nameof(OpenCannedWorldSearch)].Invoke(UIManager.Instance, new object[1] { searchTerm });

        /// <summary>
        /// Opens the Social big menu along with the Update Status popup.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenChangeStatusMenu() => nameToMethodTable[nameof(OpenChangeStatusMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the WorldInfo big menu along with the NewInstance popup.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenCreateNewWorldInstanceMenu() => nameToMethodTable[nameof(OpenCreateNewWorldInstanceMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the current world in the WorldInfo big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenCurrentWorldMenu() => nameToMethodTable[nameof(OpenCurrentWorldMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the avatar emotes ActionMenu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenEmotesActionMenu() => nameToMethodTable[nameof(OpenEmotesActionMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the Add Photo to Gallery menu in the QuickMenu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenGalleryCameraMenu() => nameToMethodTable[nameof(OpenGalleryCameraMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the Gallery big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenGalleryMenu() => nameToMethodTable[nameof(OpenGalleryMenu)].Invoke(UIManager.Instance, null);

        // TODO: asdlkfja;sdlkfj;alskdjf;
        /// <summary>
        /// Opens the UserInfo big menu on the current user.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenMainMenuTab() => nameToMethodTable[nameof(OpenMainMenuTab)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the current user in the WorldInfo big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenProfileMenu() => nameToMethodTable[nameof(OpenProfileMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens given page in the QuickMenu.
        /// A list of valid pages can be found in the CurrentMenuStateController's _wings dictionary.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        /// <param name="pageName">The page to open</param>
        public static void OpenQuickMenuTab(string pageName) => nameToMethodTable[nameof(OpenQuickMenuTab)].Invoke(UIManager.Instance, new object[1] { pageName });

        /// <summary>
        /// Opens the Safety big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenSafetyMenu() => nameToMethodTable[nameof(OpenSafetyMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the Settings big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenSettingsMenu() => nameToMethodTable[nameof(OpenSettingsMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the Social big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenSocialMenu() => nameToMethodTable[nameof(OpenSocialMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the UserIcon menu in the QuickMenu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenUserIconCameraMenu() => nameToMethodTable[nameof(OpenUserIconCameraMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the current world author in the big menu. (I think) Equal to the on click of the world author button in the worlds menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenViewWorldAuthorMenu() => nameToMethodTable[nameof(OpenViewWorldAuthorMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the VRC+ big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenVRCPlusMenu() => nameToMethodTable[nameof(OpenVRCPlusMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Opens the worlds big menu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons. Please report to me any weirdness this method has.)
        /// </summary>
        public static void OpenWorldsMenu() => nameToMethodTable[nameof(OpenWorldsMenu)].Invoke(UIManager.Instance, null);

        /// <summary>
        /// Shows an alert at the bottom of the QuickMenu.
        /// (This is a direct API to VRChat's UIManager that I could not expose directly for a few reasons)
        /// </summary>
        /// <param name="alertText">The text to show</param>
        public static void ShowAlert(string alertText) => nameToMethodTable[nameof(ShowAlert)].Invoke(UIManager.Instance, new object[1] { alertText });
        #endregion

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
