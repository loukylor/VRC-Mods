using System;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using VRC;
using VRChatUtilityKit.Ui;

[assembly: MelonInfo(typeof(SelectYourself.SelectYourselfMod), "SelectYourself", "1.0.5", "loukylor", "https://github.com/loukylor/VRC-Mods")]
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
            selectYourselfPref.OnValueChangedUntyped += OnPrefChange;

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Select Yourself",
                new Action(() => UiManager.OpenUserInQuickMenu(Player.prop_Player_0)),
                new Action<GameObject>((gameObject) => { selectYourselfButton = gameObject; OnPrefChange(); }));
        }
        public static void OnPrefChange()
        {
            selectYourselfButton?.SetActive(selectYourselfPref.Value);
        }
    }
}
