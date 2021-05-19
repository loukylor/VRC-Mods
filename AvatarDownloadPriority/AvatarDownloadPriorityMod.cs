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

[assembly: MelonInfo(typeof(AvatarPriorityDownloading.AvatarDownloadPriorityMod), "AvatarPriorityDownloading", "1.0.0", "loukylor", "https://github.com/loukylor/ButtonMover")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace AvatarPriorityDownloading
{
    public class AvatarDownloadPriorityMod : MelonMod
    {
        public static readonly List<AvatarDownload> avatarDownloadQueue = new List<AvatarDownload>();
        public static MelonPreferences_Entry<int> maxDownloadingAvatarsAtOnce;
        public static MelonPreferences_Entry<bool> prioritizeSelf;
        public static MelonPreferences_Entry<bool> prioritizeFavoriteFriends;
        public static MelonPreferences_Entry<bool> prioritizeFriends;

        private static Comparison<AvatarDownload> sortComparison;
        private static readonly Comparison<AvatarDownload> prioritizeFavoriteFriendsSortComparison = (lDownload, rDownload) =>
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
        private static readonly Comparison<AvatarDownload> prioritizeFriendsSortComparison = (lDownload, rDownload) =>
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
        private static bool downloadNextAvatar = false;
        private static int currentlyDownloadingAvatars;
        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("AvatarPriorityDownloadingConfig", "AvatarPriorityDownloading Config");
            maxDownloadingAvatarsAtOnce = (MelonPreferences_Entry<int>)MelonPreferences.CreateEntry("AvatarPriorityDownloadingConfig", nameof(maxDownloadingAvatarsAtOnce), 5, "Max number of avatars downloading at once");
            maxDownloadingAvatarsAtOnce.OnValueChanged += OnMaxDownloadingAvatarsAtOnceChange;
            prioritizeSelf = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("AvatarPriorityDownloadingConfig", nameof(prioritizeSelf), true, "Prioritize downloading your own avatar");
            prioritizeSelf.OnValueChanged += OnSortConfigChange;
            prioritizeFavoriteFriends = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("AvatarPriorityDownloadingConfig", nameof(prioritizeFavoriteFriends), true, "Prioritize downloading favorite friends' avatars");
            prioritizeFavoriteFriends.OnValueChanged += OnSortConfigChange;
            prioritizeFriends = (MelonPreferences_Entry<bool>)MelonPreferences.CreateEntry("AvatarPriorityDownloadingConfig", nameof(prioritizeFriends), true, "Prioritize downloading friends' avatars");
            prioritizeFriends.OnValueChanged += OnSortConfigChange;
            OnSortConfigChange(true, false);

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
            Harmony.Patch(downloadAvatarMethod, new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnAvatarDownload))));
            Harmony.Patch(typeof(NetworkManager).GetMethod("OnLeftRoom"), new HarmonyMethod(typeof(AvatarDownloadPriorityMod).GetMethod(nameof(OnLeftRoom))));
        }
        public override void OnUpdate()
        {
            if (currentlyDownloadingAvatars == 0 && avatarDownloadQueue.Count > 0)
                DownloadNextAvatar();
        }

        public static void OnMaxDownloadingAvatarsAtOnceChange(int oldValue, int newValue)
        {
            if (oldValue == newValue)
                return;

            if (maxDownloadingAvatarsAtOnce.Value <= 0)
                maxDownloadingAvatarsAtOnce.Value = 1;
        }
        public static void OnSortConfigChange(bool oldValue, bool newValue)
        {
            if (oldValue == newValue)
                return;

            sortComparison = null;
            if (prioritizeFavoriteFriends.Value)
                sortComparison = prioritizeFavoriteFriendsSortComparison;
            else if (prioritizeFriends.Value)
                sortComparison = prioritizeFriendsSortComparison;
        }

        public static void OnLeftRoom()
        {
            avatarDownloadQueue.Clear();
            currentlyDownloadingAvatars = 0;
            currentDownload = null;
            downloadNextAvatar = false;
        }
        public static bool OnAvatarDownload(VRCAvatarManager __instance, ApiAvatar __0, float __1, Il2CppSystem.Action<GameObject, VRC_AvatarDescriptor, bool> __2, ApiAvatar __3)
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
                if (prioritizeSelf.Value)
                {
                    download = avatarDownloadQueue.FirstOrDefault(searchDownload => searchDownload.Id == APIUser.CurrentUser.id);
                    if (download != null)
                    {
                        avatarDownloadQueue.Remove(download);
                        avatarDownloadQueue.Insert(0, download);
                    }
                }

                if (currentlyDownloadingAvatars <= maxDownloadingAvatarsAtOnce.Value - 1)
                    DownloadNextAvatar();
                return false;
            }
        }
        public static bool OnAssetBundleDownload(ref object __2, ref object __3) // This will call when OnAvatarDownload calls
        {
            if (downloadNextAvatar)
            {
                downloadNextAvatar = false;
                __2 = ((Il2CppSystem.Delegate)__2).CombineImpl((Il2CppSystem.Action<AssetBundleDownload>)new Action<AssetBundleDownload>((bundle) => OnDownloadStop()));
                __3 = ((Il2CppSystem.Delegate)__3).CombineImpl((Il2CppSystem.Action<string, string, LoadErrorReason>)new Action<string, string, LoadErrorReason>((string1, string2, errorReason) => OnDownloadStop()));
            }

            return true;
        }
        public static void OnDownloadStop() 
        {
            currentlyDownloadingAvatars--;
            if (avatarDownloadQueue.Count > 0)
                DownloadNextAvatar();   
        }

        public static void DownloadNextAvatar()
        {
            currentlyDownloadingAvatars++;
            currentDownload = avatarDownloadQueue[0];
            avatarDownloadQueue.RemoveAt(0);
            downloadNextAvatar = true;
            downloadAvatarMethod.Invoke(currentDownload.manager, currentDownload.downloadMethodParams);
        }

        public class AvatarDownload
        {
            public VRCAvatarManager manager;
            public object[] downloadMethodParams;
            public string Id => manager.field_Private_VRCPlayer_0.prop_Player_0.prop_APIUser_0.id;

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                AvatarDownload objAsDownload = obj as AvatarDownload;
                if (objAsDownload == null)
                    return false;
                else
                    return Equals(objAsDownload);
            }
            public bool Equals(AvatarDownload download)
            {
                if (download == null)
                    return false;
                return download.Id == Id;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public AvatarDownload(VRCAvatarManager manager, object[] downloadMethodParams)
            {
                this.manager = manager;
                this.downloadMethodParams = downloadMethodParams;
            }
        }
    }
}
