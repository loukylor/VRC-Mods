using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper that holds a UI element that only has text.
    /// </summary>
    public class Label : ElementBase, IElementWithText, IButtonGroupElement
    {
        /// <summary>
        /// The type of button this interface represents.
        /// </summary>
        public ElementType Type => ElementType.Label;

        /// <summary>
        /// The text mesh pro component on the label.
        /// </summary>
        public TextMeshProUGUI TextComponent { get; private set; }

        /// <summary>
        /// Gets or sets the text on the label.
        /// </summary>
        public string Text
        {
            get => TextComponent.text;
            set => TextComponent.text = value;
        }

        /// <summary>
        /// The subtitle text mesh pro component on the label.
        /// </summary>
        public TextMeshProUGUI SubtitleTextComponent { get; private set; }

        /// <summary>
        /// Gets or sets the subtitle text on the label.
        /// </summary>
        public string SubtitleText
        {
            get => SubtitleTextComponent.text;
            set => SubtitleTextComponent.text = value;
        }

        /// <summary>
        /// Creates a new label.
        /// </summary>
        /// <param name="text">The text of the label</param>
        /// <param name="subtitleText">The subtitle text of the label</param>
        /// <param name="gameObjectName">The name of the label's GameObject</param>
        /// <param name="creationCallback">An action that is called with the label object when the label is created</param>
        public Label(string text, string subtitleText, string gameObjectName, Action<Label> creationCallback = null) : base(null, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Settings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/Buttons_Debug/Button_FPS").gameObject, gameObjectName)
        {
            TextComponent = rectTransform.Find("Text_H1").GetComponent<TextMeshProUGUI>();
            Text = text;

            SubtitleTextComponent = rectTransform.Find("Text_H4").GetComponent<TextMeshProUGUI>();
            SubtitleText = subtitleText;

            creationCallback?.Invoke(this);
        }
    }
}
