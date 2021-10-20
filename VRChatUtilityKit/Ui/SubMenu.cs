using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Elements;
using VRC.UI.Elements.Controls;

#pragma warning disable IDE1006 // Naming Styles

namespace VRChatUtilityKit.Ui
{
    /// <summary>
    /// A wrapper holding a sub menu.
    /// </summary>
    public class SubMenu : ElementBase
    {
        /// <summary>
        /// The UIPage attached to the sub menu.
        /// </summary>
        public UIPage uiPage { get; private set; }

        /// <summary>
        /// The VerticalLayoutGroup that holds all the buttons and elements of the sub menu.
        /// </summary>
        public VerticalLayoutGroup PageLayoutGroup { get; private set; }

        /// <summary>
        /// The back button of the sub menu.
        /// </summary>
        public MenuBackButton BackButton { get; private set; }

        /// <summary>
        /// The Text component of the title of the sub menu.
        /// </summary>
        public TextMeshProUGUI TitleText { get; private set; }
        /// <summary>
        /// Gets or sets the title of the sub menu.
        /// </summary>
        public string Text
        {
            get => TitleText.text;
            set => TitleText.text = value;
        }

        /// <summary>
        /// Creates a new sub menu.
        /// </summary>
        /// <param name="parent">The parent of the sub menu</param>
        /// <param name="pageName">The name of the sub menu's page</param>
        /// <param name="gameObjectName">The name of the sub menu's GameObject</param>
        public SubMenu(Transform parent, string pageName, string gameObjectName) : base(parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard").gameObject, gameObjectName)
        {
            GameObject.DestroyImmediate(gameObject.GetComponent<UIPage>());
            GameObject.Destroy(rectTransform.Find("Header_H1/RightItemContainer/Button_QM_Expand").gameObject); // Dunno how the binding class works so

            PageLayoutGroup = rectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup").GetComponent<VerticalLayoutGroup>();
            for (int i = PageLayoutGroup.rectTransform.childCount - 1; i >= 0; i--)
                GameObject.DestroyImmediate(PageLayoutGroup.transform.GetChild(i).gameObject);

            BackButton = rectTransform.Find("Header_H1/LeftItemContainer/Button_Back").GetComponent<MenuBackButton>();
            TitleText = rectTransform.Find("Header_H1/LeftItemContainer/Text_Title").GetComponent<TextMeshProUGUI>();
            uiPage = gameObject.AddComponent<UIPage>();
            uiPage._menuStateController = UiManager.QMStateController;
            uiPage.Name = pageName;

            UiManager.QMStateController._uiPages.Add(pageName, uiPage);
        }
        /// <summary>
        /// Creates a new sub menu.
        /// </summary>
        /// <param name="pageName">The name of the sub menu's page</param>
        /// <param name="gameObjectName">The name of the sub menu's GameObject</param>
        public SubMenu(string pageName, string gameObjectName) : this(UiManager.QMStateController.transform.Find("Container/Window/QMParent"), pageName, gameObjectName) { }
    }
}
