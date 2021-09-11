using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;

[assembly: MelonInfo(typeof(PrivateInstanceIcon.PrivateInstanceIconMod), "PrivateInstanceIcon", "1.1.1", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace PrivateInstanceIcon
{
    public class PrivateInstanceIconMod : MelonMod
    {
        private static PropertyInfo listEnum;
        private static PropertyInfo pickerPrefabProp;
        private static Sprite lockIconSprite, openLockSprite, friendsSprite, friendsPlusSprite, globeSprite;
        private static readonly Dictionary<int, string> listTitleTable = new Dictionary<int, string>();

        public override void OnApplicationStart()
        {
            listEnum = typeof(UiUserList).GetProperties().First(pi => pi.Name.StartsWith("field_Public_Enum"));
            pickerPrefabProp = typeof(UiUserList).GetProperties().First(pi => pi.PropertyType == typeof(GameObject));

            lockIconSprite = LoadSprite("PrivateInstanceIcon.lock.png");
            openLockSprite = LoadSprite("PrivateInstanceIcon.lock-open.png");
            friendsSprite = LoadSprite("PrivateInstanceIcon.friend.png");
            friendsPlusSprite = LoadSprite("PrivateInstanceIcon.friends.png");
            globeSprite = LoadSprite("PrivateInstanceIcon.globe.png");

            foreach (MethodInfo method in typeof(UiUserList).GetMethods().Where(mi => mi.Name.StartsWith("Method_Protected_Virtual_Void_VRCUiContentButton_Object_")))
                HarmonyInstance.Patch(method, typeof(PrivateInstanceIconMod).GetMethod(nameof(OnSetPickerContentFromApiModelPrefix), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod(), typeof(PrivateInstanceIconMod).GetMethod(nameof(OnSetPickerContentFromApiModel), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());//, finalizer: typeof(PrivateInstanceIconMod).GetMethod(nameof(OnSetPickerContentFromApiModelErrored), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());
            HarmonyInstance.Patch(typeof(UiUserList).GetMethod("Awake"), postfix: typeof(PrivateInstanceIconMod).GetMethod(nameof(OnUiUserListAwake), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());

            foreach (MethodInfo method in typeof(UiUserList).GetMethods().Where(mb => mb.Name.StartsWith("Method_Private_IEnumerator_List_1_APIUser_Int32_Boolean_")))
                HarmonyInstance.Patch(method, typeof(PrivateInstanceIconMod).GetMethod(nameof(OnRenderList), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());

            Config.Init();
        }

        private Sprite LoadSprite(string manifestString)
        {
            Texture2D texture = new Texture2D(2, 2);
            using (Stream iconStream = Assembly.GetManifestResourceStream(manifestString))
            {
                var buffer = new byte[iconStream.Length];
                iconStream.Read(buffer, 0, buffer.Length);
                ImageConversion.LoadImage(texture, buffer);
            }

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Vector4 border = Vector4.zero;
            Sprite sprite = Sprite.CreateSprite_Injected(texture, ref rect, ref pivot, 50, 0, SpriteMeshType.Tight, ref border, false);
            sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }

        private static void OnRenderList(UiUserList __instance, Il2CppSystem.Collections.Generic.List<APIUser> __0)
        {
            if (__0.Count == 0)
                return;

            if (!Config.isAnyTypeHidden || !ShouldAdjustList(__instance))
            {
                if (listTitleTable.TryGetValue(__instance.GetInstanceID(), out string pastText))
                {
                    string text = __instance.field_Public_Text_0.text;

                    int indexOfLastText = text.LastIndexOf(pastText);
                    if (indexOfLastText != -1)
                        text = text.Remove(indexOfLastText, pastText.Length);

                    __instance.field_Public_Text_0.text = text;
                }
            }
            else
            {
                int hiddenCount = 0;
                int shownCount = 0;
                for (int i = __0.Count - 1; i >= 0; i--)
                {
                    if (ShouldHideUser(__0[i]))
                    {
                        hiddenCount++;
                        __0.RemoveAt(i);
                    }
                    else
                    {
                        shownCount++;
                    }
                }

                string text = __instance.field_Public_Text_0.text;
                string hiddenText = $" [{hiddenCount} hidden, {shownCount} shown]";

                if (!listTitleTable.TryGetValue(__instance.GetInstanceID(), out string pastText))
                {
                    listTitleTable.Add(__instance.GetInstanceID(), hiddenText);
                }
                else
                {
                    int indexOfLastText = text.LastIndexOf(pastText);
                    if (indexOfLastText != -1)
                        text = text.Remove(indexOfLastText, pastText.Length);
                }

                __instance.field_Public_Text_0.text = text + hiddenText;
                listTitleTable[__instance.GetInstanceID()] = hiddenText;
            }
        }

        private static void OnUiUserListAwake(UiUserList __instance)
        {
            if (!ShouldAdjustList(__instance))
                return;

            GameObject pickerPrefab = (GameObject)pickerPrefabProp.GetValue(__instance);
            VRCUiContentButton picker = pickerPrefab.GetComponent<VRCUiContentButton>();

            if (picker.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0.Any(iconCheck => iconCheck.name == "PrivateInstanceIcon"))
                return;

            GameObject[] newArr = new GameObject[picker.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0.Length + 1];
            picker.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0.CopyTo(newArr, 0);

            GameObject icon = GameObject.Instantiate(picker.transform.Find("Icons/OverlayIcons/iconUserOnPC").gameObject);
            icon.name = "PrivateInstanceIcon";
            icon.transform.SetParent(picker.transform.Find("Icons/OverlayIcons"));
            icon.SetActive(false);

            newArr[newArr.Length - 1] = icon;

            picker.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0 = newArr;
        }

        private static void OnSetPickerContentFromApiModelPrefix(VRCUiContentButton __0)
        {
            // For some reason, our icon just dies for whatever reason sometimes
            for (int i = 0; i < __0.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0.Count; i++)
            {
                if (__0.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0[i] == null)
                {
                    GameObject icon = GameObject.Instantiate(__0.transform.Find("Icons/OverlayIcons/iconUserOnPC").gameObject);
                    icon.name = "PrivateInstanceIcon";
                    icon.transform.SetParent(__0.transform.Find("Icons/OverlayIcons"));
                    icon.SetActive(false);
                    icon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                    __0.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0[i] = icon;
                    return;
                }
            }
        }

        private static void OnSetPickerContentFromApiModel(UiUserList __instance, VRCUiContentButton __0, Il2CppSystem.Object __1)
        {
            if (!ShouldAdjustList(__instance))
                return;

            APIUser user = __1.TryCast<APIUser>();
            if (user == null)
                return;

            GameObject icon = __0.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0.First(gameObject => gameObject.name == "PrivateInstanceIcon");

            if (UserInstanceSprite(user) is Sprite sprite)
            {
                icon.GetComponent<Image>().sprite = sprite;
                icon.SetActive(true);
            }
            else
                icon.SetActive(false);
        }

        private static bool ShouldHideUser(APIUser user)
        {
            // user.location == "private" && !(excludeJoinMe.Value && user.statusValue == APIUser.UserStatus.JoinMe);
            if (user.location == "private")
            {
                if (user.statusValue == APIUser.UserStatus.JoinMe) return Config.JoinablePrivateInstanceBehavior == InstanceBehavior.HideUsers;
                else return Config.PrivateInstanceBehavior == InstanceBehavior.HideUsers;
            }
            else if (user.location.Contains("~friends(")) return Config.FriendsInstanceBehavior == InstanceBehavior.HideUsers;
            else if (user.location.Contains("~hidden(")) return Config.FriendsPlusInstanceBehavior == InstanceBehavior.HideUsers;
            else if (user.location.StartsWith("wrld_")) return Config.PublicInstanceBehavior == InstanceBehavior.HideUsers;

            // For offline users for example, or any other unknown states.
            return false;
        }

        /// <summary>
        /// Returns a sprite if it should be shown for the user.
        /// </summary>
        /// <returns>The sprite to show, or null if shouldn't be shown.</returns>
        private static Sprite UserInstanceSprite(APIUser user)
        {
            if (user.location == "private")
            {
                if (user.statusValue == APIUser.UserStatus.JoinMe && Config.JoinablePrivateInstanceBehavior == InstanceBehavior.ShowIcon) return openLockSprite;
                else if (user.statusValue != APIUser.UserStatus.JoinMe && Config.PrivateInstanceBehavior == InstanceBehavior.ShowIcon) return lockIconSprite;
            }
            else if (user.location.Contains("~friends("))
            {
                if (Config.FriendsInstanceBehavior == InstanceBehavior.ShowIcon) return friendsSprite;
            }
            else if (user.location.Contains("~hidden("))
            {
                if (Config.FriendsPlusInstanceBehavior == InstanceBehavior.ShowIcon) return friendsPlusSprite;
            }
            else if (user.location.StartsWith("wrld_"))
            {
                if (Config.PublicInstanceBehavior == InstanceBehavior.ShowIcon) return globeSprite;
            }

            return null;
        }

        private static bool ShouldAdjustList(UiUserList list)
        {
            int enumValue = (int)listEnum.GetValue(list);
            if (Config.IncludeFavoritesList && enumValue != 3 && enumValue != 7)
                return false;
            else if (!Config.IncludeFavoritesList && enumValue != 3)
                return false;
            return true;
        }
    }
}
