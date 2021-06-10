using System;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UserInfoExtensions;
using UserInfoExtentions.Utilities;
using VRC.Core;
using VRC.UI;

namespace UserInfoExtentions.Modules
{
    public class OpenInWorldMenu : ModuleBase
    {
        public static MelonPreferences_Entry<bool> OpenInWorldMenuButton;

        public static GameObject openInWorldMenuButtonGameObject;

        public static MethodInfo setUpWorldInfo;
        public override void Init()
        {
            OpenInWorldMenuButton = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(OpenInWorldMenuButton), false, "Show\"Open User World in World Menu\" button");

            UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Open User World in World Menu", OpenUserWorldInWorldMenu, new Action<GameObject>((gameObject) => { openInWorldMenuButtonGameObject = gameObject; gameObject.SetActive(OpenInWorldMenuButton.Value); }));
            UserInfoExtensionsMod.menu.AddSimpleButton("Open User World in World Menu", OpenUserWorldInWorldMenu);

            setUpWorldInfo = typeof(PageWorldInfo).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_ApiWorld_ApiWorldInstance_Boolean_Boolean_") && !mb.Name.Contains("_PDM_") && Xref.CheckMethod(mb, "NEW INSTANCE")).First();
        }
        public override void OnPreferencesSaved()
        {
            openInWorldMenuButtonGameObject?.SetActive(OpenInWorldMenuButton.Value);
        }

        public static void OpenUserWorldInWorldMenu()
        {
            UserInfoExtensionsMod.HideAllPopups();

            string location;
            if (!VRCUtils.ActiveUser.IsSelf)
            {
                location = VRCUtils.ActiveUser.location;
            }
            else
            {
                location = APIUser.CurrentUser.location;
            }
            if (VRCUtils.userInfo.field_Private_ApiWorld_1 != null && !(string.IsNullOrEmpty(location) || location == "private" || location == "offline"))
            {
                string processedLocation = VRCUtils.ActiveUser.location.Split(new char[] { ':' }, 2)[1];
                int count;
                try
                {
                    count = VRCUtils.userInfo.field_Private_ApiWorld_1.instances[processedLocation];
                }
                catch
                {
                    count = 0;
                }
                ApiWorldInstance instance = new ApiWorldInstance(VRCUtils.userInfo.field_Private_ApiWorld_1, processedLocation, count);
                setUpWorldInfo.Invoke(VRCUtils.worldInfo, new object[] { VRCUtils.userInfo.field_Private_ApiWorld_1, instance, null, null });
                VRCUiManager.prop_VRCUiManager_0.ShowScreenButton("UserInterface/MenuContent/Screens/WorldInfo");
            }
            else
            {
                VRCUtils.OpenPopupV2("Notice", "Cannot grab this user's world", "Close", new Action(VRCUtils.ClosePopup));
            }
        }
    }
}
