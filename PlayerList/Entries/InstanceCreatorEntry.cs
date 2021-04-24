using System;
using System.Collections;
using VRC.Core;
using UnityEngine;
using MelonLoader;

namespace PlayerList.Entries
{
    class InstanceCreatorEntry : EntryBase
    {
        public override string Name { get { return "Instance Creator"; } }

        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            MelonCoroutines.Start(GetInstanceCreator(instance));
        }
        public IEnumerator GetInstanceCreator(ApiWorldInstance instance)
        {
            string creator = instance.GetInstanceCreator();

            if (creator != null)
            { 
                if (creator == APIUser.CurrentUser.id)
                {
                    textComponent.text = OriginalText.Replace("{instancecreator}", APIUser.CurrentUser.displayName);
                    yield break;
                }

                textComponent.text = OriginalText.Replace("{instancecreator}", "Loading...");

                yield return new WaitForSeconds(4);

                APIUser.FetchUser(creator, new Action<APIUser>(OnIdReceived), null);
                yield break;
            }
            else
            {
                textComponent.text = OriginalText.Replace("{instancecreator}", "No Instance Creator");
            }
        }
        public void OnIdReceived(APIUser user)
        {
            textComponent.text = OriginalText.Replace("{instancecreator}", user.displayName);
        }
    }
}
