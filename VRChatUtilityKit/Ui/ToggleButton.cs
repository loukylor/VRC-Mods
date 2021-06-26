using System;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0044 // Add readonly modifier

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A class representing a toggle button in VRChat's UI.
    /// </summary>
    public class ToggleButton : ElementBase
    {
        /// <summary>
        /// The GameObject containing all the text.
        /// </summary>
        public GameObject TextContainer { get; private set; }
        /// <summary>
        /// The onClick event of toggle button.
        /// </summary>
        public Action<bool> OnClick { get; set; }

        private GameObject _onTextContainer;
        private Text _onStateOnText;
        private Text _onStateOffText;

        private GameObject _offTextContainer;
        private Text _offStateOnText;
        private Text _offStateOffText;


        /// <summary>
        /// Sets whether the text should resize for best fit.
        /// </summary>
        public bool Resize
        {
            get { return _resize; }
            set
            {
                _onStateOnText.resizeTextForBestFit = value;
                _offStateOnText.resizeTextForBestFit = value;
                _onStateOffText.resizeTextForBestFit = value;
                _offStateOffText.resizeTextForBestFit = value;
                _resize = value;
            }
        }
        private bool _resize = false;

        /// <summary>
        /// The state of the toggle.
        /// </summary>
        public bool State
        {
            get { return _state; }
            set
            {
                _state = value;
                SetState(value);
            }
        }
        private bool _state = true;

        public ToggleButton(string parent, Vector3 position, string onText, string offText, Action<bool> onClick, string offTooltip, string onTooltip, string name, bool defaultState = true, bool resize = false) : this(GameObject.Find(parent), position, onText, offText, onClick, offTooltip, onTooltip, name, defaultState, resize) { }
        public ToggleButton(GameObject parent, Vector3 position, string onText, string offText, Action<bool> onClick, string offTooltip, string onTooltip, string name, bool defaultState = true, bool resize = false) : base(parent, GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/ToggleHUDButton"), position, name)
        {
            OnClick = onClick;

            gameObject.GetComponent<Button>().onClick.AddListener(new Action(() => { State = !State; OnClick.Invoke(State); }));
            UnityEngine.Object.Destroy(gameObject.transform.GetChild(1).gameObject);
            gameObject.GetComponent<UiTooltip>().field_Public_String_1 = onTooltip;
            gameObject.GetComponent<UiTooltip>().field_Public_String_0 = offTooltip;

            TextContainer = gameObject.transform.GetChild(0).gameObject;
            TextContainer.name = "ToggleStates";

            _onTextContainer = TextContainer.transform.FindChild("ON").gameObject;
            GameObject onStateOnContainer = _onTextContainer.transform.FindChild("Text_HudOn").gameObject;
            onStateOnContainer.name = "TextOn";
            _onStateOnText = onStateOnContainer.GetComponent<Text>();
            _onStateOnText.text = onText;
            GameObject onStateOffContainer = _onTextContainer.transform.FindChild("Text_HudOff").gameObject;
            onStateOnContainer.name = "TextOff";
            _onStateOffText = onStateOffContainer.GetComponent<Text>();
            _onStateOffText.text = offText;

            _offTextContainer = TextContainer.transform.FindChild("OFF").gameObject;
            GameObject offStateOnContainer = _offTextContainer.transform.FindChild("Text_HudOn").gameObject;
            offStateOnContainer.name = "TextOn";
            _offStateOnText = offStateOnContainer.GetComponent<Text>();
            _offStateOnText.text = onText;
            GameObject offStateOffContainer = _offTextContainer.transform.FindChild("Text_HudOff").gameObject;
            offStateOffContainer.name = "TextOff";
            _offStateOffText = offStateOffContainer.GetComponent<Text>();
            _offStateOffText.text = offText;

            State = defaultState;
            Resize = resize;
        }
        private void SetState(bool state)
        {
            _onTextContainer.SetActive(state);
            _offTextContainer.SetActive(!state);
        }
    }
}
