using PlayerList.Utilities;
using UnityEngine;

namespace PlayerList.UI
{
    public class Label
    {
        public GameObject gameObject;
        public UnityEngine.UI.Text text;

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
        public Label(string parent, Vector3 position, string text, string labelName)
        {
            gameObject = Object.Instantiate(GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton"), GameObject.Find(parent).transform);
            gameObject.name = labelName;
            Object.DestroyImmediate(gameObject.GetComponent<UnityEngine.UI.Button>());
            Object.DestroyImmediate(gameObject.GetComponent<UiTooltip>());
            Object.DestroyImmediate(gameObject.GetComponent<UnityEngine.UI.Image>());

            Position = position;
            
            this.text = gameObject.AddComponent<UnityEngine.UI.Text>();
            this.text.font = gameObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().font;
            this.text.fontSize = 72;
            this.text.alignment = TextAnchor.MiddleCenter;
            this.text.text = text;
            Object.DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
        }
    }
}
