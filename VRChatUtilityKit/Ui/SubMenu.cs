using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;
using VRC.UI.Elements.Controls;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles

namespace VRChatUtilityKit.Ui
{
    public class SubMenu : ElementBase
    {
        public UIPage uiPage { get; private set; }

        public VerticalLayoutGroup pageLayoutGroup { get; private set; }

        public MenuBackButton backButton { get; private set; }

        public TextMeshProUGUI titleText { get; private set; }
        public string text
        {
            get => titleText.text;
            set => titleText.text = value;
        }

        public SubMenu(GameObject parent, GameObject subMenuBase, string pageName, string gameObjectName) : base(parent, subMenuBase, gameObjectName)
        {
            GameObject.DestroyImmediate(gameObject.GetComponent<UIPage>());
            GameObject.Destroy(rectTransform.Find("Header_H1/RightItemContainer/Button_QM_Expand").gameObject); // Dunno how the binding class works so

            pageLayoutGroup = rectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup").GetComponent<VerticalLayoutGroup>();
            for (int i = pageLayoutGroup.rectTransform.childCount - 1; i >= 0; i--)
                GameObject.DestroyImmediate(pageLayoutGroup.transform.GetChild(i).gameObject);

            backButton = rectTransform.Find("Header_H1/LeftItemContainer/Button_Back").GetComponent<MenuBackButton>();
            titleText = rectTransform.Find("Header_H1/LeftItemContainer/Text_Title").GetComponent<TextMeshProUGUI>();
            uiPage = gameObject.AddComponent<UIPage>();
            uiPage._menuStateController = UiManager.QMStateController;
            uiPage.Name = pageName;

            UiManager.QMStateController._uiPages.Add(pageName, uiPage);
        }
        public SubMenu(GameObject parent, string pageName, string gameObjectName) : this(parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard").gameObject, pageName, gameObjectName) { }
        public SubMenu(string pageName, string gameObjectName) : this(UiManager.QMStateController.transform.Find("Container/Window/QMParent").gameObject, pageName, gameObjectName) { }
    }
}
