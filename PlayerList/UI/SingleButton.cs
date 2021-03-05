using System;
using PlayerList.Utilities;
using UnityEngine;

namespace PlayerList.UI
{
    public class SingleButton
    {
        public GameObject gameObject;
        public UnityEngine.UI.Button buttonComponent;

        public GameObject TextContainer;
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
        public SingleButton(string parent, Vector3 position, string text, Action onClick, string tooltip, string buttonName)
        {
            gameObject = UnityEngine.Object.Instantiate(GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton"), GameObject.Find(parent).transform);
            gameObject.name = buttonName;
            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(onClick);
            gameObject.GetComponent<UiTooltip>().field_Public_String_0 = tooltip;

            Position = position;

            TextContainer = gameObject.transform.GetChild(0).gameObject;
            this.text = TextContainer.GetComponent<UnityEngine.UI.Text>();
            this.text.text = text;
        }
    }
}