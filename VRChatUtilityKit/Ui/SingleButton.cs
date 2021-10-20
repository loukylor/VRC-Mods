using System;
using TMPro;
using UnityEngine;
using VRC.DataModel.Core;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper holding a button.
    /// </summary>
    public class SingleButton : VRCSelectable, IElementWithText
    {
        /// <summary>
        /// The OnClick of the button.
        /// </summary>
        public Action OnClick { get; set; }

        /// <summary>
        /// A small icon indicating this buttons jumps to the big menu.
        /// </summary>
        public GameObject JumpBadge { get; private set; }

        /// <summary>
        /// The text mesh pro component on the button.
        /// </summary>
        public TextMeshProUGUI TextComponent { get; private set; }

        /// <summary>
        /// Gets or sets the text on the button.
        /// </summary>
        public string Text 
        { 
            get => TextComponent.text; 
            set => TextComponent.text = value; 
        }

        /// <summary>
        /// Creates a new button.
        /// </summary>
        /// <param name="onClick">The OnClick of the button</param>
        /// <param name="icon">The icon for the button</param>
        /// <param name="text">The text of the button</param>
        /// <param name="gameObjectName">The name of the button's GameObject</param>
        public SingleButton(Action onClick, Sprite icon, string text, string gameObjectName) : base(null, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Worlds").gameObject, icon, gameObjectName)
        {
            JumpBadge = rectTransform.Find("Badge_MMJump").gameObject;
            JumpBadge.SetActive(false);
            TextComponent = rectTransform.Find("Text_H4").GetComponent<TextMeshProUGUI>();
            Text = text;
            OnClick = onClick;
            BindingExtensions.Bind(ButtonComponent, new Action(() => OnClick?.Invoke()));
        }
    }
}