using VRChatUtilityKit.Utilities;
using UnityEngine;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles

namespace VRChatUtilityKit.Ui
{
    public class ElementBase
    {
        /// <summary>
        /// The path of the GameObject
        /// </summary>
        public string Path { get; private set; }
        public GameObject gameObject { get; private set; }
        public RectTransform rectTransform { get; private set; }

        public ElementBase(GameObject parent, GameObject template, string name)
        {
            gameObject = Object.Instantiate(template, parent.transform);
            rectTransform = gameObject.GetComponent<RectTransform>();
            Path = gameObject.GetPath();

            gameObject.name = name;
        }

        public void Destroy()
        {
            Object.Destroy(gameObject);
        }
    }
}
