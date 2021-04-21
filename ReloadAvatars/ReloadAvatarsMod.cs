using System;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;

[assembly: MelonInfo(typeof(ReloadAvatars.ReloadAvatarsMod), "ReloadAvatars", "1.0.3", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ReloadAvatars
{
    public class ReloadAvatarsMod : MelonMod
    {
        public static GameObject reloadAvatarButton;
        public static GameObject reloadAllAvatarsButton;

        public static MelonPreferences_Entry<bool> reloadAvatarPref;
        public static MelonPreferences_Entry<bool> reloadAllAvatarsPref;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("ReloadAvatars", "ReloadAvatars Settings");
            reloadAvatarPref = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("ReloadAvatars", "ReloadAvatar", true, "Enable/Disable Reload Avatar Button");
            reloadAllAvatarsPref = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("ReloadAvatars", "ReloadAllAvatars", true, "Enable/Disable Reload All Avatars Button");

            MethodInfo reloadAvatarMethod = typeof(VRCPlayer).GetMethods().First(mi => mi.Name.StartsWith("Method_Private_Void_Boolean_") && mi.Name.Length < 31 && mi.GetParameters().Any(pi => pi.IsOptional));
            MethodInfo reloadAllAvatarsMethod = typeof(VRCPlayer).GetMethods().First(mi => mi.Name.StartsWith("Method_Public_Void_Boolean_") && mi.Name.Length < 30 && mi.GetParameters().Any(pi => pi.IsOptional));// Both methods seem to do the same thing

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Reload Avatar", new Action(() =>
            {
                try
                {
                    reloadAvatarMethod.Invoke(QuickMenu.prop_QuickMenu_0.field_Private_Player_0.field_Internal_VRCPlayer_0, new object[] { true });
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("Error while reloading single avatar:\n" + ex.ToString());
                } // Ignore
            }), new Action<GameObject>((gameObject) => { reloadAvatarButton = gameObject; reloadAvatarButton.SetActive(reloadAllAvatarsPref.Value); }));

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Reload All Avatars", new Action(() =>
            {
                try
                {
                    reloadAllAvatarsMethod.Invoke(VRCPlayer.field_Internal_Static_VRCPlayer_0, new object[] { true });
                    reloadAvatarMethod.Invoke(VRCPlayer.field_Internal_Static_VRCPlayer_0, new object[] { true }); // Ensure self is also reloaded (reload is not networked)
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("Error while reloading all avatars:\n" + ex.ToString());
                } // Ignore
            }), new Action<GameObject>((gameObject) => { reloadAllAvatarsButton = gameObject; reloadAllAvatarsButton.SetActive(reloadAvatarPref.Value); }));
            MelonLogger.Msg("Initialized!");
        }
        public override void OnPreferencesSaved()
        {
            reloadAvatarButton?.SetActive(reloadAvatarPref.Value);
            reloadAllAvatarsButton?.SetActive(reloadAllAvatarsPref.Value);
        }
    }
}
