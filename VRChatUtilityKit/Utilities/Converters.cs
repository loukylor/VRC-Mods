using UnityEngine;

namespace VRChatUtilityKit.Utilities
{
    /// <summary>
    /// A set of converters for positions.
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// Returns the given coordinate converted from emm units to Unity units.
        /// </summary>
        /// <param name="emmPosition">The position to conver to Unity units</param>
        public static Vector3 ConvertToUnityUnits(Vector3 emmPosition)
        {
            emmPosition.y *= -1;
            return new Vector3(-1050, 1470) + emmPosition * 420;
        }

        /// <summary>
        /// Returns the given coordinate converted from Unity units to emm units.
        /// </summary>
        /// <param name="unityPosition">The position to conver to emm units</param>
        public static Vector3 ConvertToEmmUnits(Vector3 unityPosition)
        {
            Vector3 emmPosition = (unityPosition - new Vector3(-1050, 1470)) / 420;
            emmPosition.y *= -1;
            return emmPosition;
        }
    }
}
