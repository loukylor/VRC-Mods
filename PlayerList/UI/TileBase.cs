using PlayerList.Utilities;
using UnityEngine;

namespace PlayerList.UI
{
    public class TileBase
    {
        public string path;
        public GameObject gameObject;
        public RectTransform rect;

        private Vector3 _position;
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                gameObject.transform.localPosition = Converters.ConvertToUnityUnits(value);
                _position = value;
            }
        }

        public TileBase(GameObject parent, GameObject template, Vector3 position, string name)
        {
            gameObject = Object.Instantiate(template, parent.transform);
            rect = gameObject.GetComponent<RectTransform>();
            path = gameObject.GetPath();

            Position = position;

            gameObject.name = name;
        }
    }
}
