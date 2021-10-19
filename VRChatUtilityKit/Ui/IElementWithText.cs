using TMPro;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// An interface for wrappers that hold elements with text.
    /// </summary>
    public interface IElementWithText
    {
        /// <summary>
        /// The text mesh pro component on the element.
        /// </summary>
        TextMeshProUGUI TextComponent { get; }
        /// <summary>
        /// Gets or sets the text on the element.
        /// </summary>
        string Text { get; set; }
    }
}
