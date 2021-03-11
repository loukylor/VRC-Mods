using System;
using UnityEngine;

namespace PlayerList.UI
{
    public class SingleButton : TileWithText
    {
        public UnityEngine.UI.Button buttonComponent;
        public UiTooltip tooltipComponent;

        public SingleButton(GameObject parent, GameObject template, Vector3 position, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? color = null) : base(parent, template, position, text, buttonName, resize, color)
        {
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(onClick);
            tooltipComponent = gameObject.GetComponent<UiTooltip>();
            tooltipComponent.field_Public_String_0 = tooltip;
            tooltipComponent.field_Public_String_1 = tooltip;

            Position = position;
        }
        public SingleButton(string parent, string template, Vector3 position, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? color = null) : this(GameObject.Find(parent), GameObject.Find(template), position, text, onClick, tooltip, buttonName, resize, color) { }
        public SingleButton(string parent, Vector3 position, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? color = null) : this(parent, "UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton", position, text, onClick, tooltip, buttonName, resize, color) { } 
    }
}