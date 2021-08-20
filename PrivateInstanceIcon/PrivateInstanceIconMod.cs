using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;

[assembly: MelonInfo(typeof(PrivateInstanceIcon.PrivateInstanceIconMod), "PrivateInstanceIcon", "1.1.0", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace PrivateInstanceIcon
{
	public enum InstanceBehavior
	{
		ShowIcon,
		Default,
		HideUsers
	}
    public class PrivateInstanceIconMod : MelonMod
    {
        private static PropertyInfo listEnum;
        private static PropertyInfo pickerPrefabProp;
		private static Sprite lockIconSprite, openLockSprite, friendsSprite, friendsPlusSprite, globeSprite;

		public static MelonPreferences_Entry<InstanceBehavior> privateInstanceBehavior, joinablePrivateInstanceBehavior, friendsInstanceBehavior, friendsPlusInstanceBehavior, publicInstanceBehavior;
        public static MelonPreferences_Entry<bool> includeFavoritesList;
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
                HarmonyInstance.Patch(method, postfix: typeof(PrivateInstanceIconMod).GetMethod(nameof(OnSetPickerContentFromApiModel), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod(), finalizer: typeof(PrivateInstanceIconMod).GetMethod(nameof(OnSetPickerContentFromApiModelErrored), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());
            HarmonyInstance.Patch(typeof(UiUserList).GetMethod("Awake"), postfix: typeof(PrivateInstanceIconMod).GetMethod(nameof(OnUiUserListAwake), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod());

			MelonPreferences_Category category = MelonPreferences.CreateCategory("PrivateInstanceIcon Config");
			includeFavoritesList = category.CreateEntry(nameof(includeFavoritesList), true, "Whether to include the icons and hiding in the friends favorites list.");

			privateInstanceBehavior = category.CreateEntry("Private Instances", InstanceBehavior.ShowIcon,
						"How the list should behave for private instances");
			joinablePrivateInstanceBehavior = category.CreateEntry("Joinable Private Instances", InstanceBehavior.ShowIcon,
						"How the list should behave for joinable private instances");
			friendsInstanceBehavior = category.CreateEntry("Friends Instances", InstanceBehavior.Default,
						"How the list should behave for friends instances");
			friendsPlusInstanceBehavior = category.CreateEntry("Friends+ Instances", InstanceBehavior.Default,
						"How the list should behave for friends+ instances");
			publicInstanceBehavior = category.CreateEntry("Public Instances", InstanceBehavior.Default,
						"How the list should behave for public instances");
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

		private static void OnUiUserListAwake(UiUserList __instance)
        {
            int enumValue = (int)listEnum.GetValue(__instance);
            if (includeFavoritesList.Value && enumValue != 3 && enumValue != 7)
                return;
            else if (!includeFavoritesList.Value && enumValue != 3)
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

		private static void ProcessInstanceBehavior(UiUserList userList, GameObject userButton, GameObject icon, InstanceBehavior state, Sprite sprite)
		{
			string text = userList.field_Public_Text_0.text;
			text = text.Split(new string[] { " [" }, StringSplitOptions.None)[0];
			if (state == InstanceBehavior.HideUsers)
			{
				MelonCoroutines.Start(SetInactiveCoroutine(userButton));
				int hiddenCount = 0;
				foreach (VRCUiContentButton picker in userList.pickers)
					if (!picker.gameObject.active)
						hiddenCount++;
				userList.field_Public_Text_0.text = text + $" [{hiddenCount} hidden]";
			}
			else if (state == InstanceBehavior.ShowIcon)
			{
				icon.GetComponent<Image>().sprite = sprite;
				icon.SetActive(true);
			}
			else icon.SetActive(false);

			userList.field_Public_Text_0.text = text;
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
			if (user.location == "private")
			{
				if (__0.field_Public_UiStatusIcon_0.field_Public_UserStatus_0 == APIUser.UserStatus.JoinMe)
					ProcessInstanceBehavior(__instance, __0.gameObject, icon, joinablePrivateInstanceBehavior.Value, openLockSprite);
				else ProcessInstanceBehavior(__instance, __0.gameObject, icon, privateInstanceBehavior.Value, lockIconSprite);
			}
			else if (user.location.Contains("~friends("))
				ProcessInstanceBehavior(__instance, __0.gameObject, icon, friendsInstanceBehavior.Value, friendsSprite);
			else if (user.location.Contains("~hidden("))
				ProcessInstanceBehavior(__instance, __0.gameObject, icon, friendsPlusInstanceBehavior.Value, friendsPlusSprite);
			else if (user.location.StartsWith("wrld_"))
				ProcessInstanceBehavior(__instance, __0.gameObject, icon, publicInstanceBehavior.Value, friendsPlusSprite);
			else
				icon.SetActive(false);
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
