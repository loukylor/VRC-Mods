using TMPro;
using UnityEngine;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// An interface that represents elements that go in ButtonGroups
    /// </summary>
    public interface IButtonGroupElement
    {
        /// <summary>
        /// The path of the GameObject
        /// </summary>
        string Path { get; }
        /// <summary>
        /// The GameObject of the element.
        /// </summary>
        GameObject gameObject { get; }
        /// <summary>
        /// The RectTransform of the element.
        /// </summary>
        RectTransform rectTransform { get; }

        /// <summary>
        /// A unique ID of the element.
        /// Does not persist through restarts.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The type of button this interface represents.
        /// </summary>
        ElementType Type { get; }

        /// <summary>
        /// The text mesh pro component on the element.
        /// </summary>
        TextMeshProUGUI TextComponent { get; }
        /// <summary>
        /// Gets or sets the text on the element.
        /// </summary>
        string Text { get; set; }
    }

    public enum ElementType
    {
        SingleButton,
        ToggleButton,
        Label
    }
}
