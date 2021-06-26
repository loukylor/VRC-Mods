using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.Core;
using VRChatUtilityKit.Utilities;

[assembly: MelonInfo(typeof(AskToPortal.AskToPortalMod), "AskToPortal", "3.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonIncompatibleAssemblies(new string[1] { "Portal Confirmation" })]

namespace AskToPortal
{
    class AskToPortalMod : MelonMod
    {
        internal static AskToPortalMod Instance { get; private set; }

        internal static bool shouldInterrupt = true;
        public static List<string> blacklistedUserIds = new List<string>();

        public static Dictionary<int, Player> cachedDroppers = new Dictionary<int, Player>();
        
        public override void OnApplicationStart()
        {
            Instance = this;

            AskToPortalSettings.RegisterSettings();
            if (MelonHandler.Mods.Any(mod => mod.Info.Name == "Portal Confirmation"))
            {
                MelonLogger.Warning("Use of Portal Confirmation by 404 was detected! AskToPortal is NOT Portal Confirmation. AskToPortal is simply a replacement for Portal Confirmation as 404 was BANNED from the VRChat Modding Group. If you wish to use this mod please DELETE Portal Confirmation.");
            }
            else
            {
                PortalUtils.Init();

                HarmonyInstance.Patch(PortalUtils.enterPortal, prefix: new HarmonyMethod(typeof(AskToPortalMod).GetMethod("OnPortalEnter", BindingFlags.Static | BindingFlags.Public)));
                HarmonyInstance.Patch(typeof(PortalInternal).GetMethod("ConfigurePortal"), prefix: new HarmonyMethod(typeof(AskToPortalMod).GetMethod("OnPortalDropped", BindingFlags.Static | BindingFlags.Public)));
                HarmonyInstance.Patch(typeof(PortalInternal).GetMethod("OnDestroy"), prefix: new HarmonyMethod(typeof(AskToPortalMod).GetMethod("OnPortalDestroyed", BindingFlags.Static | BindingFlags.Public)));

                VRCUtils.OnUiManagerInit += OnUiManagerInit;

                MelonLogger.Msg("Initialized!");
            }
        }
        private void OnUiManagerInit()
        {
            MenuManager.LoadAssetBundle();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            cachedDroppers.Clear();
        }

        public static void OnPortalDropped(MonoBehaviour __instance, Player __3)
        {
            cachedDroppers.Add(__instance.GetInstanceID(), __3);
        }

        public static void OnPortalDestroyed(MonoBehaviour __instance)
        {
            cachedDroppers.Remove(__instance.GetInstanceID());  
        }
        public static bool OnPortalEnter(PortalInternal __instance)
        {
            if (!AskToPortalSettings.enabled.Value)
                return true;
            if (!shouldInterrupt)
                return true;

            Photon.Pun.PhotonView photonView = __instance.gameObject.GetComponent<Photon.Pun.PhotonView>();
            APIUser dropper;
            if (photonView == null)
                dropper = new APIUser(displayName: "Not Player Dropped", id: "");
            else
                dropper = cachedDroppers[__instance.GetInstanceID()].prop_APIUser_0; // Get cached user because the photon object before gets the owner and can be spoofed

            if (blacklistedUserIds.Contains(dropper.id))
                return false;
            if (!ShouldCheckUserPortal(dropper, photonView == null))
                return true;

            string roomId = __instance.field_Private_String_1;
            string worldId = __instance.field_Private_ApiWorld_0.id;
            int roomPop = __instance.field_Private_Int32_0;

            RoomInfo roomInfo;
            if (photonView == null)
                roomInfo = new RoomInfo();
            else
                roomInfo = new RoomInfo(roomId);

            //If portal dropper is not owner of private instance but still dropped the portal or world id is the public ban world or if the population is in the negatives or is above 80
            if (!string.IsNullOrWhiteSpace(roomInfo.ownerId) && roomInfo.ownerId != dropper.id && !roomInfo.instanceType.Contains("Public")) 
                roomInfo.errors.Add("Instance type was non-public and the owner of the instance did not match the dropper of the portal");

            if (worldId == "wrld_5b89c79e-c340-4510-be1b-476e9fcdedcc") 
                roomInfo.errors.Add("Portal leads to the public ban world, which is used almost exclusively by portal droppers");

            if (roomPop < 0 || roomPop > 80)
                roomInfo.errors.Add("Room population was an invalid value");

            MenuManager.OnPortalEnter(roomInfo, __instance, dropper, worldId, roomId);
            return false;
        }

        public static bool ShouldCheckUserPortal(APIUser dropper, bool isWorldPortal) => !((APIUser.IsFriendsWith(dropper.id) && AskToPortalSettings.autoAcceptFriends.Value) || (dropper.IsSelf && AskToPortalSettings.autoAcceptSelf.Value) || (isWorldPortal && AskToPortalSettings.autoAcceptWorld.Value));
    }
}
