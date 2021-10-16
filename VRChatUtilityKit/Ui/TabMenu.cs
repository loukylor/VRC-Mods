using System;
using UnhollowerBaseLib;
using UnityEngine;
using VRC.UI.Elements;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit.Ui
{
    public class TabMenu : SubMenu
    {
        internal TabMenu(GameObject subMenuBase, GameObject parent, string name, string pageName) : base(subMenuBase, parent, name, pageName)
        {
            Il2CppReferenceArray<UIPage> rootPages = UiManager.QMStateController.menuRootPages;
            Il2CppReferenceArray<UIPage> newRootPages = new Il2CppReferenceArray<UIPage>(rootPages.Count + 1);
            rootPages.CopyTo(newRootPages, 0);
            newRootPages[rootPages.Count] = uiPage;
            UiManager.QMStateController.menuRootPages = newRootPages;
        }

        public void OpenSubMenu(UIPage uiPage) => this.uiPage.PushPage(uiPage);
        public void OpenSubMenu(SubMenu subMenu) => uiPage.PushPage(subMenu.uiPage);

        public void CloseAllSubMenus() => uiPage.ClearPageStack();

        public void PopSubMenu() => uiPage.PopPage();

        public UIPage CurrentPage() => uiPage.CurrentPage();

        public UIPage RemovePageFromStack(UIPage uiPage) => this.uiPage.RemovePageFromStack(uiPage);
        public UIPage RemovePageFromStack(SubMenu subMenu) => uiPage.RemovePageFromStack(subMenu.uiPage);

        public void GoBackToMenu(UIPage uiPage)
        {
            bool isInStack = false;
            foreach (UIPage stackPage in this.uiPage._pageStack)
            {
                if (stackPage.Name == uiPage.Name)
                {
                    isInStack = true;
                    break;
                }
            }
            if (!isInStack)
                throw new ArgumentException("Given UIPage was not in the screen stack");

            while (this.uiPage.CurrentPage().Name != uiPage.Name)
                this.uiPage.PopPage();
        }
        public void GoBackToMenu(SubMenu subMenu) => GoBackToMenu(subMenu.uiPage);
    }
}
