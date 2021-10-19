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
        /// <param name="parent">The parent of the button</param>
        /// <param name="onClick">The OnClick of the button</param>
        /// <param name="icon">The icon for the button</param>
        /// <param name="gameObjectName">The name of the button's GameObject</param>
        public SingleButton(GameObject parent, Action onClick, Sprite icon, string gameObjectName) : base(parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Worlds").gameObject, icon, gameObjectName)
        {
            TextComponent = rectTransform.Find("Text_H4").GetComponent<TextMeshProUGUI>();
            OnClick = onClick;
            BindingExtensions.Bind(ButtonComponent, new Action(() => OnClick?.Invoke()));
        }
    }
}