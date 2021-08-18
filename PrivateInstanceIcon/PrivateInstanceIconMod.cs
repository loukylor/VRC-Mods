using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;

[assembly: MelonInfo(typeof(PrivateInstanceIcon.PrivateInstanceIconMod), "PrivateInstanceIcon", "1.0.1", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace PrivateInstanceIcon
{
    public class PrivateInstanceIconMod : MelonMod
    {
        private static PropertyInfo listEnum;
        private static PropertyInfo pickerPrefabProp;
        private static Sprite iconSprite;

        public static MelonPreferences_Entry<bool> excludeJoinMe;
        public static MelonPreferences_Entry<bool> hidePrivateInstances;
        public static MelonPreferences_Entry<bool> includeFavoritesList;
        public override void OnApplicationStart()
        {
            listEnum = typeof(UiUserList).GetProperties().First(pi => pi.Name.StartsWith("field_Public_Enum"));
            pickerPrefabProp = typeof(UiUserList).GetProperties().First(pi => pi.PropertyType == typeof(GameObject));

            Texture2D iconTex = new Texture2D(2, 2);
            using (Stream iconStream = Assembly.GetManifestResourceStream("PrivateInstanceIcon.icon.png"))
            {
                var buffer = new byte[iconStream.Length];
                iconStream.Read(buffer, 0, buffer.Length);
                ImageConversion.LoadImage(iconTex, buffer);
            }

            Rect rect = new Rect(0, 0, iconTex.width, iconTex.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Vector4 border = Vector4.zero;
            iconSprite = Sprite.CreateSprite_Injected(iconTex, ref rect, ref pivot, 50, 0, SpriteMeshType.Tight, ref border, false);
            iconSprite.hideFlags = HideFlags.DontUnloadUnusedAsset;

            foreach (MethodInfo method in typeof(UiUserList).GetMethods().Where(mi => mi.Name.StartsWith("Method_Protected_Virtual_Void_VRCUiContentButton_Object_")))
                HarmonyInstance.Patch(method, postfix: typeof(PrivateInstanceIconMod).GetMethod(nameof(OnSetPickerContentFromApiModel), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod(), finalizer: typeof(PrivateInstanceIconMod).GetMethod(nameof(OnSetPickerContentFromApiModelErrored), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());
            HarmonyInstance.Patch(typeof(UiUserList).GetMethod("Awake"), postfix: typeof(PrivateInstanceIconMod).GetMethod(nameof(OnUiUserListAwake), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());

            MelonPreferences_Category category = MelonPreferences.CreateCategory("PrivateInstanceIcon Config");
            excludeJoinMe = category.CreateEntry(nameof(excludeJoinMe), true, "Whether to hide the icon when people are on join me, and in private instances.");
            hidePrivateInstances = category.CreateEntry(nameof(hidePrivateInstances), false, "Whether to just not show people who are in private instances.");
            includeFavoritesList = category.CreateEntry(nameof(includeFavoritesList), true, "Whether to include the icon and hiding in the friends favorites list.");
        }

        private static void OnUiUserListAwake(UiUserList __instance)
        {
            int enumValue = (int)listEnum.GetValue(__instance);
            if (includeFavoritesList.Value && enumValue != 3 && enumValue != 7)
                return;
            else if (!includeFavoritesList.Value && enumValue != 3)
                return;

            GameObject pickerPrefab = (GameObject)pickerPrefabProp.GetValue(__instance);
            VRCUiContentButton picker = pickerPrefab.GetComponent<VRCUiContentButton>();

            GameObject[] newArr = new GameObject[picker.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0.Length + 1];
            picker.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0.CopyTo(newArr, 0);

            GameObject icon = GameObject.Instantiate(picker.transform.Find("Icons/OverlayIcons/iconUserOnPC").gameObject);
            icon.name = "PrivateInstanceIcon";
            icon.transform.SetParent(picker.transform.Find("Icons/OverlayIcons"));
            icon.GetComponent<Image>().sprite = iconSprite;
            icon.SetActive(false);

            newArr[newArr.Length - 1] = icon;

            picker.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0 = newArr;
        }

        private static void OnSetPickerContentFromApiModel(UiUserList __instance, VRCUiContentButton __0, Il2CppSystem.Object __1)
        {
            int enumValue = (int)listEnum.GetValue(__instance);
            if (includeFavoritesList.Value && enumValue != 3 && enumValue != 7)
                return;
            else if (!includeFavoritesList.Value && enumValue != 3)
                return;

            APIUser user = __1.TryCast<APIUser>();
            if (user == null)
                return;

            GameObject icon = __0.field_Public_VRCUiDynamicOverlayIcons_0.field_Public_ArrayOf_GameObject_0.First(gameObject => gameObject.name == "PrivateInstanceIcon");
            if (user.location == "private" && !(excludeJoinMe.Value && __0.field_Public_UiStatusIcon_0.field_Public_UserStatus_0 == APIUser.UserStatus.JoinMe))
            {
                string text = __instance.field_Public_Text_0.text;
                text = text.Split(new string[] { " [" }, StringSplitOptions.None)[0];
                if (hidePrivateInstances.Value)
                {
                    MelonCoroutines.Start(SetInactiveCoroutine(__0.gameObject));
                    int hiddenCount = 0;
                    foreach (VRCUiContentButton picker in __instance.pickers)
                        if (!picker.gameObject.active)
                            hiddenCount++;
                    __instance.field_Public_Text_0.text = text + $" [{hiddenCount} hidden]";
                }
                else
                {
                    icon.SetActive(true);
                    __instance.field_Public_Text_0.text = text;
                }
            }
            else
            { 
                icon.SetActive(false);
            }
        }

        private static IEnumerator SetInactiveCoroutine(GameObject go)
        {
            // This was necessary. fuck you unity
            go.SetActive(false);
            yield return null;
            go.SetActive(false);
        }

        private static Exception OnSetPickerContentFromApiModelErrored(Exception __exception)
        {
            // There's actually no better way to do this https://github.com/knah/Il2CppAssemblyUnhollower/blob/master/UnhollowerBaseLib/Il2CppException.cs
            if (__exception is NullReferenceException || __exception.Message.Contains("System.NullReferenceException"))
                return null;
            else
                return __exception;
        }
    }
}
