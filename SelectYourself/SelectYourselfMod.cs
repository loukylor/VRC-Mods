using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using VRC;
using VRC.Core;

[assembly: MelonInfo(typeof(SelectYourself.SelectYourselfMod), "SelectYourself", "1.0.1", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace SelectYourself
{
    public class SelectYourselfMod : MelonMod
    {
        public static MelonPreferences_Entry<bool> selectYourselfPref;
        public static GameObject selectYourselfButton;
        public override void OnApplicationStart()
        {
            MelonPreferences_Category category = MelonPreferences.CreateCategory("SelectYourself", "SelectYourself Settings");
            selectYourselfPref = category.CreateEntry(nameof(selectYourselfPref), true, "Enable/Disable Select Yourself Button");

            List<Type> quickMenuNestedEnums = typeof(QuickMenu).GetNestedTypes().Where(type => type.IsEnum).ToList();
            PropertyInfo quickMenuEnumProperty = typeof(QuickMenu).GetProperties()
                .First(pi => pi.PropertyType.IsEnum && quickMenuNestedEnums.Contains(pi.PropertyType));
            MethodInfo setMenuIndex = typeof(QuickMenu).GetMethods()
                .First(mb => mb.Name.StartsWith("Method_Public_Void_Enum") && mb.GetParameters()[0].ParameterType == quickMenuEnumProperty.PropertyType);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Select Yourself",
                new Action(() => 
                {
                    QuickMenu.prop_QuickMenu_0.field_Private_Player_0 = Player.prop_Player_0;
                    QuickMenu.prop_QuickMenu_0.prop_APIUser_0 = APIUser.CurrentUser;
                    setMenuIndex.Invoke(QuickMenu.prop_QuickMenu_0, new object[1] { 3 });
                }),
                new Action<GameObject>((gameObject) => selectYourselfButton = gameObject));
        }
        public override void OnPreferencesSaved()
        {
            selectYourselfButton?.SetActive(selectYourselfPref.Value);
        }
    }
}
