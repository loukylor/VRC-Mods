using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit.Ui
{
    public class Label : ElementWithText
    {
        public Label(string parent, Vector3 position, string text, string labelName, int fontSize = 72, bool resize = false, Color? textColor = null) : this(GameObject.Find(parent), position, text, labelName, fontSize, resize, textColor) { }
        public Label(GameObject parent, Vector3 position, string text, string labelName, int fontSize = 72, bool resize = false, Color? textColor = null) : base(parent, GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton"), position, text, labelName, resize, textColor)
        {
            Object.DestroyImmediate(gameObject.GetComponent<Button>());
            Object.DestroyImmediate(gameObject.GetComponent<UiTooltip>());
            Object.DestroyImmediate(gameObject.GetComponent<Image>());
            
            TextComponent = gameObject.AddComponent<Text>();
            TextComponent.font = gameObject.transform.GetChild(0).GetComponent<Text>().font;
            TextComponent.fontSize = fontSize;
            TextComponent.alignment = TextAnchor.MiddleCenter;
            TextComponent.text = text;

            Object.DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
        }
    }
}
