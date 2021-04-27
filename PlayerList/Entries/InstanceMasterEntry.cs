using System.Collections;
using MelonLoader;
using Photon.Realtime;
using PlayerList.Utilities;
using VRC.Core;

namespace PlayerList.Entries
{
    class InstanceMasterEntry : EntryBase
    {
        public override string Name { get { return "Instance Master"; } }

        private object currentCoroutine;

        public override void Init(object[] parameters = null)
        {
            NetworkEvents.OnMasterChanged += OnMasterChanged;
        }
        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            if (currentCoroutine != null)
                MelonCoroutines.Stop(currentCoroutine);
            currentCoroutine = MelonCoroutines.Start(GetMasterOnInstanceChange());
        }
        public IEnumerator GetMasterOnInstanceChange()
        {
            while (true)
            {
                try
                {
                    foreach (VRC.Player player in VRC.PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
                    { 
                        if (player.prop_VRCPlayerApi_0.isMaster)
                        { 
                            textComponent.text = OriginalText.Replace("{instancemaster}", player.field_Private_APIUser_0.displayName);
                            yield break;
                        }
                    }
                }
                catch { }
                yield return null;
            }
        }
        private void OnMasterChanged(Player player)
        {
            textComponent.text = OriginalText.Replace("{instancemaster}", player.field_Public_Player_0.field_Private_APIUser_0.displayName);
        }
    }
}
