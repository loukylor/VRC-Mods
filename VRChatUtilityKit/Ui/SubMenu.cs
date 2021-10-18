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
        public VerticalLayoutGroup pageLayoutGroup { get; private set; }

        /// <summary>
        /// The back button of the sub menu.
        /// </summary>
        public MenuBackButton backButton { get; private set; }

        /// <summary>
        /// The Text component of the title of the sub menu.
        /// </summary>
        public TextMeshProUGUI titleText { get; private set; }
        /// <summary>
        /// Gets or sets the title of the sub menu.
        /// </summary>
        public string text
        {
            get => titleText.text;
            set => titleText.text = value;
        }

        /// <summary>
        /// Creates a new sub menu.
        /// </summary>
        /// <param name="parent">The parent of the sub menu</param>
        /// <param name="subMenuBase">An existing sub menu to instantiate a copy of</param>
        /// <param name="pageName">The name of the sub menu's page</param>
        /// <param name="gameObjectName">The name of the sub menu's GameObject</param>
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
        /// <summary>
        /// Creates a new sub menu.
        /// </summary>
        /// <param name="parent">The parent of the sub menu</param>
        /// <param name="pageName">The name of the sub menu's page</param>
        /// <param name="gameObjectName">The name of the sub menu's GameObject</param>
        public SubMenu(GameObject parent, string pageName, string gameObjectName) : this(parent, UiManager.QMStateController.transform.Find("Container/Window/QMParent/Menu_Dashboard").gameObject, pageName, gameObjectName) { }
        /// <summary>
        /// Creates a new sub menu.
        /// </summary>
        /// <param name="pageName">The name of the sub menu's page</param>
        /// <param name="gameObjectName">The name of the sub menu's GameObject</param>
        public SubMenu(string pageName, string gameObjectName) : this(UiManager.QMStateController.transform.Find("Container/Window/QMParent").gameObject, pageName, gameObjectName) { }
    }
}
