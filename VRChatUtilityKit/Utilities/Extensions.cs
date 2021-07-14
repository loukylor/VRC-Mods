using MelonLoader;
using System;
using UnityEngine;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// A set of extensions for various things.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns the path of the given GameObject.
        /// </summary>
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

        /// <summary>
        /// Sets the layer of the given GameObject and its children to the one given
        /// </summary>
        /// <param name="layer">The layer to set the GameObject and children to</param>
        public static void SetLayerRecursive(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (var child in gameObject.transform)
                SetLayerRecursive(child.Cast<Transform>().gameObject, layer);
        }

        /// <summary>
        /// Returns a new Vector3 with its z position set to the given one.
        /// </summary>
        /// <param name="newZ">The new z position of the vector</param>
        public static Vector3 SetZ(this Vector3 vector, float newZ)
        {
            vector.Set(vector.x, vector.y, newZ);
            return vector;
        }

        /// <summary>
        /// Returns a copy of the float rounded to the given number.
        /// </summary>
        /// <param name="nearestFactor">The number the float should be rounded to</param>
        public static float RoundAmount(this float i, float nearestFactor)
        {
            return (float)Math.Round(i / nearestFactor) * nearestFactor;
        }

        /// <summary>
        /// Returns a copy of the vector rounded to the given number.
        /// </summary>
        /// <param name="nearestFactor">The number the vector should be rounded to</param>
        public static Vector3 RoundAmount(this Vector3 i, float nearestFactor)
        {
            return new Vector3(i.x.RoundAmount(nearestFactor), i.y.RoundAmount(nearestFactor), i.z.RoundAmount(nearestFactor));
        }

        /// <summary>
        /// Returns a copy of the vector rounded to the given number.
        /// </summary>
        /// <param name="nearestFactor">The number the vector should be rounded to</param>
        public static Vector2 RoundAmount(this Vector2 i, float nearestFactor)
        {
            return new Vector2(i.x.RoundAmount(nearestFactor), i.y.RoundAmount(nearestFactor));
        }

        /// <summary>
        /// Safely invokes the given delegate with the given args.
        /// </summary>
        /// <param name="delegate">The given delegate</param>
        /// <param name="args">The params of the delegate</param>
        public static void DelegateSafeInvoke(this Delegate @delegate, params object[] args)
        {
            if (@delegate == null)
                return;

            foreach (Delegate @delegates in @delegate.GetInvocationList())
            {
                try
                {
                    @delegates.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("Error while invoking delegate:\n" + ex.ToString());
                }
            }
        }
    }
}
