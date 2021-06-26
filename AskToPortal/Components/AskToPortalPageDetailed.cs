using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using UnityEngine.UI;
using VRC.Core;

namespace AskToPortal.Components
{
    class AskToPortalPageDetailed : AskToPortalPromptPage
    {
        private Text instanceIdText;
        private Text instanceTypeText;
        private Text instanceRegionText;
        private Text instanceCreatorIdText;
        private Text worldNameText;
        private Text worldAuthorText;
        private Text instancePopText;

        private Text dropperNameText;
        private Text dropperIdText;
        private Text isFriendText;
        private Text isSelfText;
        private Text statusText;

        private Text errorReasonText;

        public unsafe AskToPortalPageDetailed(IntPtr obj0) : base(obj0)
        {
        }
        
        internal override void Init()
        {
            base.Init();

            field_Public_String_1 = "AskToPortal Detailed";

            instanceIdText = transform.Find("InstanceInformation/Viewport/Content/InstanceId").GetComponent<Text>();
            instanceTypeText = transform.Find("InstanceInformation/Viewport/Content/InstanceType").GetComponent<Text>();
            instanceRegionText = transform.Find("InstanceInformation/Viewport/Content/InstanceRegion").GetComponent<Text>();
            instanceCreatorIdText = transform.Find("InstanceInformation/Viewport/Content/InstanceCreatorId").GetComponent<Text>();
            worldNameText = transform.Find("InstanceInformation/Viewport/Content/WorldName").GetComponent<Text>();
            worldAuthorText = transform.Find("InstanceInformation/Viewport/Content/WorldAuthor").GetComponent<Text>();
            instancePopText = transform.Find("InstanceInformation/Viewport/Content/InstancePop").GetComponent<Text>();

            dropperNameText = transform.Find("DropperInformation/Viewport/Content/DropperName").GetComponent<Text>();
            dropperIdText = transform.Find("DropperInformation/Viewport/Content/DropperId").GetComponent<Text>();
            isFriendText = transform.Find("DropperInformation/Viewport/Content/IsFriend").GetComponent<Text>();
            isSelfText = transform.Find("DropperInformation/Viewport/Content/IsSelf").GetComponent<Text>();
            statusText = transform.Find("DropperInformation/Viewport/Content/Status").GetComponent<Text>();

            errorReasonText = transform.Find("PortalDiscrepancies/Viewport/Content/ErrorReason").GetComponent<Text>();
        }

        [method: HideFromIl2Cpp]
        public override void Setup(PortalInternal portal, RoomInfo roomInfo, APIUser dropper, string worldId, string roomId)
        {
            base.Setup(portal, roomInfo, dropper, worldId, roomId);
            
            field_Public_String_1 = "AskToPortal Detailed";

            instanceIdText.text = "Instance Id: " + roomInfo.instanceId;
            instanceTypeText.text = "Instance Type: " + roomInfo.instanceType;
            instanceRegionText.text = "Instance Region: " + roomInfo.region;
            instanceCreatorIdText.text = "Instance Creator: " + roomInfo.ownerId;
            worldNameText.text = "World Name: " + portal.field_Private_ApiWorld_0.name;
            worldAuthorText.text = "World Author: " + portal.field_Private_ApiWorld_0.authorName;
            instancePopText.text = "Instance Pop: " + portal.field_Private_Int32_0.ToString();

            dropperNameText.text = "Dropper Name: " + dropper.displayName;
            dropperIdText.text = "Dropper Id: " + dropper.id;
            isFriendText.text = "Is Friend: " + APIUser.IsFriendsWith(dropper.id).ToString();
            isSelfText.text = "Is Self: " + dropper.IsSelf.ToString();
            statusText.text = "Status: " + dropper.status;

            errorReasonText.text = string.Join("\n", roomInfo.errors);
        }
    }
}
