using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;
using VRC.UI.Elements.Controls;

#pragma warning disable IDE1006 // Naming Styles

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper holding a sub menu.
    /// </summary>
    public class SubMenu : ElementBase, IElementWithText
    {
        /// <summary>
        /// The UIPage attached to the sub menu.
        /// </summary>
        public UIPage uiPage { get; private set; }

        /// <summary>
        /// The VerticalLayoutGroup that holds all the buttons and elements of the sub menu.
        /// </summary>
        public VerticalLayoutGroup PageLayoutGroup { get; private set; }

        /// <summary>
        /// The back button of the sub menu.
        /// </summary>
        public GameObject BackButton { get; private set; } 

        /// <summary>
        /// The Text component of the title of the sub menu.
        /// </summary>
        public TextMeshProUGUI TextComponent { get; private set; }
        /// <summary>
        /// Gets or sets the title of the sub menu.
        /// </summary>
        public string Text
        {
            get => TextComponent.text;
            set => TextComponent.text = value;
        }

        /// <summary>
        /// The list of button groups on this menu.
        /// </summary>
        [Obsolete("Use ButtonRow instead. Button groups could cause overlaps.")]
        public IReadOnlyList<ButtonGroup> ButtonGroups => _buttonGroups;
        [Obsolete]
        private readonly List<ButtonGroup> _buttonGroups = new List<ButtonGroup>();

        /// <summary>
        /// The list of button rows on this menu.
        /// </summary>
        public IReadOnlyList<ButtonRow> ButtonRows => _buttonRows;
        private readonly List<ButtonRow> _buttonRows = new List<ButtonRow>();

        /// <summary>
        /// Creates a new sub menu.
        /// </summary>
        /// <param name="parent">The parent of the sub menu</param>
        /// <param name="pageName">The name of the sub menu's page</param>
        /// <param name="gameObjectName">The name of the sub menu's GameObject</param>
        /// <param name="headerText">The text of the sub menu's header</param>
        /// <param name="creationCallback">An action that is called with the submenu object when the submenu is created</param>
        public SubMenu(Transform parent, string pageName, string gameObjectName, string headerText, Action<SubMenu> creationCallback = null) : base(parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard").gameObject, gameObjectName)
        {
            GameObject.DestroyImmediate(gameObject.GetComponent<UIPage>());
            GameObject.Destroy(rectTransform.Find("Header_H1/RightItemContainer/Button_QM_Expand").gameObject); // Dunno how the binding class works so

            PageLayoutGroup = rectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup").GetComponent<VerticalLayoutGroup>();
            for (int i = PageLayoutGroup.rectTransform.childCount - 1; i >= 0; i--)
                GameObject.DestroyImmediate(PageLayoutGroup.transform.GetChild(i).gameObject);
            PageLayoutGroup.childControlHeight = true;

            BackButton = rectTransform.Find("Header_H1/LeftItemContainer/Button_Back").gameObject;
            BackButton.gameObject.SetActive(true);
            TextComponent = rectTransform.Find("Header_H1/LeftItemContainer/Text_Title").GetComponent<TextMeshProUGUI>();
            Text = headerText;
            uiPage = gameObject.AddComponent<UIPage>();
            uiPage.field_Private_MenuStateController_0 = UiManager.QMStateController;
            uiPage.field_Public_String_0 = pageName;

            UiManager.QMStateController.field_Private_Dictionary_2_String_UIPage_0.Add(pageName, uiPage);

            creationCallback?.Invoke(this);
        }
        /// <summary>
        /// Creates a new sub menu.
        /// </summary>
        /// <param name="pageName">The name of the sub menu's page</param>
        /// <param name="gameObjectName">The name of the sub menu's GameObject</param>
        /// <param name="headerText">The text of the sub menu's header</param>
        /// <param name="creationCallback">An action that is called with the submenu object when the submenu is created</param>
        public SubMenu(string pageName, string gameObjectName, string headerText, Action<SubMenu> creationCallback = null) : this(UiManager.QMStateController.transform.Find("Container/Window/QMParent"), pageName, gameObjectName, headerText, creationCallback) { }

        #region obsolete things

        /// <summary>
        /// Adds the given button group to the sub menu.
        /// </summary>
        /// <param name="buttonGroup">The button group to add</param>
        [Obsolete("Use ButtonRow instead. Button groups could cause overlaps.")]
        public void AddButtonGroup(ButtonGroup buttonGroup)
        {
            buttonGroup.ParentMenu = this;
            if (buttonGroup.Header != null)
                buttonGroup.Header.rectTransform.parent = PageLayoutGroup.rectTransform;
            buttonGroup.rectTransform.parent = PageLayoutGroup.rectTransform;
            _buttonGroups.Add(buttonGroup);
        }

        /// <summary>
        /// Adds the given range of button groups to the sub menu.
        /// </summary>
        /// <param name="buttonGroups">The range of button groups to add</param>
        [Obsolete("Use ButtonRow instead. Button groups could cause overlaps.")]
        public void AddButtonGroupRange(IEnumerable<ButtonGroup> buttonGroups)
        {
            foreach (ButtonGroup buttonGroup in buttonGroups)
                AddButtonGroup(buttonGroup);
        }

        /// <summary>
        /// Removes the given button group from the sub menu.
        /// </summary>
        /// <param name="buttonGroup">The button group to remove</param>
        [Obsolete("Use ButtonRow instead. Button groups could cause overlaps.")]
        public void RemoveButtonGroup(ButtonGroup buttonGroup)
        {
            _buttonGroups.Remove(buttonGroup);
            GameObject.DestroyImmediate(buttonGroup.gameObject);
        }

        /// <summary>
        /// Removes all button groups from the submenu.
        /// </summary>
        [Obsolete("Use ButtonRow instead. Button groups could cause overlaps.")]
        public void ClearButtonGroups()
        {
            foreach (ButtonGroup buttonGroup in _buttonGroups)
            {
                if (buttonGroup.Header != null)
                    GameObject.DestroyImmediate(buttonGroup.Header.gameObject);
                GameObject.DestroyImmediate(buttonGroup.gameObject);
            }
            _buttonGroups.Clear();
        }

        #endregion

        /// <summary>
        /// Adds the given button group to the sub menu.
        /// </summary>
        /// <param name="buttonRow">The button group to add</param>
        public void AddButtonRow(ButtonRow buttonRow)
        {
            buttonRow.ParentMenu = this;
            if (buttonRow.Header != null)
                buttonRow.Header.rectTransform.parent = PageLayoutGroup.rectTransform;
            buttonRow.rectTransform.parent = PageLayoutGroup.rectTransform;
            _buttonRows.Add(buttonRow);
        }

        /// <summary>
        /// Adds the given range of button groups to the sub menu.
        /// </summary>
        /// <param name="buttonRows">The range of button groups to add</param>
        public void AddButtonRowRange(IEnumerable<ButtonRow> buttonRows)
        {
            foreach (ButtonRow buttonRow in buttonRows)
                AddButtonRow(buttonRow);
        }

        /// <summary>
        /// Removes the given button group from the sub menu.
        /// </summary>
        /// <param name="buttonrow">The button group to remove</param>
        public void RemoveButtonRow(ButtonRow buttonrow)
        {
            _buttonRows.Remove(buttonrow);
            GameObject.DestroyImmediate(buttonrow.gameObject);
        }

        /// <summary>
        /// Removes all button groups from the submenu.
        /// </summary>
        public void ClearButtonRows()
        {
            foreach (ButtonRow buttonRow in _buttonRows)
            {
                if (buttonRow.Header != null)
                    GameObject.DestroyImmediate(buttonRow.Header.gameObject);
                GameObject.DestroyImmediate(buttonRow.gameObject);
            }
            _buttonRows.Clear();
        }

        /// <summary>
        /// Toggles the 
        /// </summary>
        /// <param name="active"></param>
        public void ToggleScrollbar(bool active)
        {
            UiManager.ToggleScrollRectOnExistingMenu(PageLayoutGroup.gameObject, active);
        }
    }
}
