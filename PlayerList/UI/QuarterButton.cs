using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayerList.UI
{
    public class QuarterButton : SingleButton
    {
        public QuarterButton(string parent, Vector3 position, Vector2 pivot, string text, Action onClick, string tooltip, string buttonName) : base(parent, position, text, onClick, tooltip, buttonName)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 210);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 210);
            rect.ForceUpdateRectTransforms();
            rect.pivot = pivot;
            
            RectTransform child = gameObject.transform.GetChild(0).GetComponent<RectTransform>();
            child.anchoredPosition /= 2;
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 210);
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 210);
            child.ForceUpdateRectTransforms();
        }
    }
}
