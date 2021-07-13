using System;
using System.Collections;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using VRC.Core;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class InstanceCreatorEntry : EntryBase
    {
        public InstanceCreatorEntry(IntPtr obj0) : base(obj0) { }

        [HideFromIl2Cpp]
        public override string Name => "Instance Creator";

        public override void OnInstanceChange(ApiWorld world, ApiWorldInstance instance)
        {
            MelonCoroutines.Start(GetInstanceCreator(instance));
        }
        [HideFromIl2Cpp]
        public IEnumerator GetInstanceCreator(ApiWorldInstance instance)
        {
            string creator = instance.ownerId;

            if (creator != null)
            { 
                if (creator == APIUser.CurrentUser? .id)
                {
                    if (LocalPlayerEntry.emmNameSpoofEnabled)
                        textComponent.text = OriginalText.Replace("{instancecreator}", LocalPlayerEntry.emmSpoofedName);
                    else
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
