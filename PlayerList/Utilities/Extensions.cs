using UnityEngine;

namespace PlayerList.Utilities
{
    static class Extensions
    {
        public static string GetPath(this GameObject gameObject)
        {
            string path = "/" + gameObject.name;
            while (gameObject.transform.parent != null)
            {
                gameObject = gameObject.transform.parent.gameObject;
                path = "/" + gameObject.name + path;
            }
            return path;
        }
        public static void SetLayerRecursive(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (var child in gameObject.transform)
                SetLayerRecursive(child.Cast<Transform>().gameObject, layer);
        }
        public static void SetZ(this Vector3 vector, float newZ)
        {
            vector.Set(vector.x, vector.y, newZ);
        }
    }
}
