using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using PlayerList.Utilities;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerList.UI
{
    // This "Button API", if you can it that, is based off of RubyButtonAPI, by DubyaDude (dooba lol) (https://github.com/DubyaDude)
    public class UIManager
    {
        private static MethodInfo openQuickMenu;
        private static MethodInfo closeQuickMenu;
        private static MethodInfo setMenuIndex;
        private static PropertyInfo quickMenuEnumProperty;

        private static Il2CppSystem.Reflection.MethodInfo showTabContentMethod;
        private static Il2CppSystem.Reflection.FieldInfo setTabTabIndexField;
        private static Il2CppSystem.Reflection.FieldInfo setTabIndexField;
        private static Il2CppSystem.Type tabDescriptorType;
        private static Component tabContainerComponent;
        private static Il2CppSystem.Reflection.FieldInfo existingTabsField;
        public static List<GameObject> ExistingTabs
        {
            get { return existingTabsField.GetValue(tabContainerComponent).Cast<Il2CppReferenceArray<GameObject>>().ToList(); }
            set { existingTabsField.SetValue(tabContainerComponent, new Il2CppReferenceArray<GameObject>(value.ToArray()).Cast<Il2CppSystem.Object>()); }
        }

        public static Sprite fullOnButtonSprite;
        public static Sprite regularButtonSprite;

        public static event Action OnQuickMenuOpenEvent;
        public static event Action OnQuickMenuCloseEvent;

        private static PropertyInfo currentMenuField;
        public static GameObject CurrentMenu
        {
            get
            {
                if (currentMenuField == null)
                    return null;
                else
                    return (GameObject)currentMenuField.GetValue(QuickMenu.prop_QuickMenu_0);
            }
            set { currentMenuField?.SetValue(QuickMenu.prop_QuickMenu_0, value); }
        }
        private static PropertyInfo currentTabMenuField;
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
        private static Type QuickMenuContextualDisplayEnum;
        private static MethodBase QuickMenuContexualDisplayMethod;

        public static void Init()
        {
            List<Type> quickMenuNestedEnums = typeof(QuickMenu).GetNestedTypes().Where(type => type.IsEnum).ToList();
            quickMenuEnumProperty = typeof(QuickMenu).GetProperties()
                .First(pi => pi.PropertyType.IsEnum && quickMenuNestedEnums.Contains(pi.PropertyType));
            setMenuIndex = typeof(QuickMenu).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Enum") && mb.GetParameters()[0].ParameterType == quickMenuEnumProperty.PropertyType);

            closeQuickMenu = typeof(QuickMenu).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && mb.Name.Length <= 29 && Xref.CheckUsed(mb, setMenuIndex.Name));

            foreach (MethodInfo method in typeof(QuickMenu).GetMethods().Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && mb.Name.Length <= 29 && !mb.Name.Contains("PDM")))
            {
                MethodBase[] possibleMethods = null;
                try
                {
                    possibleMethods = XrefScanner.UsedBy(method)
                        .Where(instance => instance.Type == XrefType.Method && instance.TryResolve() != null && instance.TryResolve().Name.StartsWith("Method_Public_Void_") && !instance.TryResolve().Name.Contains("_PDM_") && instance.TryResolve().GetParameters().Length == 0)
                        .Select(instance => instance.TryResolve())
                        .ToArray();
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                if (possibleMethods.Length == 0)
                    continue;

                foreach (MethodInfo possibleMethod in possibleMethods)
                {
                    if (XrefScanner.UsedBy(possibleMethod).Any(instance => instance.Type == XrefType.Method && instance.TryResolve() != null && instance.TryResolve().Name.Contains("OpenQuickMenu")))
                    {
                        openQuickMenu = method;
                        break;
                    }
                }

                if (openQuickMenu != null)
                    break;
            }

            QuickMenuContextualDisplayEnum = typeof(QuickMenuContextualDisplay).GetNestedTypes()
                .First(type => type.Name.StartsWith("Enum"));

            QuickMenuContexualDisplayMethod = typeof(QuickMenuContextualDisplay).GetMethod($"Method_Public_Void_{QuickMenuContextualDisplayEnum.Name}_0");

            PlayerListMod.Instance.HarmonyInstance.Patch(openQuickMenu, null, new HarmonyMethod(typeof(UIManager).GetMethod(nameof(OnQuickMenuOpen), BindingFlags.NonPublic | BindingFlags.Static)));
            PlayerListMod.Instance.HarmonyInstance.Patch(closeQuickMenu, null, new HarmonyMethod(typeof(UIManager).GetMethod(nameof(OnQuickMenuClose), BindingFlags.NonPublic | BindingFlags.Static)));
        }
        public static void Print(MethodInfo __originalMethod)
        {
            MelonLogger.Msg(__originalMethod.Name);
        }
        public static void UIInit()
        {
            GameObject tabContainer = GameObject.Find("UserInterface/QuickMenu/QuickModeTabs");
            foreach (Component component in tabContainer.GetComponents<Component>())
            {
                if (!component.GetIl2CppType().FullName.Contains("UnityEngine") && component.GetIl2CppType().GetMethods().Any(mi => mi.Name == "ShowTabContent"))
                {
                    tabContainerComponent = component;
                    setTabIndexField = component.GetIl2CppType().GetFields(Il2CppSystem.Reflection.BindingFlags.NonPublic | Il2CppSystem.Reflection.BindingFlags.Instance).First(f => f.FieldType.IsEnum);
                    foreach (Il2CppSystem.Reflection.FieldInfo field in component.GetIl2CppType().GetFields())
                    { 
                        if (field.FieldType.IsArray)
                        { 
                            existingTabsField = field;
                            break;
                        }
                    }
                    break;
                }
            }

            MonoBehaviour tabDescriptor = tabContainer.transform.GetChild(0).gameObject.GetComponents<MonoBehaviour>().First(c => c.GetIl2CppType().GetMethod("ShowTabContent") != null);

            tabDescriptorType = tabDescriptor.GetIl2CppType();
            showTabContentMethod = tabDescriptorType.GetMethod("ShowTabContent");
            setTabTabIndexField = tabDescriptorType.GetFields().First(f => f.FieldType.IsEnum);

            ButtonReaction buttonReaction = GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton").GetComponent<ButtonReaction>();
            fullOnButtonSprite = buttonReaction.field_Public_Sprite_0.name.Contains("Full_ON") ? buttonReaction.field_Public_Sprite_0 : buttonReaction.field_Public_Sprite_1;
            regularButtonSprite = buttonReaction.field_Public_Sprite_0.name.Contains("White") ? buttonReaction.field_Public_Sprite_0 : buttonReaction.field_Public_Sprite_1;

            if (currentMenuField != null) return;

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
                setMenuIndex.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { 8 });
            }
            catch { } // Ignore error cuz it still sets the menu

            // Find out which menu actually got set
            foreach (PropertyInfo prop in possibleProps)
                if (prop.GetValue(QuickMenu.prop_QuickMenu_0) != null)
                    currentMenuField = prop;

            if (currentMenuField == null) MelonLogger.Error("Something went wrong. In technical speak: after attempting to determine the current menu field info, it was null. The mod will likely not function properly");

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
                if (prop.Name != currentMenuField.Name && prop.GetValue(QuickMenu.prop_QuickMenu_0) != null)
                    currentTabMenuField = prop;

            if (currentMenuField == null) MelonLogger.Error("Something went wrong. In technical speak: after attempting to determine the current tab field info, it was null. The mod will likely not function properly");

            CurrentTabMenu.SetActive(false);

            // Set to null as to not change values unexpectedly 
            foreach (PropertyInfo prop in possibleProps)
                prop.SetValue(QuickMenu.prop_QuickMenu_0, null);
            quickMenuEnumProperty.SetValue(QuickMenu.prop_QuickMenu_0, -1);
        }
        private static void OnQuickMenuOpen() => OnQuickMenuOpenEvent?.SafeInvoke();
        private static void OnQuickMenuClose() => OnQuickMenuCloseEvent?.SafeInvoke();
        public static void SetMenuIndex(int index) => setMenuIndex.Invoke(QuickMenu.prop_QuickMenu_0, new object[1] { index });
        public static void SetTabTabIndex(GameObject tab, int index) => setTabTabIndexField.SetValue(tab.gameObject.GetComponent(tabDescriptorType), new Il2CppSystem.Int32 { m_value = index }.BoxIl2CppObject());
        public static void ShowTabContent(GameObject tab) => showTabContentMethod.Invoke(tab.gameObject.GetComponent(tabDescriptorType), null);
        public static void SetTabIndex(int index) => setTabIndexField.SetValue(tabContainerComponent, new Il2CppSystem.Int32 { m_value = index }.BoxIl2CppObject());

        public static void OpenPage(string page, bool setCurrentMenu = true, bool setCurrentTab = true)
        {
            GameObject pageGameObject = GameObject.Find(page);
            if (pageGameObject == null)
            {
                MelonLogger.Error($"Page with path {page} could not be found");
                return;
            }

            CurrentMenu?.SetActive(false);

            if (page.Split('/').Last() == "ShortcutMenu")
            {
                ShowTabContent(ExistingTabs[0]);
                SetMenuIndex(0);
                SetTabIndex(0);
            }
            else
            {
                quickMenuEnumProperty.SetValue(QuickMenu.prop_QuickMenu_0, -1);
            }

            if (setCurrentMenu)
                CurrentMenu = pageGameObject;

            CurrentTabMenu?.SetActive(false);
            if (setCurrentTab)
                CurrentTabMenu = pageGameObject;

            QuickMenuContextualDisplay quickMenuContextualDisplay = QuickMenu.prop_QuickMenu_0.field_Private_QuickMenuContextualDisplay_0;
            QuickMenuContexualDisplayMethod.Invoke(quickMenuContextualDisplay, new object[] { QuickMenuContextualDisplayEnum.GetEnumValues().GetValue(Array.IndexOf(QuickMenuContextualDisplayEnum.GetEnumNames(), "NoSelection")) });

            pageGameObject.SetActive(true);
        }
    }
}
