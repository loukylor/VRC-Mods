using System;
using UnityEngine;
using UnityEngine.Events;

namespace PlayerList.UI
{
    public class ToggleButton : TileBase
    {
        public GameObject TextContainer;
        public Action<bool> onClick;

        public GameObject onTextContainer;
        public GameObject onStateOnContainer;
        public UnityEngine.UI.Text onStateOnText;
        public GameObject onStateOffContainer;
        public UnityEngine.UI.Text onStateOffText;

        public GameObject offTextContainer;
        public GameObject offStateOnContainer;
        public UnityEngine.UI.Text offStateOnText;
        public GameObject offStateOffContainer;
        public UnityEngine.UI.Text offStateOffText;

        public Func<bool> defaultStateInvoke;
        protected Vector3 _position;

        private bool _resize = false;
        public bool Resize
        {
            get { return _resize; }
            set
            {
                onStateOnText.resizeTextForBestFit = value;
                offStateOnText.resizeTextForBestFit = value;
                onStateOffText.resizeTextForBestFit = value;
                offStateOffText.resizeTextForBestFit = value;
                _resize = value;
            }
        }

        private bool _state = true;
        public bool State
        {
            get { return _state; }
            set
            {
                _state = value;
                SetState(value);
            }
        }
        public ToggleButton(string parent, Vector3 position, string onText, string offText, Action<bool> onClick, string offTooltip, string onTooltip, string name, bool defaultState = true, bool resize = false) : base(GameObject.Find(parent), GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/ToggleHUDButton"), position, name)
        {
            this.onClick = onClick;

            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener((UnityAction)OnClick);
            UnityEngine.Object.Destroy(gameObject.transform.GetChild(1).gameObject);
            gameObject.GetComponent<UiTooltip>().field_Public_String_1 = onTooltip;
            gameObject.GetComponent<UiTooltip>().field_Public_String_0 = offTooltip;

            TextContainer = gameObject.transform.GetChild(0).gameObject;
            TextContainer.name = "ToggleStates";

            onTextContainer = TextContainer.transform.FindChild("ON").gameObject;
            onStateOnContainer = onTextContainer.transform.FindChild("Text_HudOn").gameObject;
            onStateOnContainer.name = "TextOn";
            onStateOnText = onStateOnContainer.GetComponent<UnityEngine.UI.Text>();
            onStateOnText.text = onText;
            onStateOffContainer = onTextContainer.transform.FindChild("Text_HudOff").gameObject;
            onStateOnContainer.name = "TextOff";
            onStateOffText = onStateOffContainer.GetComponent<UnityEngine.UI.Text>();
            onStateOffText.text = offText;

            offTextContainer = TextContainer.transform.FindChild("OFF").gameObject;
            offStateOnContainer = offTextContainer.transform.FindChild("Text_HudOn").gameObject;
            offStateOnContainer.name = "TextOn";
            offStateOnText = offStateOnContainer.GetComponent<UnityEngine.UI.Text>();
            offStateOnText.text = onText;
            offStateOffContainer = offTextContainer.transform.FindChild("Text_HudOff").gameObject;
            offStateOffContainer.name = "TextOff";
            offStateOffText = offStateOffContainer.GetComponent<UnityEngine.UI.Text>();
            offStateOffText.text = offText;

            State = defaultState;
            Resize = resize;
        }
        private void SetState(bool state)
        {
            onTextContainer.SetActive(state);
            offTextContainer.SetActive(!state);
        }
        private void OnClick()
        {
            State = !_state;
            onClick.Invoke(_state);
        }
    }
}
