using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.DataModel.Core;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A class representing a toggle button in VRChat's UI.
    /// </summary>
    public class ToggleButton : VRCButton, IButtonGroupElement
    {
        /// <summary>
        /// The type of button this interface represents.
        /// </summary>
        public ElementType Type => ElementType.ToggleButton;

        /// <summary>
        /// The toggle on the component on the button.
        /// </summary>
        public Toggle ToggleComponent { get; private set; }

        /// <summary>
        /// The OnClick of the button.
        /// </summary>
        public Action<bool> OnClick { get; set; }

        private readonly Image _altImage;
        /// <summary>
        /// The off sprite of the button.
        /// </summary>
        public Sprite AltSprite 
        {
            get => _altImage.sprite;
            set => _altImage.sprite = value;
        }

        /// <summary>
        /// Creates a new toggle button.
        /// </summary>
        /// <param name="onClick">The OnClick of the button</param>
        /// <param name="icon">The icon for the button</param>
        /// <param name="altIcon">The off icon for the button</param>
        /// <param name="text">The text of the button</param>
        /// <param name="gameObjectName">The name of the button's GameObject</param>
        /// <param name="tooltipText">The tooltip shown when the toggle is on</param>
        /// <param name="tooltipAltText">The tooltip shown when the toggle is off</param>
        /// <param name="creationCallback">An action that is called with the button object when the button is created</param>
        public ToggleButton(Action<bool> onClick, Sprite icon, Sprite altIcon, string text, string gameObjectName, string tooltipText = "", string tooltipAltText = "", Action<ToggleButton> creationCallback = null) : base(UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Settings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UI_Elements_Row_1/Button_ToggleQMInfo").gameObject, icon, text, gameObjectName, tooltipText)
        {
            _altImage = rectTransform.Find("Icon_Off").GetComponent<Image>();
            if (altIcon != null)
                AltSprite = altIcon;

            ToggleComponent = gameObject.GetComponent<Toggle>();

            TooltipAltText = tooltipAltText;

            OnClick = onClick;
            BindingExtensions.Method_Public_Static_ToggleBindingHelper_Toggle_Action_1_Boolean_0(ToggleComponent, new Action<bool>((state) => OnClick?.Invoke(state)));

            creationCallback?.Invoke(this);
        }
    }
}
