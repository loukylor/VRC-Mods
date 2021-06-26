using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0044 // Add readonly modifier

namespace VRChatUtilityKit.Ui
{
    // Credit for Slaynash (https://gist.github.com/Slaynash/018d6de4e8d27faf08c0fe4a6c2854de) to act as reference
    public class TabButton : ElementBase
    {
        public readonly int index;

        public Button ButtonComponent { get; private set; }

        public SubMenu SubMenu { get; set; }

        private Image _tabImage;
        public Sprite TabSprite 
        {
            get => _tabImage.sprite;
            set => _tabImage.sprite = value;
        }

        internal Il2CppSystem.Object _tabDescriptor;

        public TabButton(Sprite sprite, SubMenu subMenu, Action onClick = null) : base(GameObject.Find("UserInterface/QuickMenu/QuickModeTabs"), GameObject.Find("UserInterface/QuickMenu/QuickModeTabs/HomeTab"), Vector3.zero, "PlayerListTab")
        {
            UiManager.ExistingTabs = UiManager.ExistingTabs.Concat(new List<GameObject>() { gameObject }).ToList(); // Inefficient code is my motto
            index = UiManager.ExistingTabs.Count;

            _tabImage = gameObject.transform.Find("Icon").GetComponent<Image>();
            _tabImage.sprite = sprite;

            ButtonComponent = gameObject.GetComponent<Button>();
            ButtonComponent.onClick = new Button.ButtonClickedEvent();
            if (onClick != null)
                ButtonComponent.onClick.AddListener(onClick);

            _tabDescriptor = gameObject.GetComponent(UiManager._tabDescriptorType);

            SubMenu = subMenu;
            UiManager.SetIndexOfTab(this, UiManager.ExistingTabs.Count);
        }

        public void OpenTabMenu(bool setCurrentMenu = true, bool setCurrentTab = true) => UiManager.OpenTabMenu(this, SubMenu.gameObject, setCurrentMenu, setCurrentTab);
    }
}
