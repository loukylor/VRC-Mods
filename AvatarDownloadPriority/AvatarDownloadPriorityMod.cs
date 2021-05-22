using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using MelonLoader;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;

[assembly: MelonInfo(typeof(AvatarDownloadPriority.AvatarDownloadPriorityMod), "AvatarPriorityDownloading", "1.0.2", "loukylor", "https://github.com/loukylor/ButtonMover")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace AvatarDownloadPriority
{
    public class AvatarDownloadPriorityMod : MelonMod
    {
        public static readonly List<AvatarProcess> avatarDownloadQueue = new List<AvatarProcess>();
        public static readonly List<AvatarProcess> avatarLoadQueue = new List<AvatarProcess>();

        internal static Comparison<AvatarProcess> sortComparison;
        internal static readonly Comparison<AvatarProcess> prioritizeFavoriteFriendsSortComparison = (lProcess, rProcess) =>
        {
            int result = prioritizeFriendsSortComparison(lProcess, rProcess);
            if (result == 0)
            {
                bool lFavorite = APIUser.CurrentUser._favoriteFriendIdsInGroup.Any(list => list.Contains(lProcess.Id));
                bool rFavorite = APIUser.CurrentUser._favoriteFriendIdsInGroup.Any(list => list.Contains(rProcess.Id));
                if (!lFavorite && rFavorite)
                    return 1;
                else if (lFavorite == rFavorite)
                    return 0;
                else
                    return -1;
            }
            return result;
        };
        internal static readonly Comparison<AvatarProcess> prioritizeFriendsSortComparison = (lProcess, rProcess) =>
        {
            bool lFriend = APIUser.IsFriendsWith(lProcess.Id);
            bool rFriend = APIUser.IsFriendsWith(rProcess.Id);
            if (!lFriend && rFriend)
                return 1;
            else if (lFriend == rFriend)
                return 0;
            else
                return -1;
        };

        private static MethodInfo downloadAvatarMethod;
        private static AvatarProcess currentDownload = null;
        private static readonly List<AvatarProcess> currentlyDownloadingAvatars = new List<AvatarProcess>();
        private static bool shouldDownloadNextAvatar = false;

        private static MethodInfo startCoroutineMethod;
        private static readonly List<AvatarProcess> currentlyLoadingAvatars = new List<AvatarProcess>();
        public override void OnApplicationStart()
        {
            Config.Init();

            Harmony.Patch(typeof(NetworkManager).GetMethod("OnLeftRoom"), new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnLeftRoom))));
            Harmony.Patch(typeof(VRCAvatarManager).GetMethod("OnDestroy"), new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnDestroy))));

            downloadAvatarMethod = typeof(VRCAvatarManager).GetMethods().First(mb => mb.Name.StartsWith("Method_Private_Void_ApiAvatar_Single_MulticastDelegateNPublic")
                && mb.GetParameters().Count() == 4 && mb.GetParameters()[3].ParameterType == typeof(ApiAvatar));
            Harmony.Patch(downloadAvatarMethod, new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnAvatarDownload))));
            foreach (XrefInstance instance in XrefScanner.XrefScan(downloadAvatarMethod))
            {
                if (instance.Type == XrefType.Method && instance.TryResolve() != null && instance.TryResolve().Name.Contains("ApiAvatar"))
                {
                    Harmony.Patch(instance.TryResolve(), new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnAssetBundleDownload))));
                    break;
                }
            }

            MethodInfo attachAvatarEnumeratorMethod = typeof(VRCAvatarManager).GetMethods().First(mb =>
                mb.GetParameters().Length == 7
                && mb.GetParameters()[0].ParameterType == typeof(UnityEngine.Object)
                && mb.GetParameters()[1].ParameterType == typeof(string));
            Harmony.Patch(attachAvatarEnumeratorMethod, new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnAttachAvatarEnumerator))));
            MethodInfo loadAvatarMethod = typeof(VRCAvatarManager).GetMethods().First(mb => mb.Name.StartsWith("Method_Private_IEnumerator_ApiAvatar_AssetBundleDownload_Action_1_GameObject_Action_"));
            Harmony.Patch(loadAvatarMethod, null, new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnAvatarLoad))));
            startCoroutineMethod = typeof(MonoBehaviour).GetMethod("StartCoroutine", new Type[1] { typeof(Il2CppSystem.Collections.IEnumerator) });
            Harmony.Patch(typeof(MonoBehaviour).GetMethod("StopCoroutine", new Type[1] { typeof(Coroutine) }), new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnCoroutineStop))));
        }
        public override void OnUpdate()
        {
            // Here just as a fail safe. I haven't seen it actually run in testing
            if (currentlyDownloadingAvatars.Count == 0 && avatarDownloadQueue.Count > 0)
                DownloadNextAvatar();

            if (currentlyLoadingAvatars.Count == 0 && avatarLoadQueue.Count > 0)
                LoadNextAvatar();

            if (Input.GetKeyDown(KeyCode.B))
            {
                MelonLogger.Msg("current download");
                foreach (AvatarProcess process in currentlyDownloadingAvatars)
                    MelonLogger.Msg(process.DisplayName);

                MelonLogger.Msg("download queue");
                foreach (AvatarProcess process in avatarDownloadQueue)
                    MelonLogger.Msg(process.DisplayName);

                MelonLogger.Msg("current load");
                foreach (AvatarProcess process in currentlyLoadingAvatars)
                    MelonLogger.Msg(process.DisplayName);

                MelonLogger.Msg("loading queuue");
                foreach (AvatarProcess process in avatarLoadQueue)
                    MelonLogger.Msg(process.DisplayName);
            }    
        }

        public static void OnLeftRoom()
        {
            avatarDownloadQueue.Clear();
            currentlyDownloadingAvatars.Clear();
            currentDownload = null;
            shouldDownloadNextAvatar = false;

            avatarLoadQueue.Clear();
            currentlyLoadingAvatars.Clear();
        }

        public static void OnDestroy(VRCAvatarManager __instance) // If someone leaves
        {
            currentlyLoadingAvatars.Remove(new AvatarProcess { manager = __instance });
            avatarLoadQueue.Remove(new AvatarProcess { manager = __instance });
            currentlyDownloadingAvatars.Remove(new AvatarProcess { manager = __instance });
            avatarDownloadQueue.Remove(new AvatarProcess { manager = __instance });
        }

        public static bool OnAvatarDownload(VRCAvatarManager __instance, ApiAvatar __0, float __1, Il2CppSystem.Action<GameObject, VRC_AvatarDescriptor, bool> __2, ApiAvatar __3) // When an avatar download has been requested
        {
            if (shouldDownloadNextAvatar)
            {
                return true;
            }
            else
            {
                // This is a bit slow but in the long run its like 0.1ms so idc
                AvatarProcess download = new AvatarProcess(__instance, new object[4] { __0, __1, __2, __3 });
                avatarDownloadQueue.Remove(download); // Prevents duplicates because why download something that's already gonna be replaced
                avatarDownloadQueue.Add(download);

                if (sortComparison != null)
                    avatarDownloadQueue.Sort(sortComparison);
                if (Config.prioritizeSelf.Value)
                {
                    download = avatarDownloadQueue.FirstOrDefault(searchDownload => searchDownload.Id == APIUser.CurrentUser.id);
                    if (download != null)
                    {
                        avatarDownloadQueue.Remove(download);
                        avatarDownloadQueue.Insert(0, download);
                    }
                }

                if (currentlyDownloadingAvatars.Count <= Config.maxDownloadingAvatarsAtOnce.Value - 1)
                    DownloadNextAvatar();
                return false;
            }
        }
        public static void OnAssetBundleDownload(ref object __2, ref object __3) // This will call when OnAvatarDownload calls (inside the method body of it)
        {
            if (shouldDownloadNextAvatar)
            {
                __2 = ((Il2CppSystem.Delegate)__2).CombineImpl((Il2CppSystem.Action<AssetBundleDownload>)new Action<AssetBundleDownload>((assetBundle) => OnDownloadStop(new AvatarProcess() { manager = currentDownload.manager })));
                __3 = ((Il2CppSystem.Delegate)__3).CombineImpl((Il2CppSystem.Action<string, string, LoadErrorReason>)new Action<string, string, LoadErrorReason>((string1, string2, errorReason) => OnDownloadStop(new AvatarProcess() { manager = currentDownload.manager })));
            }
        }

        public static void OnDownloadStop(VRCAvatarManager manager) 
        {
            int index = currentlyDownloadingAvatars.IndexOf(new AvatarProcess() { manager = manager });
            if (index >= 0)
            {
                currentlyDownloadingAvatars.RemoveAt(index);
                if (avatarDownloadQueue.Count > 0)
                    DownloadNextAvatar();
            }
        }
        public static void OnDownloadStop(AvatarProcess download)
        {
            currentlyDownloadingAvatars.Remove(download);
            if (avatarDownloadQueue.Count > 0)
                DownloadNextAvatar();
        }

        public static void DownloadNextAvatar()
        {
            currentDownload = avatarDownloadQueue[0];
            currentlyDownloadingAvatars.Remove(currentDownload);
            currentlyDownloadingAvatars.Add(currentDownload);
            avatarDownloadQueue.RemoveAt(0);
            shouldDownloadNextAvatar = true;
            downloadAvatarMethod.Invoke(currentDownload.manager, currentDownload.methodParams);
        }

        public static void OnAvatarLoad(VRCAvatarManager __instance, ref object __result)
        {
            AvatarProcess avatarLoad = new AvatarProcess(__instance, new object[1] { __result });
            __result = null;

            OnDownloadStop(avatarLoad);
            avatarLoadQueue.Remove(avatarLoad);
            avatarLoadQueue.Add(avatarLoad);

            if (sortComparison != null)
                avatarLoadQueue.Sort(sortComparison);
            if (Config.prioritizeSelf.Value)
            {
                AvatarProcess selfAvatarLoadProcess = avatarLoadQueue.FirstOrDefault(searchLoad => searchLoad.Id == APIUser.CurrentUser.id);
                if (selfAvatarLoadProcess != null)
                {
                    avatarLoadQueue.Remove(selfAvatarLoadProcess);
                    avatarLoadQueue.Insert(0, selfAvatarLoadProcess);
                }
            }

            if (currentlyLoadingAvatars.Count <= Config.maxLoadingAvatarsAtOnce.Value - 1)
                LoadNextAvatar();
        }
        public static void OnAttachAvatarEnumerator(VRCAvatarManager __instance, ref Il2CppSystem.Action<GameObject> __5, ref Il2CppSystem.Action __6) // Calls when the gameobject has been created and avatar processing has started
        {
            __5 = __5.CombineImpl((Il2CppSystem.Action<GameObject>)new Action<GameObject>((gameObject) => OnLoadStop(__instance))).Cast<Il2CppSystem.Action<GameObject>>(); // onSuccess
            __6 = __6.CombineImpl((Il2CppSystem.Action)new Action(() => OnLoadStop(__instance))).Cast<Il2CppSystem.Action>(); // onError
        }
        public static void OnCoroutineStop(MonoBehaviour __instance) // If the coroutine is canceled and someone is turned into any prefab avatar
        {
            VRCAvatarManager manager = __instance.TryCast<VRCAvatarManager>();
            if (manager == null)
                return;

            OnLoadStop(manager);
        }

        public static void OnLoadStop(VRCAvatarManager manager)
        {
            int index = currentlyLoadingAvatars.IndexOf(new AvatarProcess() { manager = manager });
            if (index >= 0)
            {
                currentlyLoadingAvatars.RemoveAt(index);
                if (avatarLoadQueue.Count > 0)
                    LoadNextAvatar();
            }
        }
        public static void LoadNextAvatar()
        {
            AvatarProcess currentLoadingAvatar = avatarLoadQueue[0];
            currentlyLoadingAvatars.Remove(currentLoadingAvatar);
            currentlyLoadingAvatars.Add(currentLoadingAvatar);
            avatarLoadQueue.RemoveAt(0);
            startCoroutineMethod.Invoke(currentLoadingAvatar.manager, currentLoadingAvatar.methodParams);
        }
    }
}
