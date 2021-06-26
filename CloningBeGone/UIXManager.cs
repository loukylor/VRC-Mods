using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.Core;

namespace CloningBeGone
{
    class UIXManager
    {
        public static GameObject alwaysOnButton;
        public static GameObject alwaysOffButton;

        private static bool ignore;

        public static void Init() 
        {
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
            }), null, new Action<GameObject>((gameObject) => alwaysOnButton = gameObject));

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
            }), null, new Action<GameObject>((gameObject) => alwaysOffButton = gameObject));
        }

        public static void OnAvatarInstantiated(ApiAvatar avatar)
        {
            ignore = true;
            alwaysOnButton.GetComponent<Toggle>().isOn = CloningBeGoneMod.cloningOnAvatars.Value.Contains(avatar.id);
            alwaysOffButton.GetComponent<Toggle>().isOn = CloningBeGoneMod.cloningOffAvatars.Value.Contains(avatar.id);
            ignore = false;
        }
    }
}
