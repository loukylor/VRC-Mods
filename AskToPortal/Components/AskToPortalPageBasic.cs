using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using UnityEngine.UI;
using VRC.Core;

namespace AskToPortal.Components
{
    class AskToPortalPageBasic : AskToPortalPromptPage
    {
        private RawImage avatarImage;
        private Text messageText;

        public unsafe AskToPortalPageBasic(IntPtr obj0) : base(obj0)
        {
        }

        internal override void Init()
        {
            base.Init();

            field_Public_String_1 = "AskToPortal Basic";

            avatarImage = transform.Find("MessagePanel/AvatarThumbnail").GetComponent<RawImage>();
            messageText = transform.Find("MessagePanel/Message").GetComponent<Text>();
        }

        [method: HideFromIl2Cpp]
        public override void Setup(PortalInternal portal, RoomInfo roomInfo, APIUser dropper, string worldId, string roomId)
        {
            base.Setup(portal, roomInfo, dropper, worldId, roomId);

            field_Public_String_1 = "AskToPortal Basic";

            if (AskToPortalMod.cachedDroppers.ContainsKey(portal.GetInstanceID()))
            {
                try
                {
                    avatarImage.texture = AskToPortalMod.cachedDroppers[portal.GetInstanceID()].prop_VRCPlayer_0.field_Private_Texture2D_1;
                    if (avatarImage.texture == null)
                        avatarImage.texture = AskToPortalMod.cachedDroppers[portal.GetInstanceID()].prop_VRCPlayer_0.field_Private_Texture2D_0;
                }
                catch (NullReferenceException)
                {

                }
            }
            else
            {
                avatarImage.texture = null;
            }

            if (dropper.id != "")
                messageText.text = $"{dropper.displayName} has dropped a portal to {portal.field_Private_ApiWorld_0.name} #{roomInfo.instanceId} {roomInfo.instanceType}.";
            else
                messageText.text = $"This is a world portal to {portal.field_Private_ApiWorld_0.name}.";
            
            switch (roomInfo.errors.Count)
            {
                case 0:
                    messageText.text += "\nThere are no detected errors with the portal, so it's destination should be safe.";
                    break;
                case 1:
                    messageText.text += "\nThere is one error with the portal, so it's destination will most likely be safe.";
                    break;
                case 2:
                    messageText.text += "\nThere are multiple errors with the portal. You probably should leave this one alone";
                    break;
                default:
                    messageText.text += "\nThere are a lot of errors with this portal. You really can only get this high when deliberately messing with the portal. Leave this one alone.";
                    break;
            }

            if (dropper.id != "")
                messageText.text += "\nDo you wish to enter, leave, or blacklist this dropper so you cant enter their porals until you restart?";
            else
                messageText.text += "\nDo you wish to enter or leave?";
        }
    }
}
