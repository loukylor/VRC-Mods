using UnityEngine;
using UnityEngine.UI;

namespace InstanceHistory.UI
{
    public class Label : TileWithText
    {
        public Label(GameObject parent, Vector3 position, string text, string labelName, int fontSize = 72, bool resize = false, Color? color = null) : base(parent, GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton"), position, text, labelName, resize, color)
        {
            Object.DestroyImmediate(gameObject.GetComponent<Button>());
            Object.DestroyImmediate(gameObject.GetComponent<UiTooltip>());
            Object.DestroyImmediate(gameObject.GetComponent<Image>());
            
            textComponent = gameObject.AddComponent<Text>();
            textComponent.font = gameObject.transform.GetChild(0).GetComponent<Text>().font;
            textComponent.fontSize = fontSize;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.text = text;

            Object.DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
        }
    }
}
