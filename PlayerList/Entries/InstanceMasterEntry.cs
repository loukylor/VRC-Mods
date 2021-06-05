using System.Collections;
using MelonLoader;
using Photon.Realtime;
using PlayerList.Utilities;

namespace PlayerList.Entries
{
    class InstanceMasterEntry : EntryBase
    {
        public override string Name { get { return "Instance Master"; } }

        public override void Init(object[] parameters = null)
        {
            NetworkEvents.OnPlayerJoined += OnPlayerJoined;
            NetworkEvents.OnMasterChanged += OnMasterChanged;
        }
        public void OnPlayerJoined(VRC.Player player)
        {
            // This will handle getting the master on instance join
            if (player.prop_VRCPlayerApi_0.isMaster)
                textComponent.text = OriginalText.Replace("{instancemaster}", player.prop_APIUser_0.displayName);
        }

        private void OnMasterChanged(Player player)
        {
            MelonCoroutines.Start(GetOnMasterChanged(player));
        }
        private IEnumerator GetOnMasterChanged(Player player)
        {
            while (player.field_Public_Player_0 == null)
                yield return null;

            textComponent.text = OriginalText.Replace("{instancemaster}", player.field_Public_Player_0.prop_APIUser_0.displayName);
            yield break;
        }
    }
}
