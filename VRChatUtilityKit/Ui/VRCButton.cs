using TMPro;
using UnityEngine;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper that holds a generic button used in VRChat's menus.
    /// </summary>
    public class VRCButton : VRCSelectable, IElementWithText
    {
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
        /// Creates a new VRCButton.
        /// </summary>
        /// <param name="template">The template of the button you want to make</param>
        /// <param name="icon">The icon of the button</param>
        /// <param name="text">The text on the button</param>
        /// <param name="gameObjectName">The name of the button's GameObject</param>
        /// <param name="tooltipText">The tooltip of the button</param>
        protected VRCButton(GameObject template, Sprite icon, string text, string gameObjectName, string tooltipText = "") : base(null, template, icon, gameObjectName, tooltipText)
        {
            TextComponent = rectTransform.Find("Text_H4").GetComponent<TextMeshProUGUI>();
            Text = text;
        }
    }
}
