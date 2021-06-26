using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using VRC.Core;
using VRChatUtilityKit.Ui;

namespace InstanceHistory
{
    class WorldManager
    {
        public static event Action<ApiWorld, ApiWorldInstance> OnEnterWorldEvent;

        private static MethodInfo enterWorldMethod;
        private static Type transitionInfoEnum;

        public static void Init()
        {
            // Ty Ben (https://github.com/BenjaminZehowlt/) for this patch
            typeof(RoomManager).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name.StartsWith("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_"))
                .ToList().ForEach(m => InstanceHistoryMod.Instance.HarmonyInstance.Patch(m, prefix: new HarmonyMethod(typeof(WorldManager).GetMethod(nameof(OnEnterWorld), BindingFlags.Public | BindingFlags.Static))));

            transitionInfoEnum = typeof(WorldTransitionInfo).GetNestedTypes().First();
            enterWorldMethod = typeof(VRCFlowManager).GetMethod("Method_Public_Void_String_String_WorldTransitionInfo_Action_1_String_Boolean_0");
        }

        public static void OnEnterWorld(ApiWorld __0, ApiWorldInstance __1)
        {
            try
            {
                OnEnterWorldEvent.Invoke(__0, __1);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Something went wrong, this was most likely caused by your InstanceHistory.json file failing to parse correctly.\nHere is the error for debug purposes:\n" + ex.ToString());
            }
        }
        public static void EnterWorld(string roomId)
        {
            object currentPortalInfo = Activator.CreateInstance(typeof(WorldTransitionInfo), new object[2] { Enum.Parse(transitionInfoEnum, "Menu"), "WorldInfo_Go" });
            currentPortalInfo.GetType().GetProperty($"field_Public_{transitionInfoEnum.Name}_0").SetValue(currentPortalInfo, transitionInfoEnum.GetEnumValues().GetValue(3)); //I hate reflection
            enterWorldMethod.Invoke(VRCFlowManager.prop_VRCFlowManager_0, new object[5] { roomId.Split(':')[0], roomId.Split(':')[1], currentPortalInfo, (Il2CppSystem.Action<string>)new Action<string>((str) => UiManager.OpenAlertPopup("Cannot Join World")), false }); //Just kill me
        }
    }
}
