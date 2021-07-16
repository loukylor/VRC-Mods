using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using VRC;

[assembly: MelonInfo(typeof(ChairExitController.ChairExitControllerMod), "ChairExitController", "1.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ChairExitController
{
    public class ChairExitControllerMod : MelonMod
    {
        public static VRC_StationInternal currentStation;

        private static bool shouldInterrupt = false;

        private static VRCInput useLeft;
        private static VRCInput useRight;

        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(typeof(VRC_StationInternal).GetMethod("UseStation"), new HarmonyMethod(typeof(ChairExitControllerMod).GetMethod(nameof(OnUseStation), BindingFlags.NonPublic | BindingFlags.Static)));
            HarmonyInstance.Patch(typeof(VRC_StationInternal).GetMethod("ExitStation"), new HarmonyMethod(typeof(ChairExitControllerMod).GetMethod(nameof(OnExitStation), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex != -1)
                return;

            useRight = VRCInputManager.Method_Public_Static_VRCInput_String_0("UseRight");
            useLeft = VRCInputManager.Method_Public_Static_VRCInput_String_0("UseLeft");
        }

        public override void OnUpdate()
        {
            if (currentStation == null)
                return;

            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E) || useLeft.field_Private_Boolean_0 && useRight.field_Private_Boolean_0)
            {
                shouldInterrupt = false;
                currentStation.ExitStation(Player.prop_Player_0);
                currentStation = null;
            }
        }

        private static void OnUseStation(VRC_StationInternal __instance, Player __0)
        {
            if (!__0.prop_APIUser_0.IsSelf)
                return;

            currentStation = __instance;
            shouldInterrupt = true;
        }

        private static bool OnExitStation(Player __0) => !(__0.prop_APIUser_0.IsSelf && shouldInterrupt);
    }
}
