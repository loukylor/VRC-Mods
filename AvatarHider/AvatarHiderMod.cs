using System.Diagnostics;
using AvatarHider.DataTypes;
using AvatarHider.Utilities;
using MelonLoader;

[assembly: MelonInfo(typeof(AvatarHider.AvatarHiderMod), "AvatarHider", "1.2.0", "loukylor and ImTiara", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

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
            PlayerProp.Init();
            PlayerManager.Init();
            RefreshManager.Init();
            Config.OnConfigChange();
        }
        public override void VRChat_OnUiManagerInit()
        {
            NetworkEvents.NetworkInit();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != -1) return;

            PlayerManager.OnSceneWasLoaded();

        }
        public override void OnUpdate()
        {
            // About 50-100 microseconds (0.05 - 0.1 milliseconds) per refresh in instance of ~20 people;
            timer.Restart();
            RefreshManager.Refresh();
            MelonLogger.Msg(((float)timer.ElapsedTicks / (Stopwatch.Frequency / 1000)).ToString("N5"));
        }
    }
}