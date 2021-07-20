using System;
using MelonLoader;
using UIExpansionKit.API;
using UIExpansionKit.Components;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRChatUtilityKit.Ui;

namespace UserInfoExtensions.Modules
{
    class AvatarInformation : ModuleBase
    {
        public static MelonPreferences_Entry<bool> AvatarInfoMenuButton;
        private static GameObject avatarInfoMenuButtonGameObject;

        public static ICustomShowableLayoutedMenu avatarInfoMenu;

        private static Text authorNameLabel;
        private static Text avatarNameLabel;
        private static Text platformLabel;
        private static Text releaseTypeLabel;
        private static Text lastUpdatedLabel;
        private static Text VersionLabel;

        private static ApiAvatar avatar;

        public override void Init()
        {
            AvatarInfoMenuButton = MelonPreferences.CreateEntry("UserInfoExtensionsSettings", nameof(AvatarInfoMenuButton), true, "Show \"Avatar Info Menu\" button in Avatar Menu");

            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.AvatarMenu).AddSimpleButton("Avatar Info Menu", new Action(() => { avatarInfoMenu?.Show(); OnMenuShown(); }), new Action<GameObject>((go) =>
            {
                avatarInfoMenuButtonGameObject = go;
                GetAvatarAuthor.avatarPage.GetComponent<EnableDisableListener>().OnDisabled += new Action(() => avatarInfoMenu?.Hide());
            }));

            avatarInfoMenu = ExpansionKitApi.CreateCustomFullMenuPopup(new LayoutDescription()
            {
                RowHeight = 80,
                NumColumns = 3,
                NumRows = 5
            });
            avatarInfoMenu.AddLabel("Avatar information:");
            avatarInfoMenu.AddSpacer();
            avatarInfoMenu.AddSimpleButton("Back", new Action(() => avatarInfoMenu.Hide()));
            avatarInfoMenu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => authorNameLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            avatarInfoMenu.AddSimpleButton("Show Avatar Description", new Action(() => { avatarInfoMenu.Hide(); UiManager.OpenSmallPopup("Description:", avatar.description == null ? "" : avatar.description, "Close", UiManager.ClosePopup); }));
            avatarInfoMenu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => avatarNameLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            avatarInfoMenu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => platformLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            avatarInfoMenu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => releaseTypeLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            avatarInfoMenu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => lastUpdatedLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
            avatarInfoMenu.AddSimpleButton("", new Action(() => { }), new Action<GameObject>((gameObject) => VersionLabel = gameObject.transform.GetChild(0).GetComponent<Text>()));
        }

        private static void OnMenuShown()
        {
            avatar = GetAvatarAuthor.avatarPage.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0;

            if (avatar == null)
            {
                authorNameLabel.text = "Author Name:\nUnknown";
                avatarNameLabel.text = "Avatar Name:\nUnknown";
                platformLabel.text = "Platform:\nUnknown";
                releaseTypeLabel.text = "Release Type:\nUnknown";
                lastUpdatedLabel.text = "Last Updated At:\nUnknown";
                VersionLabel.text = "Version:\nUnknown";
            }
            else
            {
                if (string.IsNullOrEmpty(avatar.authorName))
                    authorNameLabel.text = "Author Name:\nUnknown";
                else
                    authorNameLabel.text = $"Author Name:\n{avatar.authorName}";

                if (string.IsNullOrEmpty(avatar.name))
                    avatarNameLabel.text = "Avatar Name:\nUnknown";
                else
                    avatarNameLabel.text = $"Avatar Name:\n{avatar.name}";

                string supportedPlatforms = avatar.supportedPlatforms.ToString();
                switch (supportedPlatforms)
                {
                    case "StandaloneWindows":
                        supportedPlatforms = "PC";
                        break;
                    case "Android":
                        supportedPlatforms = "Quest";
                        break;
                }
                platformLabel.text = "Platform:\n" + supportedPlatforms;

                if (string.IsNullOrEmpty(avatar.releaseStatus))
                    releaseTypeLabel.text = "Release Type:\nUnknown";
                else
                    releaseTypeLabel.text = "Release Type:\n" + char.ToUpper(avatar.releaseStatus[0]) + avatar.releaseStatus.Substring(1);

                if (avatar.updated_at == null)
                {
                    lastUpdatedLabel.text = "Last Updated At:\nUnknown";
                }
                else
                {
                    if (UserInformation.militaryTimeFormat.Value)
                        lastUpdatedLabel.text = "Last Updated At:\n" + avatar.updated_at.ToString("M/d/yyyy HH:mm");
                    else
                        lastUpdatedLabel.text = "Last Updated At:\n" + avatar.updated_at.ToString("M/d/yyyy hh:mm tt");
                }
                 
                if (avatar.version < 1)
                    VersionLabel.text = "Version:\nUnknown";
                else
                    VersionLabel.text = $"Version:\n{avatar.version}";
            }
        }

        public override void OnPreferencesSaved()
        {
            avatarInfoMenuButtonGameObject?.SetActive(AvatarInfoMenuButton.Value);
        }
    }
}
