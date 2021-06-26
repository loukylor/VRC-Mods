using System;
using System.Linq;
using System.Reflection;
using VRC.Core;
using VRChatUtilityKit.Utilities;
using VRChatUtilityKit.Ui;

namespace AskToPortal
{
    class PortalUtils
    {
        internal static Type portalInfo; //Dunno if the object name is static so lets just xref 
        internal static Type portalInfoEnum;

        internal static MethodInfo enterPortal;
        internal static MethodInfo enterWorld;

        internal static void Init()
        {
            portalInfo = typeof(VRCFlowManager).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_String_WorldTransitionInfo_")).First().GetParameters()[1].ParameterType;
            portalInfoEnum = portalInfo.GetNestedTypes().First();
            enterWorld = typeof(VRCFlowManager).GetMethods()
                .Where(mb => mb.Name.StartsWith($"Method_Public_Void_String_String_{portalInfo.Name}_Action_1_String_Boolean_") && !mb.Name.Contains("PDM") && XrefUtils.CheckMethod(mb, "EnterWorld called with an invalid world id.")).First();
            enterPortal = typeof(PortalInternal).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_") && mb.Name.Length <= 21 && XrefUtils.CheckUsedBy(mb, "OnTriggerEnter")).First();

        }
        public static void EnterPortal(PortalInternal portalInternal, APIUser dropper, string worldId, string roomId)
        {
            if (portalInternal == null)
            {
                object currentPortalInfo = Activator.CreateInstance(portalInfo);
                currentPortalInfo.GetType().GetProperty($"field_Public_{portalInfoEnum.Name}_0").SetValue(currentPortalInfo, 3); //I hate reflection
                typeof(Il2CppSystem.Collections.Generic.Dictionary<string, string>).GetMethod("Add").Invoke(currentPortalInfo.GetType().GetProperty("field_Public_Dictionary_2_String_String_0").GetValue(currentPortalInfo), new object[2] { "transitionPortalType", dropper.id == "" ? "Static" : "Dynamic" });
                typeof(Il2CppSystem.Collections.Generic.Dictionary<string, string>).GetMethod("Add").Invoke(currentPortalInfo.GetType().GetProperty("field_Public_Dictionary_2_String_String_0").GetValue(currentPortalInfo), new object[2] { "transitionPortalOwner", dropper.id });
                enterWorld.Invoke(VRCFlowManager.prop_VRCFlowManager_0, new object[5] { worldId, roomId, currentPortalInfo, (Il2CppSystem.Action<string>)new Action<string>((str) => UiManager.OpenAlertPopup("Cannot Join World")), false }); //Just kill me
            }
            else
            {
                AskToPortalMod.shouldInterrupt = false;
                enterPortal.Invoke(portalInternal, null);
                AskToPortalMod.shouldInterrupt = true;
            }
        }
    }
}
