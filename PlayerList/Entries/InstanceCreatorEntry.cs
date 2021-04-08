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
            ResetEntry();

            string creator = instance.GetInstanceCreator();

            if (creator != null)
            { 
                if (creator == APIUser.CurrentUser.id)
                {
                    ChangeEntry("instancecreator", APIUser.CurrentUser.displayName);
                    yield break;
                }

                yield return new WaitForSeconds(4);

                APIUser.FetchUser(creator, new Action<APIUser>(OnIdReceived), null);
                ChangeEntry("instancecreator", "Loading...");
                yield break;
            }
            else
            {
                ResetEntry();
                ChangeEntry("instancecreator", "No Instance Creator");
            }
        }
        public void OnIdReceived(APIUser user)
        {
            MelonLoader.MelonLogger.Msg("awefjas;lekf");
            ChangeEntry("instancecreator", user.displayName);
        }
    }
}
