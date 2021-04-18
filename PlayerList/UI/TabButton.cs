using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerList.UI
{
    // Credit for Slaynash (https://gist.github.com/Slaynash/018d6de4e8d27faf08c0fe4a6c2854de) to act as reference
    public class TabButton : TileBase
    {
        public Button buttonComponent;
        public readonly int index;
        public TabButton(Sprite image, Action onClick) : base(GameObject.Find("UserInterface/QuickMenu/QuickModeTabs"), GameObject.Find("UserInterface/QuickMenu/QuickModeTabs/HomeTab"), Vector3.zero, "PlayerListTab")
        {
            UIManager.ExistingTabs = UIManager.ExistingTabs.Concat(new List<GameObject>() { gameObject }).ToList(); // Inefficient code is my motto
            UIManager.SetTabTabIndex(this, UIManager.ExistingTabs.Count);
            index = UIManager.ExistingTabs.Count;

            gameObject.transform.Find("Icon").GetComponent<Image>().sprite = image;

            buttonComponent = gameObject.GetComponent<Button>();
            buttonComponent.onClick = new Button.ButtonClickedEvent();
            buttonComponent.onClick.AddListener(onClick);
        }
    }
}
