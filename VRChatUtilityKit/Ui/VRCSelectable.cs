using UnityEngine;
using UnityEngine.UI;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A fairly low level wrapper representing something you can select.
    /// You should not be interacting directly with it.
    /// </summary>
    public class VRCSelectable : ElementBase
    {
        /// <summary>
        /// The button component on the selectable.
        /// </summary>
        public Button ButtonComponent { get; private set; }

        /// <summary>
        /// The tooltip on the selectable.
        /// </summary>
        public VRC.UI.Elements.Tooltips.UiTooltip Tooltip { get; private set; }
        /// <summary>
        /// Gets or sets the selectable's tooltip's text.
        /// </summary>
        public string TooltipText
        {
            get => Tooltip.text;
            set => Tooltip.text = value;
        }
        /// <summary>
        /// Gets or sets the selectable's tooltip's alternate text.
        /// </summary>
        public string TooltipAltText
        {
            get => Tooltip.alternateText;
            set => Tooltip.alternateText = value;
        }

        /// <summary>
        /// Creates a new VRCSelectable.
        /// It is not recommended to call this manually.
        /// </summary>
        /// <param name="parent">The parent of the selectable</param>
        /// <param name="template">An existing selectable to instantiate a copy of</param>
        /// <param name="gameObjectName">The name of the selectable's GameObject</param>
        public VRCSelectable(GameObject parent, GameObject template, string gameObjectName) : base(parent, template, gameObjectName)
        {
            ButtonComponent = gameObject.GetComponent<Button>();
            Tooltip = gameObject.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>();
        }
    }
}
