using System;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRChatUtilityKit.Ui;

namespace CloningBeGone
{
    class UIXManager
    {
        public static MelonPreferences_Entry<bool> enableAlwaysOnButton;
        public static MelonPreferences_Entry<bool> enableAlwaysOffButton;

        public static GameObject alwaysOnButton;
        public static GameObject alwaysOffButton;

        private static bool ignore;

        public static void Init() 
        {
            enableAlwaysOnButton = CloningBeGoneMod.category.CreateEntry(nameof(enableAlwaysOnButton), true, "Enable/Disable the \"Always have cloning on with this avatar\" button");
            enableAlwaysOffButton = CloningBeGoneMod.category.CreateEntry(nameof(enableAlwaysOffButton), true, "Enable/Disable the \"Always have cloning off with this avatar\" button");
            enableAlwaysOnButton.OnValueChangedUntyped += OnPrefChanged;
            enableAlwaysOffButton.OnValueChangedUntyped += OnPrefChanged;

            UiManager.OnQuickMenuIndexSet += OnSetMenuIndexCalled;

            ICustomLayoutedMenu menu = ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu);
            menu.AddToggleButton("Always have cloning on with this avatar", new Action<bool>((state) => 
            {
                if (ignore)
                    return;

                if (state)
                    CloningBeGoneMod.cloningOnAvatars.Value.Add(Player.prop_Player_0.prop_ApiAvatar_0.id);
                else
                    CloningBeGoneMod.cloningOnAvatars.Value.Remove(Player.prop_Player_0.prop_ApiAvatar_0.id);

                if (RoomManager.field_Internal_Static_ApiWorldInstance_0 != null)
                    CloningBeGoneMod.CheckAccessType(RoomManager.field_Internal_Static_ApiWorldInstance_0.type);
                CloningBeGoneMod.OnAvatarInstantiated(Player.prop_Player_0.prop_ApiAvatar_0);
            }), null, new Action<GameObject>((gameObject) => { alwaysOnButton = gameObject; alwaysOnButton?.SetActive(enableAlwaysOnButton.Value); }));

            menu.AddToggleButton("Always have cloning off with this avatar", new Action<bool>((state) =>
            {
                if (ignore)
                    return;

                if (state)
                    CloningBeGoneMod.cloningOffAvatars.Value.Add(Player.prop_Player_0.prop_ApiAvatar_0.id);
                else
                    CloningBeGoneMod.cloningOffAvatars.Value.Remove(Player.prop_Player_0.prop_ApiAvatar_0.id);

                if (RoomManager.field_Internal_Static_ApiWorldInstance_0 != null)
                    CloningBeGoneMod.CheckAccessType(RoomManager.field_Internal_Static_ApiWorldInstance_0.type);
                CloningBeGoneMod.OnAvatarInstantiated(Player.prop_Player_0.prop_ApiAvatar_0);
            }), null, new Action<GameObject>((gameObject) => { alwaysOffButton = gameObject; alwaysOffButton?.SetActive(enableAlwaysOnButton.Value); }));
        }

        public static void OnSetMenuIndexCalled(int index)
        {
            if (index != 0)
                return;

            ignore = true;
            if (alwaysOnButton.active)
                alwaysOnButton.GetComponent<Toggle>().isOn = CloningBeGoneMod.cloningOnAvatars.Value.Contains(Player.prop_Player_0.prop_ApiAvatar_0.id);
            if (alwaysOffButton.active)
                alwaysOffButton.GetComponent<Toggle>().isOn = CloningBeGoneMod.cloningOffAvatars.Value.Contains(Player.prop_Player_0.prop_ApiAvatar_0.id);
            ignore = false;
        }

        private static void OnPrefChanged()
        {
            alwaysOnButton?.SetActive(enableAlwaysOnButton.Value);
            alwaysOffButton?.SetActive(enableAlwaysOffButton.Value);
        }
    }
}
