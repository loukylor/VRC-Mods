using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0044 // Add readonly modifier

namespace VRChatUtilityKit.Ui
{
    // Credit for Slaynash (https://gist.github.com/Slaynash/018d6de4e8d27faf08c0fe4a6c2854de) to act as reference
    public class TabButton : ElementBase
    {
        public TabMenu SubMenu { get; private set; }

        public MenuTab MenuTab { get; private set; }

        private Image _tabImage;
        public Sprite TabSprite 
        {
            get => _tabImage.sprite;
            set => _tabImage.sprite = value;
        }

        public TabButton(Sprite sprite, string pageName, string gameObjectName) : base(UiManager.QMStateController.transform.Find("Container/Window/Page_Buttons_QM/HorizontalLayoutGroup/Page_DevTools").gameObject, UiManager.QMStateController.menuTabsParent.gameObject, gameObjectName)
        {
            MenuTab = gameObject.GetComponent<MenuTab>();
            MenuTab.pageName = pageName;

            SubMenu = new TabMenu(pageName, $"Page_{pageName}");
            
            _tabImage = gameObject.transform.Find("Icon").GetComponent<Image>();
            _tabImage.sprite = sprite;
        }


    }
}
