using System;
using UnhollowerBaseLib;
using VRC.UI.Elements;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper that holds a tab menu 
    /// </summary>
    public class TabMenu : SubMenu
    {
        internal TabMenu(string name, string pageName) : base(name, pageName)
        {
            Il2CppReferenceArray<UIPage> rootPages = UiManager.QMStateController.field_Public_ArrayOf_UIPage_0;
            Il2CppReferenceArray<UIPage> newRootPages = new Il2CppReferenceArray<UIPage>(rootPages.Count + 1);
            for (int i = 0; i < rootPages.Count; i++)
                newRootPages[i] = rootPages[i];
            newRootPages[rootPages.Count] = uiPage;
            UiManager.QMStateController.field_Public_ArrayOf_UIPage_0 = newRootPages;
        }
        
        /// <summary>
        /// Opens a the specified menu as a sub menu of the tab menu.
        /// </summary>
        /// <param name="uiPage">The page to open</param>
        public void OpenSubMenu(UIPage uiPage) => UiManager._pushPageMethod.Invoke(uiPage, new object[1] { uiPage });

        /// <summary>
        /// Opens a the specified menu as a sub menu of the tab menu.
        /// </summary>
        /// <param name="subMenu">The menu to open</param>
        public void OpenSubMenu(SubMenu subMenu) => OpenSubMenu(subMenu.uiPage);

        /// <summary>
        /// Closes all sub menus of the tab menu.
        /// </summary>
        public void CloseAllSubMenus() => uiPage.Method_Public_Void_Predicate_1_UIPage_0(null);

        /// <summary>
        /// Closes the most recently open sub menu of the tab menu.
        /// </summary>
        public void PopSubMenu() => UiManager._removePageMethod.Invoke(uiPage, new object[1] { CurrentPage() });

        /// <summary>
        /// Returns the most recently open menu of the tab menu.
        /// </summary>
        /// <returns>The most recently open menu of the tab menu</returns>
        public UIPage CurrentPage() => uiPage.field_Private_List_1_UIPage_0[uiPage.field_Private_List_1_UIPage_0.Count - 1];

        /// <summary>
        /// Removes the given sub menu from the tab menu's stack.
        /// </summary>
        /// <param name="uiPage">The page to remove</param>
        /// <returns>The page that was removed</returns>
        public UIPage RemovePageFromStack(UIPage uiPage) => this.uiPage.Method_Private_UIPage_UIPage_0(uiPage);
        /// <summary>
        /// Removes the given sub menu from the tab menu's stack.
        /// </summary>
        /// <param name="subMenu">The menu to remove</param>
        /// <returns>The page that was removed</returns>
        public UIPage RemovePageFromStack(SubMenu subMenu) => RemovePageFromStack(subMenu.uiPage);

        /// <summary>
        /// Goes to the given page in the tab menu's stack.
        /// Closes any other pages above it in the stack.
        /// The given page must already be in the stack.
        /// </summary>
        /// <param name="uiPage">The page to open</param>
        public void GoBackToMenu(UIPage uiPage)
        {
            bool isInStack = false;
            foreach (UIPage stackPage in this.uiPage.field_Private_List_1_UIPage_0)
            {
                if (stackPage.field_Public_String_0 == uiPage.field_Public_String_0)
                {
                    isInStack = true;
                    break;
                }
            }
            if (!isInStack)
                throw new ArgumentException("Given UIPage was not in the screen stack");

            while (CurrentPage().field_Public_String_0 != uiPage.field_Public_String_0)
                PopSubMenu();
        }

        /// <summary>
        /// Goes to the given page in the tab menu's stack.
        /// Closes any other pages above it in the stack.
        /// The given page must already be in the stack.
        /// </summary>
        /// <param name="subMenu">The menu to open</param>
        public void GoBackToMenu(SubMenu subMenu) => GoBackToMenu(subMenu.uiPage);
    }
}
