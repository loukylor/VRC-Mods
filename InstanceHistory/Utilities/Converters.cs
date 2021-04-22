using UnityEngine;

namespace InstanceHistory.Utilities
{
    class Converters
    {
        public static Vector3 ConvertToUnityUnits(Vector3 menuPosition)
        {
            menuPosition.y *= -1;
            return new Vector3(-1050, 1470) + menuPosition * 420;
        }
        public static Vector3 ConvertToMenuUnits(Vector3 unityPosition)
        {
            Vector3 menuUnits = (unityPosition - new Vector3(-1050, 1470)) / 420;
            menuUnits.y *= -1;
            return menuUnits;
        }
    }
}
