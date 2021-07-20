using System;
using System.IO;
using System.Net;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VRC.Core;
using VRC.UI;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;

namespace UserInfoExtensions.Modules
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

        public override void Init()
        {
            AuthorFromSocialMenuButton = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(AuthorFromSocialMenuButton), false, "Show \"Avatar Author\" button in Social Menu");
            AuthorFromAvatarMenuButton = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(AuthorFromAvatarMenuButton), true, "Show \"Avatar Author\" button in Avatar Menu");

            UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Avatar Author", FromSocial, new Action<GameObject>((gameObject) => { authorFromSocialMenuButtonGameObject = gameObject; gameObject.SetActive(AuthorFromSocialMenuButton.Value); }));
            UIExpansionKit.API.ExpansionKitApi.GetExpandedMenu(UIExpansionKit.API.ExpandedMenu.AvatarMenu).AddSimpleButton("Avatar Author", FromAvatar, new Action<GameObject>((gameObject) => { authorFromAvatarMenuButtonGameObject = gameObject; gameObject.SetActive(AuthorFromAvatarMenuButton.Value); }));
            UserInfoExtensionsMod.menu.AddSimpleButton("Avatar Author", FromSocial);
            UserInfoExtensionsMod.menu.AddSpacer();
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
                avatarLink = new Il2CppSystem.Uri(VRCUtils.ActiveUserInUserInfoMenu.currentAvatarImageUrl);

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
                UiManager.OpenErrorPopup("Something went wrong and the avatar author could not be retreived. Please try again");
                return;
            }

            if (!Utils.StartRequestTimer())
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
                UiManager.OpenErrorPopup("Something went wrong and the avatar author could not be retreived. Please try again");
                return;
            }
        }

        public static void FromAvatar()
        {
            UserInfoExtensionsMod.HideAllPopups();

            if (!Utils.StartRequestTimer())
                return;

            isFromSocialPage = false;
            OpenUserInSocialMenu(avatarPage.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0.authorId);
        }

        public static void OpenUserInSocialMenu(string userId) => APIUser.FetchUser(userId, new Action<APIUser>(OnUserFetched), new Action<string>((thing) => UiManager.OpenErrorPopup("Something went wrong and the author could not be retreived.")));
        private static void OnUserFetched(APIUser user)
        {
            if (isFromSocialPage && user.id == VRCUtils.ActiveUserInUserInfoMenu.id)
            {
                UiManager.OpenSmallPopup("Notice:", "You are already viewing the avatar author", "Close", new Action(UiManager.ClosePopup));
                return;
            }

            UiManager.OpenUserInUserInfoPage(user);
            if (isFromSocialPage)
                isFromSocialPage = false;
        }

        public struct JsonData
        {
            [JsonProperty("ownerId")]
            public string ownerId;
        }
    }
}
