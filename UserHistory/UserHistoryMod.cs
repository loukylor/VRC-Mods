using MelonLoader;
using VRChatUtilityKit.Utilities;

[assembly: MelonInfo(typeof(UserHistory.UserHistoryMod), "UserHistory", "1.0.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonAdditionalDependencies("UIExpansionKit")]

namespace UserHistory
{
    public class UserHistoryMod : MelonMod
    {
        public static UserHistoryMod Instance { get; private set; }
        public override void OnApplicationStart()
        {
            Instance = this;
            Config.Init();
            UserManager.Init();

            VRCUtils.OnUiManagerInit += OnUiManagerInit;
        }

        public void OnUiManagerInit()
        {
            MenuManager.UiInit();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex != -1)
                return;

            UserManager.cachedPlayers.Clear();
        }
    }
}
