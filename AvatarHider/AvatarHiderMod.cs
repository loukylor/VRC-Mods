using System.Diagnostics;
using AvatarHider.DataTypes;
using MelonLoader;

[assembly: MelonInfo(typeof(AvatarHider.AvatarHiderMod), "AvatarHider", "1.2.6", "loukylor and ImTiara", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace AvatarHider
{
    public class AvatarHiderMod : MelonMod
    {
        public static AvatarHiderMod Instance { get; private set; }

        public static readonly Stopwatch timer = Stopwatch.StartNew();

        public override void OnApplicationStart()
        {
            Instance = this;
            Config.RegisterSettings();
            OnPreferencesSaved();
            AvatarHiderPlayer.Init();
            PlayerManager.Init();
            RefreshManager.Init();
            Config.OnConfigChange();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != -1) return;

            PlayerManager.OnSceneWasLoaded();

        }
        public override void OnUpdate()
        {
            // About 50-100 microseconds (0.05 - 0.1 milliseconds) per refresh in instance of ~20 people;
            RefreshManager.Refresh();
        }
    }
}