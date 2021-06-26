using System;
using UnityEngine;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace VRChatUtilityKit.Ui
{
    public class QuarterButton : SingleButton
    {
        public QuarterButton(string parent, Vector3 position, Vector2 pivot, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? textColor = null) : this(GameObject.Find(parent), position, pivot, text, onClick, tooltip, buttonName, resize, textColor) { }
        public QuarterButton(GameObject parent, Vector3 position, Vector2 pivot, string text, Action onClick, string tooltip, string buttonName, bool resize = false, Color? textColor = null) : base(parent, position, text, onClick, tooltip, buttonName, resize, textColor)
        {
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 210);
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 210);
            Rect.ForceUpdateRectTransforms();
            Rect.pivot = pivot;
            
            RectTransform child = gameObject.transform.GetChild(0).GetComponent<RectTransform>();
            child.anchoredPosition /= 2;
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 210);
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 210);
            child.ForceUpdateRectTransforms();
        }
    }
}
