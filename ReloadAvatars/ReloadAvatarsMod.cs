using System;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using VRChatUtilityKit.Utilities;

[assembly: MelonInfo(typeof(ReloadAvatars.ReloadAvatarsMod), "ReloadAvatars", "1.0.5", "loukylor", "https://github.com/loukylor/VRC-Mods")]
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
            MelonPreferences_Category category = MelonPreferences.CreateCategory("ReloadAvatars", "ReloadAvatars Settings");
            reloadAvatarPref = category.CreateEntry("ReloadAvatar", true, "Enable/Disable Reload Avatar Button");
            reloadAllAvatarsPref = category.CreateEntry("ReloadAllAvatars", true, "Enable/Disable Reload All Avatars Button");

            foreach (MelonPreferences_Entry entry in category.Entries)
                entry.OnValueChangedUntyped += OnPrefChanged;

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Reload Avatar", new Action(() =>
            {
                try
                {
                    VRCUtils.ReloadAvatar(VRCUtils.ActivePlayerInQuickMenu.prop_VRCPlayer_0);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("Error while reloading single avatar:\n" + ex.ToString());
                } // Ignore
            }), new Action<GameObject>((gameObject) => { reloadAvatarButton = gameObject; reloadAvatarButton.SetActive(reloadAllAvatarsPref.Value); OnPrefChanged(); }));

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Reload All Avatars", new Action(() =>
            {
                try
                {
                    VRCUtils.ReloadAllAvatars();
                    VRCUtils.ReloadAvatar(VRCPlayer.field_Internal_Static_VRCPlayer_0);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("Error while reloading all avatars:\n" + ex.ToString());
                } // Ignore
            }), new Action<GameObject>((gameObject) => { reloadAllAvatarsButton = gameObject; reloadAllAvatarsButton.SetActive(reloadAvatarPref.Value); OnPrefChanged(); }));
            MelonLogger.Msg("Initialized!");
        }
        public void OnPrefChanged()
        {
            reloadAvatarButton?.SetActive(reloadAvatarPref.Value);
            reloadAllAvatarsButton?.SetActive(reloadAllAvatarsPref.Value);
        }
    }
}
