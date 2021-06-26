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
        public RectTransform Rect { get; private set; }

        private Vector3 _position;
        public Vector3 Position
        {
            get => Converters.ConvertToEmmUnits(_position); 
            set
            {
                gameObject.transform.localPosition = Converters.ConvertToUnityUnits(value);
                _position = value;
            }
        }

        public ElementBase(GameObject parent, GameObject template, Vector3 position, string name)
        {
            gameObject = Object.Instantiate(template, parent.transform);
            Rect = gameObject.GetComponent<RectTransform>();
            Path = gameObject.GetPath();

            Position = position;

            gameObject.name = name;
        }

        public void Destroy()
        {
            Object.Destroy(gameObject);
        }
    }
}
