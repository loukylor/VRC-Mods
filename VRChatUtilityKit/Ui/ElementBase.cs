using VRChatUtilityKit.Utilities;
using UnityEngine;
using System.Collections.Generic;

#pragma warning disable IDE1006 // Naming Styles

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper that holds a UI element.
    /// </summary>
    public class ElementBase
    {
        /// <summary>
        /// The path of the GameObject
        /// </summary>
        public string Path { get; private set; }
        /// <summary>
        /// The GameObject of the element.
        /// </summary>
        public GameObject gameObject { get; private set; }
        /// <summary>
        /// The RectTransform of the element.
        /// </summary>
        public RectTransform rectTransform { get; private set; }

        private readonly int _id;
        /// <summary>
        /// A unique ID of the element.
        /// Does not persist through updates.
        /// </summary>
        public int id => _id;

        /// <summary>
        /// Creates a new ElementBase.
        /// </summary>
        /// <param name="parent">The parent of the element</param>
        /// <param name="template">An existing element to create a copy of</param>
        /// <param name="gameObjectName">The name of the element's GameObject</param>
        protected ElementBase(GameObject parent, GameObject template, string gameObjectName)
        {
            gameObject = Object.Instantiate(template, parent.transform);
            _id = gameObject.GetInstanceID();
            rectTransform = gameObject.GetComponent<RectTransform>();
            Path = gameObject.GetPath();

            gameObject.name = gameObjectName;
        }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false</returns>
        public override bool Equals(object obj) => obj is ElementBase element && element.id == id;

        /// <summary>
        /// Returns a unique integer representing the element.
        /// </summary>
        /// <returns>A unique integer representing the element</returns>
        public override int GetHashCode() => id;
    }
}
