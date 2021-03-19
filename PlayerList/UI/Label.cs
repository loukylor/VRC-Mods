using UnityEngine;

namespace PlayerList.UI
{
    public class Label : TileWithText
    {
        public Label(string parent, Vector3 position, string text, string labelName, int fontSize = 72, bool resize = false, Color? color = null) : base(GameObject.Find(parent), GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton"), position, text, labelName, resize, color)
        {
            Object.DestroyImmediate(gameObject.GetComponent<UnityEngine.UI.Button>());
            Object.DestroyImmediate(gameObject.GetComponent<UiTooltip>());
            Object.DestroyImmediate(gameObject.GetComponent<UnityEngine.UI.Image>());
            
            textComponent = gameObject.AddComponent<UnityEngine.UI.Text>();
            textComponent.font = gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().font;
            textComponent.fontSize = fontSize;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.text = text;

            Object.DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
        }
    }
}
