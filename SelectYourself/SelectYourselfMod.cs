using System;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;

[assembly: MelonInfo(typeof(SelectYourself.SelectYourselfMod), "SelectYourself", "1.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace SelectYourself
{
    public class SelectYourselfMod : MelonMod
    {
        public static MelonPreferences_Entry<bool> selectYourselfPref;
        public static GameObject selectYourselfButton;
        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("SelectYourself", "SelectYourself Settings");
            selectYourselfPref = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("SelectYourself", nameof(selectYourselfPref), true, "Enable/Disable Select Yourself Button");

            MethodInfo clickMethod = typeof(VRCUiCursor).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Void_VRCPlayer_") && mi.GetParameters().Any(pi => pi.ParameterType == typeof(VRCPlayer)));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Select Yourself",
                new Action(() => { clickMethod.Invoke(VRCUiCursorManager.Method_Public_Static_VRCUiCursor_0(), new object[] { VRCPlayer.field_Internal_Static_VRCPlayer_0 }); }),
                new Action<GameObject>((gameObject) => selectYourselfButton = gameObject));
        }
        public override void OnPreferencesSaved()
        {
            selectYourselfButton?.SetActive(selectYourselfPref.Value);
        }
    }
}
