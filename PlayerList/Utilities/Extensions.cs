using System;
using MelonLoader;
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
        public static float RoundAmount(this float i, float lowestDecimal)
        {
            return (float)Math.Round(i / lowestDecimal) * lowestDecimal;
        }
        public static Vector3 RoundAmount(this Vector3 i, float lowestDecimal)
        {
            return new Vector3(i.x.RoundAmount(lowestDecimal), i.y.RoundAmount(lowestDecimal), i.z.RoundAmount(lowestDecimal));
        }
        public static Vector2 RoundAmount(this Vector2 i, float lowestDecimal)
        {
            return new Vector2(i.x.RoundAmount(lowestDecimal), i.y.RoundAmount(lowestDecimal));
        }

        public static void SafeInvoke(this Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Error while invoking delegate:\n" + ex.ToString());
            }
        }
        public static void SafeInvoke<T>(this Action<T> action, T arg1)
        {
            try
            {
                action.Invoke(arg1);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Error while invoking delegate:\n" + ex.ToString());
            }
        }
        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            try
            {
                action.Invoke(arg1, arg2);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Error while invoking delegate:\n" + ex.ToString());
            }
        }
    }
}
