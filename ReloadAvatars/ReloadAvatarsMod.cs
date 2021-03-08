using System;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;

[assembly: MelonInfo(typeof(ReloadAvatars.ReloadAvatarsMod), "TargetReload", "1.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ReloadAvatars
{
    public class ReloadAvatarsMod : MelonMod
    {
        public static GameObject reloadAvatarButton;
        public static GameObject reloadAllAvatarsButton;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("TargetReload", "TargetReload Settings");
            MelonPreferences.CreateEntry("TargetReload", "ReloadAvatar", true, "Enable/Disable Reload Avatar Button");
            MelonPreferences.CreateEntry("TargetReload", "ReloadAllAvatars", true, "Enable/Disable Reload All Avatars Button");

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Reload Avatar", new Action(() =>
            {
                try
                {
                    VRCPlayer.Method_Public_Static_Void_APIUser_0(QuickMenu.prop_QuickMenu_0.prop_APIUser_0);
                }
                catch { } // Ignore
            }), new Action<GameObject>((gameObject) => { reloadAvatarButton = gameObject; reloadAvatarButton.SetActive(MelonPreferences.GetEntryValue<bool>("TargetReload", "ReloadAvatar")); }));
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Reload All Avatars", new Action(() =>
            {
                try
                {
                    VRCPlayer.field_Internal_Static_VRCPlayer_0.Method_Public_Void_Boolean_0();
                }
                catch { } // Ignore
            }), new Action<GameObject>((gameObject) => { reloadAllAvatarsButton = gameObject; reloadAllAvatarsButton.SetActive(MelonPreferences.GetEntryValue<bool>("TargetReload", "ReloadAllAvatars")); }));
        }
        public override void OnPreferencesSaved()
        {
            reloadAvatarButton.SetActive(MelonPreferences.GetEntryValue<bool>("TargetReload", "ReloadAvatar"));
            reloadAllAvatarsButton.SetActive(MelonPreferences.GetEntryValue<bool>("TargetReload", "ReloadAllAvatars"));
        }
    }
}
