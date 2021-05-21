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

[assembly: MelonInfo(typeof(AvatarDownloadPriority.AvatarDownloadPriorityMod), "AvatarPriorityDownloading", "1.0.1", "loukylor", "https://github.com/loukylor/ButtonMover")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace AvatarDownloadPriority
{
    public class AvatarDownloadPriorityMod : MelonMod
    {
        public static readonly List<AvatarDownload> avatarDownloadQueue = new List<AvatarDownload>();

        internal static Comparison<AvatarDownload> sortComparison;
        internal static readonly Comparison<AvatarDownload> prioritizeFavoriteFriendsSortComparison = (lDownload, rDownload) =>
        {
            int result = prioritizeFriendsSortComparison(lDownload, rDownload);
            if (result == 0)
            {
                bool lFavorite = APIUser.CurrentUser._favoriteFriendIdsInGroup.Any(list => list.Contains(lDownload.Id));
                bool rFavorite = APIUser.CurrentUser._favoriteFriendIdsInGroup.Any(list => list.Contains(rDownload.Id));
                if (!lFavorite && rFavorite)
                    return 1;
                else if (lFavorite == rFavorite)
                    return 0;
                else
                    return -1;
            }
            return result;
        };
        internal static readonly Comparison<AvatarDownload> prioritizeFriendsSortComparison = (lDownload, rDownload) =>
        {
            bool lFriend = APIUser.IsFriendsWith(lDownload.Id);
            bool rFriend = APIUser.IsFriendsWith(rDownload.Id);
            if (!lFriend && rFriend)
                return 1;
            else if (lFriend == rFriend)
                return 0;
            else
                return -1;
        };

        private static MethodInfo downloadAvatarMethod;

        private static AvatarDownload currentDownload = null;
        private static readonly List<AvatarDownload> currentlyDownloadingAvatars = new List<AvatarDownload>();
        private static bool downloadNextAvatar = false;
        public override void OnApplicationStart()
        {
            Config.Init();

            downloadAvatarMethod = typeof(VRCAvatarManager).GetMethods().First(mb => mb.Name.StartsWith("Method_Private_Void_ApiAvatar_Single_MulticastDelegateNPublic")
                && mb.GetParameters().Count() == 4 && mb.GetParameters()[3].ParameterType == typeof(ApiAvatar));
            foreach (XrefInstance instance in XrefScanner.XrefScan(downloadAvatarMethod))
            {
                if (instance.Type == XrefType.Method && instance.TryResolve() != null && instance.TryResolve().Name.Contains("ApiAvatar"))
                {
                    Harmony.Patch(instance.TryResolve(), new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnAssetBundleDownload))));
                    break;
                }
            }

            Harmony.Patch(typeof(MonoBehaviour).GetMethod("StopCoroutine", new Type[1] { typeof(Coroutine) }), new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnCoroutineStop))));

            MethodInfo attachAvatarEnumeratorMethod = typeof(VRCAvatarManager).GetMethods().First(mb =>
                mb.GetParameters().Length == 7
                && mb.GetParameters()[0].ParameterType == typeof(UnityEngine.Object)
                && mb.GetParameters()[1].ParameterType == typeof(string));
            Harmony.Patch(attachAvatarEnumeratorMethod, new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnAttachAvatarEnumerator))));
            Harmony.Patch(typeof(VRCAvatarManager).GetMethod("OnDestroy"), new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnDestroy))));
            Harmony.Patch(downloadAvatarMethod, new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnAvatarDownload))));
            Harmony.Patch(typeof(NetworkManager).GetMethod("OnLeftRoom"), new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnLeftRoom))));
        }
        public override void OnUpdate()
        {
            // Here just as a fail safe. I haven't seen it actually run in testing
            if (currentlyDownloadingAvatars.Count == 0 && avatarDownloadQueue.Count > 0)
                DownloadNextAvatar();
        }

        public static void OnLeftRoom()
        {
            avatarDownloadQueue.Clear();
            currentlyDownloadingAvatars.Clear();
            currentDownload = null;
            downloadNextAvatar = false;
        }
        public static bool OnAvatarDownload(VRCAvatarManager __instance, ApiAvatar __0, float __1, Il2CppSystem.Action<GameObject, VRC_AvatarDescriptor, bool> __2, ApiAvatar __3) // When an avatar download has been requested
        {
            if (downloadNextAvatar)
            {
                return true;
            }
            else
            {
                // This is a bit slow but in the long run its like 0.1ms so idc
                AvatarDownload download = new AvatarDownload(__instance, new object[4] { __0, __1, __2, __3 });
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
        public static void OnAssetBundleDownload(ref object __3) // This will call when OnAvatarDownload calls (inside the method body of it)
        {
            if (downloadNextAvatar)
            {
                downloadNextAvatar = false;
                // Only patching onError as OnAttachAvatarEnumerator will handle when it succeeds
                __3 = ((Il2CppSystem.Delegate)__3).CombineImpl((Il2CppSystem.Action<string, string, LoadErrorReason>)new Action<string, string, LoadErrorReason>((string1, string2, errorReason) => OnDownloadStop(new AvatarDownload() { manager = currentDownload.manager })));
            }
        }
        public static void OnAttachAvatarEnumerator(VRCAvatarManager __instance, ref Il2CppSystem.Action<GameObject> __5, ref Il2CppSystem.Action __6) // Calls when the gameobject has been created and avatar processing has started
        {
            __5 = __5.CombineImpl((Il2CppSystem.Action<GameObject>)new Action<GameObject>((gameObject) => OnDownloadStop(__instance))).Cast<Il2CppSystem.Action<GameObject>>(); // onSuccess
            __6 = __6.CombineImpl((Il2CppSystem.Action)new Action(() => OnDownloadStop(__instance))).Cast<Il2CppSystem.Action>(); // onError
        }
        public static void OnCoroutineStop(MonoBehaviour __instance) // If the coroutine is canceled and someone is turned into any prefab avatar
        {
            VRCAvatarManager manager = __instance.TryCast<VRCAvatarManager>();
            if (manager == null)
                return;

            OnDownloadStop(manager);
        }
        public static void OnDestroy(VRCAvatarManager __instance) // If someone leaves
        {
            OnDownloadStop(__instance);
        }

        public static void OnDownloadStop(VRCAvatarManager manager) 
        {
            int index = currentlyDownloadingAvatars.IndexOf(new AvatarDownload() { manager = manager });
            if (index >= 0)
            {
                currentlyDownloadingAvatars.RemoveAt(index);
                if (avatarDownloadQueue.Count > 0)
                    DownloadNextAvatar();
            }
        }
        public static void OnDownloadStop(AvatarDownload download)
        {
            currentlyDownloadingAvatars.Remove(download);
            if (avatarDownloadQueue.Count > 0)
                DownloadNextAvatar();
        }

        public static void DownloadNextAvatar()
        {
            currentDownload = avatarDownloadQueue[0];
            currentlyDownloadingAvatars.Add(currentDownload);
            avatarDownloadQueue.RemoveAt(0);
            downloadNextAvatar = true;
            downloadAvatarMethod.Invoke(currentDownload.manager, currentDownload.downloadMethodParams);
        }
    }
}
