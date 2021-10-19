using System;
using UnityEngine;
using VRC.DataModel.Core;

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper holding a button.
    /// </summary>
    public class SingleButton : VRCSelectable
    {
        /// <summary>
        /// The OnClick of the button.
        /// </summary>
        public Action OnClick { get; set; }

        /// <summary>
        /// Creates a new button.
        /// </summary>
        /// <param name="parent">The parent of the button</param>
        /// <param name="onClick">The OnClick of the button</param>
        /// <param name="icon">The icon for the button</param>
        /// <param name="gameObjectName">The name of the button's GameObject</param>
        public SingleButton(GameObject parent, Action onClick, Sprite icon, string gameObjectName) : base(parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_QuickLinks/Button_Worlds").gameObject, icon, gameObjectName)
        {
            OnClick = onClick;
            BindingExtensions.Bind(ButtonComponent, new Action(() => OnClick?.Invoke()));
        }
    }
}