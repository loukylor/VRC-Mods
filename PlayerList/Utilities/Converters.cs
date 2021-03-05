using System;
using UnityEngine;

namespace PlayerList.Utilities
{
    public class Converters
    {
        public static Vector3 ConvertToUnityUnits(Vector3 menuPosition)
        {
            menuPosition.y *= -1;
            return RoundAmount(new Vector3(-1050, 1470) + menuPosition * 420, 10);
        }
        public static Vector3 ConvertToMenuUnits(Vector3 unityPosition)
        {
            Vector3 menuUnits = (RoundAmount(unityPosition, 10) - new Vector3(-1050, 1470)) / 420;
            menuUnits.y *= -1;
            return menuUnits;
        }
        public static int RoundAmount(double i, int amount)
        {
            return ((int)Math.Round(i / amount)) * amount;
        }
        public static Vector3 RoundAmount(Vector3 i, int amount)
        {
            return new Vector3(RoundAmount(i.x, amount), RoundAmount(i.y, amount), RoundAmount(i.z, amount));
        }
    }
}
