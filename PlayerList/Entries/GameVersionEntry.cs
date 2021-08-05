using System;
using Il2CppSystem.Reflection;
using MelonLoader;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using VRC.Core;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class GameVersionEntry : EntryBase
    {
        public GameVersionEntry(IntPtr obj0) : base(obj0) { }

        [HideFromIl2Cpp]
        public override string Name => "Game Version";

        [HideFromIl2Cpp]
        public override void Init(object[] parameters = null)
        {
            Il2CppArrayBase<MonoBehaviour> components = GameObject.Find("_Application/ApplicationSetup").GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                if (component.TryCast<Transform>() != null || component.TryCast<ApiCache>() != null)
                    continue;

                foreach (FieldInfo field in component.GetIl2CppType().GetFields())
                {
                    if (field.FieldType == UnhollowerRuntimeLib.Il2CppType.Of<int>())
                    {
                        textComponent.text = OriginalText.Replace("{gameversion}", field.GetValue(component).Unbox<int>().ToString());
                        return;
                    }
                }
            }
        }
    }
}
