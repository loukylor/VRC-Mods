using UnityEngine;

namespace PlayerList.UI
{
    public class SubMenu
    {
        public GameObject gameObject;
        public string path;
        public SubMenu(string parent, string pageName)
        {
            gameObject = Object.Instantiate(GameObject.Find("UserInterface/QuickMenu/ShortcutMenu"), GameObject.Find(parent).transform);
            Object.Destroy(gameObject.GetComponent<ShortcutMenu>());
            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
            }
            gameObject.name = pageName;

            path = parent + "/" + pageName;
        }
    }
}
