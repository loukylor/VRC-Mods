using System;
using UnityEngine;
using VRC.DataModel.Core;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper holding a button.
    /// </summary>
    public class SingleButton : VRCButton, IButtonGroupElement
    {
        /// <summary>
        /// The type of button this interface represents.
        /// </summary>
        public ElementType Type => ElementType.SingleButton;

        /// <summary>
        /// The OnClick of the button.
        /// </summary>
        public Action OnClick { get; set; }

        /// <summary>
        /// A small icon indicating this buttons jumps to the big menu.
        /// </summary>
        public GameObject JumpBadge { get; private set; }

        /// <summary>
        /// Creates a new button.
        /// </summary>
        /// <param name="onClick">The OnClick of the button</param>
        /// <param name="icon">The icon for the button</param>
        /// <param name="text">The text of the button</param>
        /// <param name="gameObjectName">The name of the button's GameObject</param>
        /// <param name="tooltipText">The tooltip of the button</param>
        /// <param name="creationCallback">An action that is called with the button object when the button is created</param>
        public SingleButton(Action onClick, Sprite icon, string text, string gameObjectName, string tooltipText = "", Action<SingleButton> creationCallback = null) : base(UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Settings/Panel_QM_ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UI_Elements_Row_1/Button_NameplateSettings").gameObject, icon, text, gameObjectName, tooltipText)
        {
            JumpBadge = rectTransform.Find("Badge_MMJump").gameObject;
            JumpBadge.SetActive(false);
            OnClick = onClick;
            BindingExtensions.Method_Public_Static_ButtonBindingHelper_Button_Action_0(ButtonComponent, new Action(() => OnClick?.Invoke()));

            creationCallback?.Invoke(this);
        }
    }
}