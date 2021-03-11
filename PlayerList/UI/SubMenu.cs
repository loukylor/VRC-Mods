using PlayerList.Utilities;
using UnityEngine;

namespace PlayerList.UI
{
    public class SubMenu
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public string path;
        public SubMenu(GameObject parent, string pageName)
        {
            gameObject = Object.Instantiate(GameObject.Find("UserInterface/QuickMenu/ShortcutMenu"), parent.transform);
            Object.Destroy(gameObject.GetComponent<ShortcutMenu>());
            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
            }
            gameObject.name = pageName;

            rectTransform = gameObject.GetComponent<RectTransform>();

            path = parent.GetPath() + "/" + pageName;
        }
        public SubMenu(string parent, string pageName) : this(GameObject.Find(parent), pageName) { }
    }
}
