using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit.Ui
{
    public class ElementWithText : ElementBase
    {
        public Text TextComponent { get; protected set; }
        
        private bool _resize;
        public bool Resize
        {
            get { return _resize; }
            set { _resize = value; TextComponent.resizeTextForBestFit = value; }
        }

        public ElementWithText(GameObject parent, GameObject template, Vector3 position, string text, string name, bool resize = false, Color? textColor = null) : base(parent, template, position, name)
        {
            TextComponent = gameObject.transform.GetComponentInChildren<Text>();

            if (TextComponent != null)
            {
                TextComponent.text = text;
                if (textColor != null)
                    TextComponent.color = (Color)textColor;
                Resize = resize;
            }
        }
    }
}
