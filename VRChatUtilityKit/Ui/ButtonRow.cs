using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A class that represents an optional button header and row of buttons.
    /// </summary>
    public class ButtonRow : ElementBase
    {
        /// <summary>
        /// The sub menu the group is in.
        /// </summary>
        public SubMenu ParentMenu { get; internal set; }

        /// <summary>
        /// The group's header.
        /// May be null.
        /// </summary>
        public ButtonHeader Header { get; private set; }

        /// <summary>
        /// The layout all the buttons go into.
        /// </summary>
        public GridLayoutGroup ButtonLayoutGroup { get; private set; }

        /// <summary>
        /// The buttons in the group.
        /// </summary>
        public IReadOnlyList<IButtonGroupElement> Buttons => _buttons;
        private readonly List<IButtonGroupElement> _buttons = new List<IButtonGroupElement>();

        /// <summary>
        /// The number of buttons this row can contain.
        /// </summary>
        public int RowWidth { get; private set; }

        /// <summary>
        /// Creates a new button group.
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <param name="headerText">The text of the header</param>
        /// <param name="buttons">The buttons on the group</param>
        /// <param name="rowWidth">The number of buttons the row should hold</param>
        /// <param name="creationCallback">An action that is called with the group object when the group is created</param>
        public ButtonRow(string name, string headerText = null, List<IButtonGroupElement> buttons = null, int rowWidth = 4, Action<ButtonRow> creationCallback = null) : base(null, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks").gameObject, $"Buttons_{name}")
        {
            ButtonLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();
            for (int i = ButtonLayoutGroup.transform.childCount - 1; i >= 0; i--)
                GameObject.DestroyImmediate(ButtonLayoutGroup.transform.GetChild(i).gameObject);
            ButtonLayoutGroup.padding.bottom = 32;
            ButtonLayoutGroup.constraintCount = rowWidth;

            RowWidth = rowWidth;
            if (headerText != null)
                AddButtonHeader(headerText, $"Header_{name}");
            if (buttons != null)
                AddButtonRange(buttons);

            creationCallback?.Invoke(this);
        }

        /// <summary>
        /// Adds a new button header to the group.
        /// </summary>
        /// <param name="text">The text on the header</param>
        /// <param name="gameObjectName">The name of the header GameObject</param>
        public ButtonRow AddButtonHeader(string text, string gameObjectName)
        {
            Header = new ButtonHeader(rectTransform.parent, text, gameObjectName);
            rectTransform.SetSiblingIndex(rectTransform.GetSiblingIndex() + 1);

            return this;
        }

        /// <summary>
        /// Removes the button header from the group.
        /// </summary>
        public ButtonRow RemoveButtonHeader()
        {
            GameObject.DestroyImmediate(Header.gameObject);
            Header = null;

            return this;
        }

        /// <summary>
        /// Adds the given button to the group.
        /// </summary>
        /// <param name="button">The button to add</param>
        public ButtonRow AddButton(IButtonGroupElement button)
        {
            if (_buttons.Count + 1 > RowWidth)
                throw new ArgumentException($"This row can only contain {RowWidth} buttons, but more were added.");

            button.rectTransform.parent = ButtonLayoutGroup.transform;
            _buttons.Add(button);

            return this;
        }

        /// <summary>
        /// Adds the given range of buttons to the group.
        /// </summary>
        /// <param name="buttons">The range of buttons to add</param>
        public ButtonRow AddButtonRange(IEnumerable<IButtonGroupElement> buttons)
        {
            foreach (IButtonGroupElement button in buttons)
                AddButton(button);
            return this;
        }

        /// <summary>
        /// Removes the given button from the group.
        /// </summary>
        /// <param name="button">The button to remove</param>
        public ButtonRow RemoveButton(IButtonGroupElement button)
        {
            _buttons.Remove(button);
            GameObject.DestroyImmediate(button.gameObject);

            return this;
        }

        /// <summary>
        /// Removes all buttons from the group.
        /// </summary>
        public ButtonRow ClearButtons()
        {
            foreach (IButtonGroupElement button in _buttons)
                GameObject.DestroyImmediate(button.gameObject);
            _buttons.Clear();

            return this;
        }
    }
}
