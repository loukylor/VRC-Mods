using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnhollowerRuntimeLib;
using UnityEngine;
using UserInfoExtensions;
using VRC;
using VRC.Core;
using VRC.UI;

namespace UserInfoExtentions.Modules
{
    public class QuickMenuFromSocial
    {
        public static MethodBase closeMenu;
        public static MethodBase openQuickMenu;

        public static void Init()
        {
            if (UserInfoExtensionsSettings.QuickMenuFromSocialButton) UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("To Quick Menu", ToQuickMenu);
            UserInfoExtensionsMod.menu.AddSimpleButton("To Quick Menu", ToQuickMenu);

            closeMenu = typeof(VRCUiManager).GetMethods()
                            .Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && !mb.Name.Contains("_PDM_") && mb.GetParameters().Where(pi => pi.HasDefaultValue && (bool)pi.DefaultValue == false).Count() == 2).First();
            openQuickMenu = typeof(QuickMenu).GetMethods()
                            .Where(mb => mb.Name.StartsWith("Method_Public_Void_Boolean_") && mb.Name.Length <= 29 && mb.GetParameters().Any(pi => pi.HasDefaultValue == false)).First();
        }
        public static void ToQuickMenu()
        {
            UserInfoExtensionsMod.HideAllPopups();

            foreach (Player player in PlayerManager.prop_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player.field_Private_APIUser_0 == null) continue;
                if (player.field_Private_APIUser_0.id == Utilities.ActiveUser.id)
                {
                    closeMenu.Invoke(VRCUiManager.prop_VRCUiManager_0, new object[] { true, false }); //Closes Big Menu
                    openQuickMenu.Invoke(QuickMenu.prop_QuickMenu_0, new object[] { true }); //Opens Quick Menu
                    QuickMenu.prop_QuickMenu_0.Method_Public_Void_Player_0(PlayerManager.Method_Public_Static_Player_String_0(Utilities.ActiveUser.id)); //Does the rest lmao
                    return;
                }
            }
            Utilities.OpenPopupV2("Notice:", "You cannot show this user on the Quick Menu because they are not in the same instance", "Close", new Action(() => Utilities.ClosePopup()));
        }
    }
    public class GetAvatarAuthor
    {
        public static Il2CppSystem.Uri avatarLink;
        public static bool canGet = true;

        public static PageAvatar avatarPage;
        public static bool isFromSocialPage = false;

        public static Type genericType;
        public static PropertyInfo screenStackProp;

        public static void Init()
        {
            if (UserInfoExtensionsSettings.AuthorFromSocialMenuButton) UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Avatar Author", FromSocial);
            if (UserInfoExtensionsSettings.AuthorFromAvatarMenuButton) UIExpansionKit.API.ExpansionKitApi.GetExpandedMenu(UIExpansionKit.API.ExpandedMenu.AvatarMenu).AddSimpleButton("Avatar Author", FromAvatar);
            UserInfoExtensionsMod.menu.AddSimpleButton("Avatar Author", FromSocial);

            screenStackProp = typeof(VRCUiManager).GetProperties().Where(pi => pi.Name.Contains("field_Internal_List_1_") && !pi.Name.Contains("String")).First();
        }
        public static void UiInit()
        {
            GameObject gameObject = GameObject.Find("UserInterface/MenuContent/Screens/Avatar");
            avatarPage = gameObject.GetComponent<PageAvatar>();
        }
        public static void OnUserInfoOpen()
        {
            avatarLink = new Il2CppSystem.Uri(Utilities.ActiveUser.currentAvatarImageUrl);

            string adjustedLink = string.Format("https://{0}", avatarLink.Authority);

            for (int i = 0; i < avatarLink.Segments.Length - 2; i++)
            {
                adjustedLink += avatarLink.Segments[i];
            }

            avatarLink = new Il2CppSystem.Uri(adjustedLink.Trim("/".ToCharArray()));
        }

        public static async void FromSocial()
        {
            UserInfoExtensionsMod.HideAllPopups();

            if (!canGet)
            {
                Utilities.OpenPopupV2("Slow down!", "Please wait a little in between button presses", "Close", new Action(() => Utilities.ClosePopup()));
                return;
            }

            MelonCoroutines.Start(StartTimer());

            WebRequest request = WebRequest.Create(avatarLink.OriginalString);

            try
            {
                WebResponse response = await request.GetResponseAsync().NoAwait();
                isFromSocialPage = true;

                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                JObject jsonData = JObject.Parse(streamReader.ReadToEnd());
                JsonData requestedData = jsonData.ToObject<JsonData>();

                await Utilities.YieldToMainThread();
                OpenUserInSocialMenu(requestedData.ownerId);

                response.Close();
                streamReader.Close();
            }
            catch
            {
                Utilities.OpenPopupV2("Error!", "Something went wrong and the author could not be retreived. Please try again", "Close", new Action(() => Utilities.ClosePopup()));
                return;
            }
        }

        public static void FromAvatar()
        {
            UserInfoExtensionsMod.HideAllPopups();

            if (!canGet)
            {
                Utilities.OpenPopupV2("Slow down!", "Please wait a little in between button presses", "Close", new Action(() => Utilities.ClosePopup()));
                return;
            }

            MelonCoroutines.Start(StartTimer());

            isFromSocialPage = false;
            OpenUserInSocialMenu(avatarPage.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0.authorId);
        }

        public static IEnumerator StartTimer()
        {
            canGet = false;

            float endTime = Time.time + 3.5f;

            while (Time.time < endTime)
            {
                yield return null;
            }

            canGet = true;
            yield break;
        }

        public static void OpenUserInSocialMenu(string userId) => APIUser.FetchUser(userId, new Action<APIUser>(OnUserFetched), new Action<string>((thing) => { Utilities.OpenPopupV2("Error!", "Something went wrong and the author could not be retreived.", "Close", new Action(() => Utilities.ClosePopup())); }));
        private static void OnUserFetched(APIUser user)
        {
            if (isFromSocialPage && user.id == Utilities.ActiveUser.id)
            {
                Utilities.OpenPopupV2("Notice:", "You are already viewing the avatar author", "Close", new Action(() => Utilities.ClosePopup()));
                return;
            }

            QuickMenu.prop_QuickMenu_0.field_Private_APIUser_0 = user;
            QuickMenu.prop_QuickMenu_0.Method_Public_Void_Int32_Boolean_0(4, false);
            if (isFromSocialPage)
            {
                // For some reason when called from the Social Menu the screen stack adds the UserInfo page twice (making the user have to hit the back button twice to leave the page), so I'm just getting the screen stack using Reflection (as the name doesn't seem static) and removing the 2nd to last entry
                var listObject = screenStackProp.GetValue(VRCUiManager.prop_VRCUiManager_0);
                Type genericType = typeof(Il2CppSystem.Collections.Generic.List<>).MakeGenericType(new Type[] { typeof(VRCUiManager).GetNestedTypes().First() });
                genericType.GetMethod("RemoveAt").Invoke(listObject, new object[] { (int)genericType.GetProperty("Count").GetValue(listObject) - 1 });

                isFromSocialPage = false;
            }

        }


        public class JsonData
        {
            [JsonProperty("ownerId")]
            public string ownerId;
        }
    }
    public class OpenInWorldMenu
    {
        public static MethodInfo setUpWorldInfo;
        public static void Init()
        {
            if (UserInfoExtensionsSettings.OpenUserWorldInWorldMenu) UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Open User World in World Menu", OpenUserWorldInWorldMenu);
            UserInfoExtensionsMod.menu.AddSimpleButton("Open User World in World Menu", OpenUserWorldInWorldMenu);

            setUpWorldInfo = typeof(PageWorldInfo).GetMethods()
                .Where(mb => mb.Name.StartsWith("Method_Public_Void_ApiWorld_ApiWorldInstance_Boolean_Boolean_") && !mb.Name.Contains("_PDM_") && Utilities.CheckMethod(mb, "NEW INSTANCE")).First();
        }

        public static void OpenUserWorldInWorldMenu()
        {
            UserInfoExtensionsMod.HideAllPopups();

            string location;
            if (!Utilities.ActiveUser.IsSelf)
            {
                location = Utilities.ActiveUser.location;
            }
            else
            {
                location = APIUser.CurrentUser.location;
            }
            if (Utilities.userInfo.field_Private_ApiWorld_1 != null && !(string.IsNullOrEmpty(location) || location == "private"))
            {
                string processedLocation = Utilities.ActiveUser.location.Split(new char[] { ':' }, 2)[1];
                int count;
                try
                {
                    count = Utilities.userInfo.field_Private_ApiWorld_1.instances[processedLocation];
                }
                catch
                {
                    count = 0;
                }
                ApiWorldInstance instance = new ApiWorldInstance(Utilities.userInfo.field_Private_ApiWorld_1, processedLocation, count);
                setUpWorldInfo.Invoke(Utilities.worldInfo, new object[] { Utilities.userInfo.field_Private_ApiWorld_1, instance, null, null });
                VRCUiManager.prop_VRCUiManager_0.ShowScreenButton("UserInterface/MenuContent/Screens/WorldInfo");
            }
            else
            {
                Utilities.OpenPopupV2("Notice", "Cannot grab this user's world", "Close", new Action(() => Utilities.ClosePopup()));
            }
        }
    }

    public class BioButtons
    {
        public static Component.BioLinksPopup bioLinksPopup;
        public static Component.BioLanguagesPopup bioLanguagesPopup;
        public static List<Uri> bioLinks = new List<Uri>();
        public static List<string> userLanguages = new List<string>();
        public readonly static Dictionary<string, string> languageLookup = new Dictionary<string, string>
        {
            { "eng", "[ eng ] English" },
            { "kor", "[ kor ] 한국어" },
            { "rus", "[ rus ] Русский" },
            { "spa", "[ spa ] Español" },
            { "por", "[ por ] Português" },
            { "zho", "[ zho ] 中文" },
            { "deu", "[ deu ] Deutsch" },
            { "jpn", "[ jpn ] 日本語" },
            { "fra", "[ fra ] Français" },
            { "swe", "[ swe ] Svenska" },
            { "nld", "[ nld ] Nederlands" },
            { "pol", "[ pol ] Polski" },
            { "dan", "[ dan ] Dansk" },
            { "nor", "[ nor ] Norsk" },
            { "ita", "[ ita ] Italiano" },
            { "tha", "[ tha ] ภาษาไทย" },
            { "fin", "[ fin ] Suomi" },
            { "hun", "[ hun ] Magyar" },
            { "ces", "[ ces ] Čeština" },
            { "tur", "[ tur ] Türkçe" },
            { "ara", "[ ara ] العربية" },
            { "ron", "[ ron ] Română" },
            { "vie", "[ vie ] Tiếng Việt" },
            { "ase", "[ ase ] American Sign Language" },
            { "bfi", "[ bfi ] British Sign Language" },
            { "dse", "[ dse ] Dutch Sign Language" },
            { "fsl", "[ fsl ] French Sign Language" },
            { "kvk", "[ kvk ] Korean Sign Language" },
        };

        public static void Init()
        {
            if (UserInfoExtensionsSettings.BioButton) UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Bio", GetBio);
            if (UserInfoExtensionsSettings.BioLinksButton) UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Bio Links", ShowBioLinksPopup);
            if (UserInfoExtensionsSettings.BioLanguagesButton) UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Bio Languages", ShowBioLanguagesPopup);

            UserInfoExtensionsMod.menu.AddLabel("Bio Related Things");
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSimpleButton("Bio", GetBio);
            UserInfoExtensionsMod.menu.AddSimpleButton("Bio Links", ShowBioLinksPopup);
            UserInfoExtensionsMod.menu.AddSimpleButton("Bio Languages", ShowBioLanguagesPopup);
        }
        public static void UiInit() //This is a shit show but it works so shshshhhshh
        {
            ClassInjector.RegisterTypeInIl2Cpp<Component.BioLinksPopup>();
            GameObject popupGameObject = GameObject.Find("UserInterface/MenuContent/Popups/UpdateStatusPopup");
            popupGameObject = UnityEngine.Object.Instantiate(popupGameObject, popupGameObject.transform.parent);
            UnityEngine.Object.DestroyImmediate(popupGameObject.GetComponent<PopupUpdateStatus>());
            UnityEngine.Object.DestroyImmediate(popupGameObject.transform.Find("Popup/StatusSettings/DoNotDisturbStatus").gameObject);
            UnityEngine.Object.DestroyImmediate(popupGameObject.transform.Find("Popup/StatusSettings/OfflineStatus").gameObject);
            UnityEngine.Object.DestroyImmediate(popupGameObject.transform.Find("Popup/InputFieldStatus").gameObject);
            foreach (I2.Loc.Localize component in popupGameObject.GetComponentsInChildren<I2.Loc.Localize>()) UnityEngine.Object.Destroy(component);

            bioLinksPopup = popupGameObject.AddComponent<Component.BioLinksPopup>();

            bioLinksPopup.field_Public_String_0 = "LINKS_POPUP"; //Required to make popup work

            bioLinksPopup.field_Public_Button_0 = popupGameObject.transform.Find("Popup/ExitButton").GetComponent<UnityEngine.UI.Button>();
            bioLinksPopup.field_Public_Button_0.onClick.AddListener((UnityEngine.Events.UnityAction)(() => bioLinksPopup.Close()));

            bioLinksPopup.toggleGroup = popupGameObject.GetComponent<UnityEngine.UI.ToggleGroup>();

            bioLinksPopup.openLinkButton = popupGameObject.transform.Find("Popup/Buttons/UpdateButton").GetComponent<UnityEngine.UI.Button>();
            bioLinksPopup.openLinkButton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            bioLinksPopup.openLinkButton.onClick.AddListener(new Action(() => bioLinksPopup.OnOpenLink()));
            bioLinksPopup.openLinkButton.gameObject.name = "OpenLinkButton";
            bioLinksPopup.openLinkButton.transform.GetComponentInChildren<UnityEngine.UI.Text>().text = "Open Link";

            popupGameObject.transform.Find("Popup/UpdateStatusTitleText").GetComponent<UnityEngine.UI.Text>().text = "Open Bio Link";

            bioLinksPopup.linkTexts = new UnityEngine.UI.Text[3];
            bioLinksPopup.icons = new UnityEngine.UI.RawImage[3];
            bioLinksPopup.linkStates = new GameObject[3];
            Transform statusSettings = popupGameObject.transform.Find("Popup/StatusSettings");
            for (int i = 0; i < 3; i++)
            {
                UnityEngine.UI.Toggle toggle = statusSettings.GetChild(i).GetComponent<UnityEngine.UI.Toggle>();
                bioLinksPopup.linkStates[i] = toggle.gameObject;

                UnityEngine.Object.DestroyImmediate(toggle.transform.FindChild("StatusIcon").GetComponent<UiStatusIcon>());
                bioLinksPopup.icons[i] = toggle.transform.FindChild("StatusIcon").GetComponent<UnityEngine.UI.RawImage>();

                toggle.transform.FindChild("StatusIcon").name = "WebsiteIcon";

                bioLinksPopup.toggleGroup.RegisterToggle(toggle);
                toggle.group = bioLinksPopup.toggleGroup;
                toggle.onValueChanged.AddListener(new Action<bool>((state) =>
                {
                    if (!state)
                    {
                        toggle.transform.FindChild("Description").GetComponent<UnityEngine.UI.Text>().color = new Color(0.5f, 0.5f, 0.5f);
                    }
                    else
                    {
                        toggle.transform.FindChild("Description").GetComponent<UnityEngine.UI.Text>().color = Color.white;

                        bioLinksPopup.currentLink = bioLinks[toggle.transform.GetSiblingIndex()];
                    }
                }));

                toggle.transform.FindChild("Description").GetComponent<UnityEngine.UI.Text>().color = new Color(0.5f, 0.5f, 0.5f);

                bioLinksPopup.linkTexts[i] = toggle.transform.FindChild("Description").GetComponent<UnityEngine.UI.Text>();

                toggle.transform.gameObject.name = $"BioLink{i + 1}";
            }

            bioLinksPopup.gameObject.name = "BioLinksPopup";

            ClassInjector.RegisterTypeInIl2Cpp<Component.BioLanguagesPopup>();
            popupGameObject = GameObject.Find("UserInterface/MenuContent/Popups/UpdateStatusPopup");
            popupGameObject = UnityEngine.Object.Instantiate(popupGameObject, popupGameObject.transform.parent);
            UnityEngine.Object.DestroyImmediate(popupGameObject.GetComponent<PopupUpdateStatus>());
            UnityEngine.Object.DestroyImmediate(popupGameObject.transform.Find("Popup/StatusSettings/DoNotDisturbStatus").gameObject);
            UnityEngine.Object.DestroyImmediate(popupGameObject.transform.Find("Popup/StatusSettings/OfflineStatus").gameObject);
            UnityEngine.Object.DestroyImmediate(popupGameObject.transform.Find("Popup/InputFieldStatus").gameObject);
            foreach (I2.Loc.Localize component in popupGameObject.GetComponentsInChildren<I2.Loc.Localize>()) UnityEngine.Object.Destroy(component);

            bioLanguagesPopup = popupGameObject.AddComponent<Component.BioLanguagesPopup>();

            bioLanguagesPopup.field_Public_String_0 = "LANGUAGES_POPUP"; //Required to make popup work

            bioLanguagesPopup.field_Public_Button_0 = popupGameObject.transform.Find("Popup/ExitButton").GetComponent<UnityEngine.UI.Button>();
            bioLanguagesPopup.field_Public_Button_0.onClick.AddListener((UnityEngine.Events.UnityAction)(() => bioLanguagesPopup.Close()));

            bioLanguagesPopup.closeButton = popupGameObject.transform.Find("Popup/Buttons/UpdateButton").GetComponent<UnityEngine.UI.Button>();
            bioLanguagesPopup.closeButton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            bioLanguagesPopup.closeButton.onClick.AddListener(new Action(() => bioLanguagesPopup.Close()));
            bioLanguagesPopup.closeButton.gameObject.name = "CloseButton";
            bioLanguagesPopup.closeButton.transform.GetComponentInChildren<UnityEngine.UI.Text>().text = "Close";

            popupGameObject.transform.Find("Popup/UpdateStatusTitleText").GetComponent<UnityEngine.UI.Text>().text = "Bio Languages";

            bioLanguagesPopup.languageTexts = new UnityEngine.UI.Text[3];
            bioLanguagesPopup.languageStates = new GameObject[3];
            statusSettings = popupGameObject.transform.Find("Popup/StatusSettings");
            for (int i = 0; i < 3; i++)
            {
                UnityEngine.Object.Destroy(statusSettings.GetChild(i).GetComponent<UnityEngine.UI.Toggle>());
                bioLanguagesPopup.languageStates[i] = statusSettings.GetChild(i).gameObject;

                UnityEngine.Object.DestroyImmediate(statusSettings.GetChild(i).transform.FindChild("StatusIcon").GetComponent<UiStatusIcon>());

                statusSettings.GetChild(i).transform.FindChild("StatusIcon").GetComponent<UnityEngine.UI.RawImage>().color = Color.white;
                UnityEngine.Object.DestroyImmediate(statusSettings.GetChild(i).transform.FindChild("Highlight").gameObject);

                statusSettings.GetChild(i).transform.FindChild("Description").GetComponent<UnityEngine.UI.Text>().color = Color.white;

                bioLanguagesPopup.languageTexts[i] = statusSettings.GetChild(i).transform.FindChild("Description").GetComponent<UnityEngine.UI.Text>();

                statusSettings.GetChild(i).transform.gameObject.name = $"BioLanguage{i + 1}";
            }

            bioLanguagesPopup.gameObject.name = "BioLanguagesPopup";
        }
        public static void OnUserInfoOpen()
        {
            userLanguages.Clear();
            foreach (string tag in Utilities.ActiveUser.tags) //Cant use where here because Il2Cpp List and regular List
            {
                if (tag.StartsWith("language_")) userLanguages.Add(languageLookup[tag.Substring(9)]);
            }
        }
        public static void OnPageOpen(VRCUiPage __0)
        {
            // This field (which is very important) is literally changed at random during runtime, it changes to random numbers to an invalid string so i have to set it before the page opens
            if (__0.TryCast<Component.BioLanguagesPopup>() != null) __0.field_Public_String_0 = "LINKS_POPUP";
            if (__0.TryCast<Component.BioLinksPopup>() != null) __0.field_Public_String_0 = "BIO_LINKS";
        }

        public static void GetBio()
        {
            UserInfoExtensionsMod.HideAllPopups();

            if (Utilities.ActiveUser.bio != null && Utilities.ActiveUser.bio.Length >= 100)
            {
                Utilities.OpenPopupV1("Bio:", Utilities.ActiveUser.bio, "Close", new Action(() => Utilities.ClosePopup()));
            }
            else
            {
                Utilities.OpenPopupV2("Bio:", Utilities.ActiveUser.bio, "Close", new Action(() => Utilities.ClosePopup()));
            }
        }
        public static void ShowBioLinksPopup()
        {
            UserInfoExtensionsMod.HideAllPopups();

            if (Utilities.ActiveUser.bioLinks == null)
            {
                Utilities.OpenPopupV2("Notice:", "Cannot get users links", "Close", new Action(() => Utilities.ClosePopup()));
            }
            else
            {
                CheckLinks(Utilities.ActiveUser.bioLinks);
                if (Utilities.ActiveUser.bioLinks.Count == 0)
                {
                    Utilities.OpenPopupV2("Notice:", "This user has no bio links", "Close", new Action(() => Utilities.ClosePopup()));
                }
                else if (bioLinks.Count == 0)
                {
                    Utilities.OpenPopupV2("Notice:", "This user has invalid links", "Close", new Action(() => Utilities.ClosePopup()));
                }
                else
                {
                    VRCUiManager.prop_VRCUiManager_0.ShowScreenButton("UserInterface/MenuContent/Popups/BioLinksPopup");
                }
            }
        }
        public static void CheckLinks(Il2CppSystem.Collections.Generic.List<string> checkLinks)
        {
            bioLinks = new List<Uri>();
            foreach (string link in checkLinks)
            {
                Uri checkedLink;
                try
                {
                    checkedLink = new Uri(link);
                }
                catch
                {
                    continue;
                }
                bioLinks.Add(checkedLink);
            }
        }

        public static void ShowBioLanguagesPopup()
        {
            UserInfoExtensionsMod.HideAllPopups();

            if (userLanguages == null || userLanguages.Count == 0)
            {
                Utilities.OpenPopupV2("Notice:", "This user has no bio languages", "Close", new Action(() => Utilities.ClosePopup()));
            }
            else
            {
                VRCUiManager.prop_VRCUiManager_0.ShowScreenButton("UserInterface/MenuContent/Popups/BioLanguagesPopup");
            }
        }
    }

    public class OpenInBrowser
    {
        public static void Init()
        {
            if (UserInfoExtensionsSettings.OpenUserInBrowserButton) UserInfoExtensionsMod.userDetailsMenu.AddSimpleButton("Open User in Browser", OpenUserInBrowser);

            UserInfoExtensionsMod.menu.AddLabel("Website Related Things");
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSimpleButton("Open User in Browser", OpenUserInBrowser);
            UserInfoExtensionsMod.menu.AddSpacer();
            UserInfoExtensionsMod.menu.AddSpacer();
        }

        public static void OpenUserInBrowser()
        {
            UserInfoExtensionsMod.HideAllPopups();

            System.Diagnostics.Process.Start("https://vrchat.com/home/user/" + Utilities.ActiveUser.id);
            Utilities.OpenPopupV2("Notice:", "User has been opened in the default browser", "Close", new Action(() => Utilities.ClosePopup()));
        }
    }
}
