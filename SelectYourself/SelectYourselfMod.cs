using System;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UIExpansionKit.API;

[assembly: MelonInfo(typeof(SelectYourself.SelectYourselfMod), "SelectYourself", "1.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace SelectYourself
{
    public class SelectYourselfMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            MethodInfo clickMethod = typeof(VRCUiCursor).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Void_VRCPlayer_") && mi.GetParameters().Any(pi => pi.ParameterType == typeof(VRCPlayer)));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Select Yourself", new Action(() => { clickMethod.Invoke(VRCUiCursorManager.Method_Public_Static_VRCUiCursor_0(), new object[] { VRCPlayer.field_Internal_Static_VRCPlayer_0 }); }));
        }
    }
}
