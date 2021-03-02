using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(CloningBeGone.CloningBeGoneMod), "CloningBeGone", "1.0.1", "loukylor (https://github.com/loukylor/CloningBeGone)")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace CloningBeGone
{
    class CloningBeGoneMod : MelonMod
    {
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex == -1) GameObject.Find("UserInterface/MenuContent/Screens/Settings/OtherOptionsPanel/AllowAvatarCopyingToggle").GetComponent<UiSettingConfig>().SetEnable(false);
        }
    }
}
