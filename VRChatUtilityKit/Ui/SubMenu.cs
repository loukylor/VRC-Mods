using VRChatUtilityKit.Utilities;
using UnityEngine;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles

namespace VRChatUtilityKit.Ui
{
    public class SubMenu
    {
        public GameObject gameObject { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public string Path { get; private set; }

        public SubMenu(GameObject subMenuBase)
        {
            gameObject = subMenuBase;

            RectTransform = gameObject.GetComponent<RectTransform>();

            Path = gameObject.GetPath();
        }
        public SubMenu(GameObject subMenuBase, GameObject parent, string pageName)
        {
            gameObject = Object.Instantiate(subMenuBase, parent.transform);
            Object.Destroy(gameObject.GetComponent<ShortcutMenu>());
            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
            }
            gameObject.name = pageName;

            RectTransform = gameObject.GetComponent<RectTransform>();

            Path = gameObject.GetPath();
        }
        public SubMenu(GameObject parent, string pageName) : this(GameObject.Find("UserInterface/QuickMenu/ShortcutMenu"), parent, pageName) { }
        public SubMenu(string parent, string pageName) : this(GameObject.Find(parent), pageName) { }

        public void OpenSubMenu(bool setCurrentMenu = true, bool setCurrentTab = true) => UiManager.OpenSubMenu(this, setCurrentMenu, setCurrentTab);
    }
}
