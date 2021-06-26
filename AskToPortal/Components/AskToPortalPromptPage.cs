using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRChatUtilityKit.Ui;

namespace AskToPortal.Components
{
    class AskToPortalPromptPage : VRCUiPage
    {
        protected Button leaveButton;
        protected Button enterButton;
        protected Button blackListButton;

        protected Text titleText;

        protected PortalInternal portal;
        protected APIUser dropper;
        protected string worldId;
        protected string roomId;

        public unsafe AskToPortalPromptPage(IntPtr obj0) : base(obj0)
        {
        }

        internal virtual void Init()
        {
            field_Public_String_0 = "SCREEN";
            field_Protected_CanvasGroup_0 = GetComponent<CanvasGroup>();
            leaveButton = transform.Find("Footer/LeaveButton").GetComponent<Button>();
            enterButton = transform.Find("Footer/EnterButton").GetComponent<Button>();
            blackListButton = transform.Find("Footer/BlackListButton").GetComponent<Button>();
            titleText = transform.Find("TitlePanel/TitleText").GetComponent<Text>();

            leaveButton.onClick = new Button.ButtonClickedEvent();
            leaveButton.onClick.AddListener(new Action(() => UiManager.CloseBigMenu()));

            enterButton.onClick = new Button.ButtonClickedEvent();
            enterButton.onClick.AddListener(new Action(() => PortalUtils.EnterPortal(portal, dropper, worldId, roomId)));

            blackListButton.onClick = new Button.ButtonClickedEvent();
            blackListButton.onClick.AddListener(new Action(() => 
            {
                if (dropper.id == "")
                {
                    UiManager.OpenAlertPopup("Cant blacklist this user");
                }
                else
                {
                    AskToPortalMod.blacklistedUserIds.Add(dropper.id);
                    UiManager.CloseBigMenu();
                }
            }));
        }

        [method: HideFromIl2Cpp]
        public virtual void Setup(PortalInternal portal, RoomInfo roomInfo, APIUser dropper, string worldId, string roomId)
        {
            field_Public_String_0 = "SCREEN";
            this.portal = portal;
            this.dropper = dropper;
            this.worldId = worldId;
            this.roomId = roomId;
        }
    }
}
