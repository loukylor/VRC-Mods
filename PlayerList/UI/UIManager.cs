using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using PlayerList.Utilities;
using UnityEngine;

namespace PlayerList.UI
{
    // This "Button API", if you can it that, is based off of RubyButtonAPI, by DubyaDude (dooba lol) (https://github.com/DubyaDude)
    public class UIManager
    {
        private static MethodInfo openQuickMenu;
        private static MethodInfo closeQuickMenu;
        private static MethodInfo setMenuIndex;

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
            set { currentMenuField.SetValue(QuickMenu.prop_QuickMenu_0, value); }
        }
        public static GameObject lastMenu;
        private static Type QuickMenuContextualDisplayEnum;
        private static MethodBase QuickMenuContexualDisplayMethod;

        public static void Init()
        {
            closeQuickMenu = typeof(QuickMenu).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && mb.Name.Length <= 29 && !mb.Name.Contains("PDM") && Xref.CheckUsed(mb, "Method_Public_Void_Int32_Boolean_")).First();

            openQuickMenu = typeof(QuickMenu).GetMethods()
                 .Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && mb.Name.Length <= 29 && mb.GetParameters().Any(pi => pi.HasDefaultValue == false)).First();

            setMenuIndex = typeof(QuickMenu).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_Int32_") && mb.Name.Length <= 27 && Xref.CheckUsed(mb, "Method_Public_Void_Boolean_")).First();

            QuickMenuContextualDisplayEnum = typeof(QuickMenuContextualDisplay).GetNestedTypes()
                .Where(type => type.Name.StartsWith("Enum")).First();

            QuickMenuContexualDisplayMethod = typeof(QuickMenuContextualDisplay).GetMethod($"Method_Public_Void_{QuickMenuContextualDisplayEnum.Name}_0");
        }
        public static void UIInit(HarmonyInstance harmonyInstance)
        {
            // Check which fields return null
            List<PropertyInfo> possibleProps = new List<PropertyInfo>();
            foreach (PropertyInfo prop in typeof(QuickMenu).GetProperties().Where(pi => pi.Name.StartsWith("field_Private_GameObject_")))
            {
                GameObject value = (GameObject)prop.GetValue(QuickMenu.prop_QuickMenu_0);
                if (value == null) possibleProps.Add(prop);
            }

            // Open QuickMenu to set current menu
            try
            {
                setMenuIndex.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { 2 });
            }
            catch { } // Ignore error cuz it still sets the menu

            // Find out which menu actually got set
            foreach (PropertyInfo prop in possibleProps)
                if (prop.GetValue(QuickMenu.prop_QuickMenu_0) != null) currentMenuField = prop;

            // Close QM so it doesn't float in front of your face
            try
            {
                openQuickMenu.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { false });
            }
            catch { }
            try
            {
                closeQuickMenu.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { false });
            }
            catch { } // Ignore error cuz it still closes the menu

            // Set to null as to not change values unexpectedly 
            foreach (PropertyInfo prop in possibleProps)
                prop.SetValue(QuickMenu.prop_QuickMenu_0, null);

            harmonyInstance.Patch(openQuickMenu, null, new HarmonyMethod(typeof(UIManager).GetMethod(nameof(OnQuickMenuOpen))));
            harmonyInstance.Patch(closeQuickMenu, null, new HarmonyMethod(typeof(UIManager).GetMethod(nameof(OnQuickMenuClose))));
        }
        public static void OnQuickMenuOpen() => OnQuickMenuOpenEvent?.Invoke();
        public static void OnQuickMenuClose() => OnQuickMenuCloseEvent?.Invoke();
        public static void SetMenuIndex(int index) => setMenuIndex.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { index });
        public static void OpenPage(string page)
        {
            GameObject pageGameObject = GameObject.Find(page);
            if (pageGameObject == null)
            {
                MelonLoader.MelonLogger.Error($"Page with path {page} could not be found");
                return;
            }
            CurrentMenu.SetActive(false);

            if (page.Split('/').Last() == "ShortcutMenu")
            { 
                SetMenuIndex(0);
            }
            else
            {
                QuickMenu.prop_QuickMenu_0.field_Private_Int32_0 = -1;
                CurrentMenu = pageGameObject;
            }

            QuickMenuContextualDisplay quickMenuContextualDisplay = QuickMenu.prop_QuickMenu_0.field_Private_QuickMenuContextualDisplay_0;
            QuickMenuContexualDisplayMethod.Invoke(quickMenuContextualDisplay, new object[] { QuickMenuContextualDisplayEnum.GetEnumValues().GetValue(Array.IndexOf(QuickMenuContextualDisplayEnum.GetEnumNames(), "NoSelection")) });

            pageGameObject.SetActive(true);
        }
    }
}
