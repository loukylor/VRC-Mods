using System;
using UnityEngine;
using VRC.UI.Elements.Controls;

namespace VRChatUtilityKit.Ui
{
    // Credit for Slaynash (https://gist.github.com/Slaynash/018d6de4e8d27faf08c0fe4a6c2854de) to act as reference
    /// <summary>
    /// A wrapper that holds a tab button.
    /// </summary>
    public class TabButton : VRCSelectable
    {
        /// <summary>
        /// The tab menu attached to the tab button.
        /// </summary>
        public TabMenu SubMenu { get; private set; }

        /// <summary>
        /// The MenuTab component on the tab button.
        /// </summary>
        public MenuTab MenuTab { get; private set; }

        /// <summary>
        /// Creates a new tab button.
        /// </summary>
        /// <param name="sprite">The sprite of the tab button</param>
        /// <param name="pageName">The name of the tab menu's page</param>
        /// <param name="gameObjectName">The name of the tab button's GameObject</param>
        /// <param name="tooltipText">The tooltip of the tab button</param>
        /// <param name="headerText">The text of the sub menu's header</param>
        /// <param name="creationCallback">An action that is called with the tabbutton object when the tabbutton is created</param>
        public TabButton(Sprite sprite, string pageName, string gameObjectName, string headerText, string tooltipText = "", Action<TabButton> creationCallback = null) : base(UiManager.QMStateController.transform.Find("Container/Window/Page_Buttons_QM/HorizontalLayoutGroup"), UiManager.QMStateController.transform.Find("Container/Window/Page_Buttons_QM/HorizontalLayoutGroup/Page_Dashboard").gameObject, sprite, gameObjectName, tooltipText)
        {
            MenuTab = gameObject.GetComponent<MenuTab>();
            MenuTab.field_Private_MenuStateController_0 = UiManager.QMStateController;
            MenuTab.field_Public_String_0 = pageName;

            SubMenu = new TabMenu(pageName, $"Menu_{pageName}", headerText);

            creationCallback?.Invoke(this);
        }
    }
}
