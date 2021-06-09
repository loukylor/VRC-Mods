using System.Collections;
using System.Linq;
using InstanceHistory.UI;
using InstanceHistory.Utilities;
using MelonLoader;

[assembly: MelonInfo(typeof(InstanceHistory.InstanceHistoryMod), "InstanceHistory", "1.0.2", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies(new string[1] { "UIExpansionKit" })]

namespace InstanceHistory
{
    public class InstanceHistoryMod : MelonMod
    {
        internal static bool HasUIX { get { return MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit")); } }
        public static InstanceHistoryMod Instance { get; private set; }
        public override void OnApplicationStart()
        {
            Instance = this;
            Config.Init();
            VRCUtils.Init();
            UIManager.Init();
            WorldManager.Init();
            InstanceManager.Init();

            if (HasUIX)
                typeof(UIXManager).GetMethod("AddMethodToUIInit").Invoke(null, null);
            else
                MelonCoroutines.Start(StartUiManagerInitIEnumerator());
        }

        private IEnumerator StartUiManagerInitIEnumerator()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null)
                yield return null;

            OnUiManagerInit();
        }

        public void OnUiManagerInit()
        {
            MenuManager.UiInit();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex == -1)
                UIManager.OnSceneWasLoaded();
        }
    }
}
