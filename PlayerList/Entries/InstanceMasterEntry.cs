using System;
using System.Collections;
using MelonLoader;
using Photon.Realtime;
using VRC.Core;
using VRChatUtilityKit.Utilities;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class InstanceMasterEntry : EntryBase
    {
        public InstanceMasterEntry(IntPtr obj0) : base(obj0) { }

        public override string Name { get { return "Instance Master"; } }

        public override void Init(object[] parameters = null)
        {
            NetworkEvents.OnPlayerJoined += OnPlayerJoined;
            NetworkEvents.OnMasterChanged += OnMasterChanged;
        }
        public void OnPlayerJoined(VRC.Player player)
        {
            // This will handle getting the master on instance join
            if (player.prop_VRCPlayerApi_0 != null && player.prop_VRCPlayerApi_0.isMaster)
            {
                if (APIUser.CurrentUser != null && player.prop_APIUser_0.id == APIUser.CurrentUser.id && LocalPlayerEntry.emmNameSpoofEnabled)
                    textComponent.text = OriginalText.Replace("{instancemaster}", LocalPlayerEntry.emmSpoofedName);
                else
                    textComponent.text = OriginalText.Replace("{instancemaster}", player.prop_APIUser_0.displayName);
            }
        }

        private void OnMasterChanged(Player player)
        {
            MelonCoroutines.Start(GetOnMasterChanged(player));
        }
        private IEnumerator GetOnMasterChanged(Player player)
        {
            while (player.field_Public_Player_0 == null)
                yield return null;

            if (APIUser.CurrentUser != null && player.field_Public_Player_0.prop_APIUser_0.id == APIUser.CurrentUser.id && LocalPlayerEntry.emmNameSpoofEnabled)
                textComponent.text = OriginalText.Replace("{instancemaster}", LocalPlayerEntry.emmSpoofedName);
            else
                textComponent.text = OriginalText.Replace("{instancemaster}", player.field_Public_Player_0.prop_APIUser_0.displayName);
            yield break;
        }
    }
}
