using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AskToPortal.Utilities;
using Harmony;
using MelonLoader;
using UnityEngine;
using VRC;
using VRC.Core;

[assembly: MelonInfo(typeof(AskToPortal.AskToPortalMod), "AskToPortal", "2.1.4", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonIncompatibleAssemblies(new string[1] { "Portal Confirmation" })]

namespace AskToPortal
{
    class AskToPortalMod : MelonMod
    {
        public static bool hasTriggered = false;
        public static List<string> blacklistedUserIds = new List<string>();

        public static Type portalInfo; //Dunno if the object name is static so lets just xref 
        public static Type portalInfoEnum;

        public static Dictionary<int, Player> cachedDroppers = new Dictionary<int, Player>();

        public static MethodInfo enterPortal;
        public static MethodInfo enterWorld;
        
        public override void OnApplicationStart()
        {
            AskToPortalSettings.RegisterSettings();
            if (MelonHandler.Mods.Any(mod => mod.Info.Name == "Portal Confirmation"))
            {
                MelonLogger.Warning("Use of Portal Confirmation by 404 was detected! AskToPortal is NOT Portal Confirmation. AskToPortal is simply a replacement for Portal Confirmation as 404 was BANNED from the VRChat Modding Group. If you wish to use this mod please DELETE Portal Confirmation.");
            }
            else
            {
                portalInfo = typeof(VRCFlowManager).GetMethods()
                    .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_WorldTransitionInfo_")).First().GetParameters()[1].ParameterType;
                portalInfoEnum = portalInfo.GetNestedTypes().First();            
                enterWorld = typeof(VRCFlowManager).GetMethods()
                    .Where(mb => mb.Name.StartsWith($"Method_Public_Void_String_String_{portalInfo.Name}_Action_1_String_Boolean_") && !mb.Name.Contains("PDM") && Xref.CheckMethod(mb, "EnterWorld called with an invalid world id.")).First();
                enterPortal = typeof(PortalInternal).GetMethods()
                    .Where(mb => mb.Name.StartsWith("Method_Public_Void_") && mb.Name.Length <= 21 && Xref.CheckUsed(mb, "OnTriggerEnter")).First();

                Harmony.Patch(enterPortal, prefix: new HarmonyMethod(typeof(AskToPortalMod).GetMethod("OnPortalEnter", BindingFlags.Static | BindingFlags.Public)));
                Harmony.Patch(typeof(PortalInternal).GetMethod("ConfigurePortal"), prefix: new HarmonyMethod(typeof(AskToPortalMod).GetMethod("OnPortalDropped", BindingFlags.Static | BindingFlags.Public)));
                Harmony.Patch(typeof(PortalInternal).GetMethod("OnDestroy"), prefix: new HarmonyMethod(typeof(AskToPortalMod).GetMethod("OnPortalDestroyed", BindingFlags.Static | BindingFlags.Public)));

                VRCUtils.Init();

                MelonLogger.Msg("Initialized!");
            }
        }
        public override void VRChat_OnUiManagerInit()
        {
            //MenuManager.LoadAssetBundle();
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
            if (!AskToPortalSettings.enabled.Value) return true;
            if (!hasTriggered)
            {
                Photon.Pun.PhotonView photonView = __instance.gameObject.GetComponent<Photon.Pun.PhotonView>();
                APIUser dropper;
                if (photonView == null)
                {
                    dropper = new APIUser(displayName: "Not Player Dropped", id: "");
                }
                else
                {
                    dropper = cachedDroppers[__instance.GetInstanceID()].prop_APIUser_0; // Get cached user because the photon object before gets the owner and can be spoofed
                }

                if (blacklistedUserIds.Contains(dropper.id)) return false;

                string roomId = __instance.field_Private_String_1;
                string worldId = __instance.field_Private_ApiWorld_0.id;
                int roomPop = __instance.field_Private_Int32_0;
                RoomInfo roomInfo = new RoomInfo();
                if (roomId == null)
                {
                    if (dropper.id != "") roomInfo.isPortalDropper = true; //If there is a dropper but the portal has no room id
                    roomInfo.instanceType = "Unknown";
                }
                else
                {
                    roomInfo = ParseRoomId(roomId);
                }
                
                //If portal dropper is not owner of private instance but still dropped the portal or world id is the public ban world or if the population is in the negatives or is above 80
                if ((roomInfo.ownerId != "" && roomInfo.ownerId != dropper.id && !roomInfo.instanceType.Contains("Friend")) || worldId == "wrld_5b89c79e-c340-4510-be1b-476e9fcdedcc" || roomPop < 0 || roomPop > 80) roomInfo.isPortalDropper = true;

                //MenuManager.OnPortalEnter(roomInfo, __instance, dropper, worldId, roomId);
                //return false;
                
                if (roomInfo.isPortalDropper && !(AskToPortalSettings.autoAcceptSelf.Value && dropper.IsSelf))
                {
                    VRCUtils.OpenPopup("Portal Dropper Detected!!!",
                        $"This portal was likely dropped by someone malicious! Only go into this portal if you trust {dropper.displayName}. Pressing \"Leave and Blacklist\" will blacklist {dropper.displayName}'s portals until the game restarts",
                        "Enter", new Action(() =>
                        {
                            VRCUtils.ClosePopup();

                            if (__instance == null)
                            {
                                object currentPortalInfo = Activator.CreateInstance(portalInfo);
                                currentPortalInfo.GetType().GetProperty($"field_Public_{portalInfoEnum.Name}_0").SetValue(currentPortalInfo, portalInfoEnum.GetEnumValues().GetValue(3)); //I hate reflection
                                typeof(Il2CppSystem.Collections.Generic.Dictionary<string, string>).GetMethod("Add").Invoke(currentPortalInfo.GetType().GetProperty("field_Public_Dictionary_2_String_String_0").GetValue(currentPortalInfo), new object[2] { "transitionPortalType", dropper.id == "" ? "Static" : "Dynamic" });
                                typeof(Il2CppSystem.Collections.Generic.Dictionary<string, string>).GetMethod("Add").Invoke(currentPortalInfo.GetType().GetProperty("field_Public_Dictionary_2_String_String_0").GetValue(currentPortalInfo), new object[2] { "transitionPortalOwner", dropper.id });
                                enterWorld.Invoke(VRCFlowManager.prop_VRCFlowManager_0, new object[5] { worldId, roomId, currentPortalInfo, (Il2CppSystem.Action<string>)new Action<string>((str) => VRCUtils.OpenSmallPopup("Alert", "Cannot Join World", "Close", new Action(VRCUtils.ClosePopup))), false }); //Just kill me
                            }
                            else
                            {
                                hasTriggered = true;
                                try
                                {
                                    enterPortal.Invoke(__instance, null);
                                }
                                catch {}
                            }
                        }), "Leave and Blacklist", new Action(() => { VRCUtils.ClosePopup(); blacklistedUserIds.Add(dropper.id); }), null);
                    return false;
                }
                else if (ShouldCheckUserPortal(dropper) && !(AskToPortalSettings.autoAcceptHome.Value && __instance.field_Internal_Boolean_1) && !AskToPortalSettings.onlyPortalDrop.Value)
                {
                    VRCUtils.OpenPopup("Enter This Portal?",
                        $"Do you want to enter this portal?{Environment.NewLine}World Name: {__instance.field_Private_ApiWorld_0.name}{Environment.NewLine}Dropper: {dropper.displayName}{Environment.NewLine}Instance Type: {roomInfo.instanceType}",
                        "Yes", new Action(() =>
                        {
                            VRCUtils.ClosePopup();

                            if (__instance == null)
                            {
                                object currentPortalInfo = Activator.CreateInstance(portalInfo);
                                currentPortalInfo.GetType().GetProperty($"field_Public_{portalInfoEnum.Name}_0").SetValue(currentPortalInfo, portalInfoEnum.GetEnumValues().GetValue(3)); //I hate reflection
                                typeof(Il2CppSystem.Collections.Generic.Dictionary<string, string>).GetMethod("Add").Invoke(currentPortalInfo.GetType().GetProperty("field_Public_Dictionary_2_String_String_0").GetValue(currentPortalInfo), new object[2] { "transitionPortalType", dropper.id == "" ? "Static" : "Dynamic" });
                                typeof(Il2CppSystem.Collections.Generic.Dictionary<string, string>).GetMethod("Add").Invoke(currentPortalInfo.GetType().GetProperty("field_Public_Dictionary_2_String_String_0").GetValue(currentPortalInfo), new object[2] { "transitionPortalOwner", dropper.id });
                                enterWorld.Invoke(VRCFlowManager.prop_VRCFlowManager_0, new object[5] { worldId, roomId, currentPortalInfo, new Action<string>((str) => VRCUtils.OpenSmallPopup("Alert", "Cannot Join World", "Close", new Action(VRCUtils.ClosePopup))), false }); //Just kill me
                            }
                            else
                            {
                                hasTriggered = true;
                                try
                                {
                                    enterPortal.Invoke(__instance, null);
                                }
                                catch {}
                            }

                        }), "No", new Action(VRCUtils.ClosePopup), null);

                    return false;
                }
            }
            hasTriggered = false;
            return true;
        }
        public static RoomInfo ParseRoomId(string roomId)
        {
            //Example invite room id: instanceId~private(someones user id here)~nonce(Long hex code here)
            //Example invite+ room id: instanceId~private(someones user id here)~canRequestInvite~nonce(Long hex code here)
            //Example friends room id: instanceId~friend(someones user id here)~nonce(Long hex code here)
            //Example friends+ room id: instanceId~hidden(someones user id here)~nonce(Long hex code here)
            //Example public room id: instanceId
            RoomInfo roomInfo = new RoomInfo();
            
            IEnumerator splitString = roomId.Split(new char[1] { '~' }).GetEnumerator();
            splitString.MoveNext();
            roomInfo.instanceId = (string) splitString.Current;
            try
            {
                int instanceId = int.Parse(roomInfo.instanceId);
                if (instanceId > 99998 || instanceId < 1) throw new Exception();
            }
            catch
            {
                roomInfo.isPortalDropper = true;
                return roomInfo;
            }
            if (splitString.MoveNext())
            {
                string[] tempString = ((string)splitString.Current).Split(new char[1] { '(' });
                
                switch (tempString[0])
                {
                    case "private":
                        roomInfo.instanceType = "Invite Only";
                        break;
                    case "friend":
                        roomInfo.instanceType = "Friends Only";
                        break;
                    case "hidden":
                        roomInfo.instanceType = "Friends+";
                        break;
                    default:
                        roomInfo.isPortalDropper = true;
                        return roomInfo;
                }
                try
                {
                    roomInfo.ownerId = tempString[1].TrimEnd(new char[1] { ')' });
                }
                catch (IndexOutOfRangeException) 
                {
                    roomInfo.isPortalDropper = true;
                    return roomInfo;
                }

                if (!splitString.MoveNext())
                {
                    roomInfo.isPortalDropper = true;
                    return roomInfo;
                }
                if ((string) splitString.Current == "canRequestInvite")
                {
                    roomInfo.instanceType = "Invite+";
                    splitString.MoveNext();
                }

                try
                {
                    roomInfo.nonce = ((string)splitString.Current).Split(new char[1] { '(' })[1].TrimEnd(new char[1] { ')' });
                }
                catch
                {
                    roomInfo.isPortalDropper = true;
                    return roomInfo;
                }
            }
            else
            {
                roomInfo.instanceType = "Public";
            }

            return roomInfo;
        }
        public static bool ShouldCheckUserPortal(APIUser dropper)
        {
            if ((APIUser.IsFriendsWith(dropper.id) && AskToPortalSettings.autoAcceptFriends.Value) || (dropper.IsSelf && AskToPortalSettings.autoAcceptSelf.Value) || (dropper.id == "" && AskToPortalSettings.autoAcceptWorld.Value))
            {
                return false;
            }
            return true;
        }
    }
}
