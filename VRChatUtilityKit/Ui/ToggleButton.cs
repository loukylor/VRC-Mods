using System;
using UnityEngine;
using UnityEngine.UI;
using VRC.DataModel.Core;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A class representing a toggle button in VRChat's UI.
    /// </summary>
    public class ToggleButton : VRCButton
    {
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
        public ToggleButton(Action<bool> onClick, Sprite icon, Sprite altIcon, string text, string gameObjectName) : base(UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Worlds").gameObject, icon, text, gameObjectName)
        {
            _altImage = rectTransform.Find("Icon_Off").GetComponent<Image>();
            AltSprite = altIcon;

            OnClick = onClick;
            BindingExtensions.Method_Public_Static_ToggleBindingHelper_Toggle_Action_1_Boolean_0(ToggleComponent, new Action<bool>((state) => OnClick?.Invoke(state)));
        }
    }
}
