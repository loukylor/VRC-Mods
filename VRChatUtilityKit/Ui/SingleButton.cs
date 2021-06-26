using System;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit.Ui
{
    public class SingleButton : ElementWithText
    {
        public Button ButtonComponent { get; private set; }
        public UiTooltip TooltipComponent { get; private set; }

        public SingleButton(GameObject parent, GameObject template, Vector3 position, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? textColor = null) : base(parent, template, position, text, buttonName, resize, textColor)
        {
            ButtonComponent = gameObject.GetComponent<Button>();
            ButtonComponent.onClick.AddListener(onClick);
            TooltipComponent = gameObject.GetComponent<UiTooltip>();
            TooltipComponent.field_Public_String_0 = tooltip;
            TooltipComponent.field_Public_String_1 = tooltip;

            Position = position;
        }
        public SingleButton(GameObject parent, Vector3 position, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? textColor = null) : this(parent, GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton"), position, text, onClick, tooltip, buttonName, resize, textColor) { }
        public SingleButton(string parent, string template, Vector3 position, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? textColor = null) : this(GameObject.Find(parent), GameObject.Find(template), position, text, onClick, tooltip, buttonName, resize, textColor) { }
        public SingleButton(string parent, Vector3 position, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? textColor = null) : this(parent, "UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton", position, text, onClick, tooltip, buttonName, resize, textColor) { }
    }
}