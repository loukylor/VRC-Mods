using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UserInfoExtensions;
using UserInfoExtentions.Utilities;
using VRC.Core;
using VRC.UI;

namespace UserInfoExtentions.Modules
{
    public class GetAvatarAuthor : ModuleBase
    {
        public static MelonPreferences_Entry<bool> AuthorFromSocialMenuButton;
        public static MelonPreferences_Entry<bool> AuthorFromAvatarMenuButton;

        public static GameObject authorFromSocialMenuButtonGameObject;
        public static GameObject authorFromAvatarMenuButtonGameObject;

        public static Il2CppSystem.Uri avatarLink;

        public static PageAvatar avatarPage;
        public static bool isFromSocialPage = false;

        private static PropertyInfo screenStackProp;

        private static MethodInfo setBigMenuEnum;

        public override void Init()
        {
            AuthorFromSocialMenuButton = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(AuthorFromSocialMenuButton), false, "Show \"Avatar Author\" button in Social Menu");
            AuthorFromAvatarMenuButton = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(AuthorFromAvatarMenuButton), true, "Show \"Avatar Author\" button in Avatar Menu");

            UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Avatar Author", FromSocial, new Action<GameObject>((gameObject) => { authorFromSocialMenuButtonGameObject = gameObject; gameObject.SetActive(AuthorFromSocialMenuButton.Value); }));
            UIExpansionKit.API.ExpansionKitApi.GetExpandedMenu(UIExpansionKit.API.ExpandedMenu.AvatarMenu).AddSimpleButton("Avatar Author", FromAvatar, new Action<GameObject>((gameObject) => { authorFromAvatarMenuButtonGameObject = gameObject; gameObject.SetActive(AuthorFromAvatarMenuButton.Value); }));
            UserInfoExtensionsMod.menu.AddSimpleButton("Avatar Author", FromSocial);

            screenStackProp = typeof(VRCUiManager).GetProperties().Where(pi => pi.Name.Contains("field_Internal_List_1_") && !pi.Name.Contains("String")).First();

            List<Type> quickMenuNestedEnums = typeof(QuickMenu).GetNestedTypes().Where(type => type.IsEnum).ToList();
            setBigMenuEnum = typeof(QuickMenu).GetMethods().First(mb => mb.Name.StartsWith("Method_Public_Void_Enum") && mb.GetParameters().Length == 2 && quickMenuNestedEnums.Contains(mb.GetParameters()[0].ParameterType) && mb.GetParameters()[1].ParameterType == typeof(bool));
        }
        public override void UiInit()
        {
            GameObject gameObject = GameObject.Find("UserInterface/MenuContent/Screens/Avatar");
            avatarPage = gameObject.GetComponent<PageAvatar>();
        }
        public override void OnPreferencesSaved()
        {
            authorFromSocialMenuButtonGameObject?.SetActive(AuthorFromSocialMenuButton.Value);
            authorFromAvatarMenuButtonGameObject?.SetActive(AuthorFromAvatarMenuButton.Value);
        }
        public override void OnUserInfoOpen()
        {
            try
            {
                avatarLink = new Il2CppSystem.Uri(VRCUtils.ActiveUser.currentAvatarImageUrl);

                string adjustedLink = string.Format("https://{0}", avatarLink.Authority);

                for (int i = 0; i < avatarLink.Segments.Length - 2; i++)
                {
                    adjustedLink += avatarLink.Segments[i];
                }

                avatarLink = new Il2CppSystem.Uri(adjustedLink.Trim("/".ToCharArray()));
            }
            catch
            {
                avatarLink = null;
            }
        }

        public static async void FromSocial()
        {
            UserInfoExtensionsMod.HideAllPopups();

            if (avatarLink == null)
            {
                VRCUtils.OpenPopupV2("Error!", "Something went wrong and the avatar author could not be retreived. Please try again", "Close", new Action(VRCUtils.ClosePopup));
                return;
            }

            if (!VRCUtils.StartRequestTimer())
                return;

            HttpWebRequest request = WebRequest.CreateHttp(avatarLink.OriginalString);

            try
            {
                request.UserAgent = "Mozilla/5.0";
                WebResponse response = await request.GetResponseAsync();
                isFromSocialPage = true;

                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                JObject jsonData = JObject.Parse(streamReader.ReadToEnd());
                JsonData requestedData = jsonData.ToObject<JsonData>();

                await AsyncUtils.YieldToMainThread();
                OpenUserInSocialMenu(requestedData.ownerId);

                response.Close();
                streamReader.Close();
            }
            catch (WebException)
            {
                await AsyncUtils.YieldToMainThread();
                VRCUtils.OpenPopupV2("Error!", "Something went wrong and the avatar author could not be retreived. Please try again", "Close", new Action(VRCUtils.ClosePopup));
                return;
            }
        }

        public static void FromAvatar()
        {
            UserInfoExtensionsMod.HideAllPopups();

            if (!VRCUtils.StartRequestTimer())
                return;

            isFromSocialPage = false;
            OpenUserInSocialMenu(avatarPage.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0.authorId);
        }

        public static void OpenUserInSocialMenu(string userId) => APIUser.FetchUser(userId, new Action<APIUser>(OnUserFetched), new Action<string>((thing) => { VRCUtils.OpenPopupV2("Error!", "Something went wrong and the author could not be retreived.", "Close", new Action(VRCUtils.ClosePopup)); }));
        private static void OnUserFetched(APIUser user)
        {
            if (isFromSocialPage && user.id == VRCUtils.ActiveUser.id)
            {
                VRCUtils.OpenPopupV2("Notice:", "You are already viewing the avatar author", "Close", new Action(VRCUtils.ClosePopup));
                return;
            }

            QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0 = user;
            setBigMenuEnum.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { 4, false });
            if (isFromSocialPage)
            {
                // For some reason when called from the Social Menu the screen stack adds the UserInfo page twice (making the user have to hit the back button twice to leave the page), so I'm just getting the screen stack using Reflection (as the name doesn't seem static) and removing the 2nd to last entry
                var listObject = screenStackProp.GetValue(VRCUiManager.prop_VRCUiManager_0);
                Type genericType = typeof(Il2CppSystem.Collections.Generic.List<>).MakeGenericType(new Type[] { typeof(VRCUiManager).GetNestedTypes().First() });
                genericType.GetMethod("RemoveAt").Invoke(listObject, new object[] { (int)genericType.GetProperty("Count").GetValue(listObject) - 1 });

                isFromSocialPage = false;
            }

        }


        public struct JsonData
        {
            [JsonProperty("ownerId")]
            public string ownerId;
        }
    }
}
