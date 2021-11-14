using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper holding a set of buttons' header.
    /// </summary>
    public class ButtonHeader : ElementBase, IElementWithText
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

        internal ButtonHeader(Transform parent, string text, string gameObjectName) : base(parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Header_QuickLinks").gameObject, gameObjectName)
        {
            TextComponent = rectTransform.Find("LeftItemContainer/Text_Title").GetComponent<TextMeshProUGUI>();
            gameObject.GetComponent<LayoutElement>().preferredHeight = 96;
            Text = text;
        }
    }
}
