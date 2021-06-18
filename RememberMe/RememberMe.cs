using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(RememberMe.RememberMe), RememberMe.BuildInfo.Name, RememberMe.BuildInfo.Version, RememberMe.BuildInfo.Author, RememberMe.BuildInfo.DownloadLink)]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace RememberMe
{
    public static class BuildInfo
    {
        public const string Name = "RememberMe";
        public const string Author = "Herp Derpinstine & dave-kun";
        public const string Company = "Lava Gang";
        public const string Version = "1.065";
        public const string DownloadLink = "https://github.com/HerpDerpinstine/RememberMe";
    }

    public class RememberMe : MelonMod
    {
        private static MethodInfo VRCUiManager_Instance_get = null;
        private static VRCUiPageAuthentication PageAuthentication = null;
        private static MethodInfo ValidateTextTargetMethod;
        private static string SecurePlayerPrefsPassword = "vl9u1grTnvXA";
        private static string ToggleKey = "RememberMe_Toggle";
        private static string UserKey = "RememberMe_User";
        private static string PassKey = "RememberMe_Pass";

        public static RememberMe Instance { get; private set; }
        public static bool HasUIX => MelonHandler.Mods.Any(x => x.Info.Name.Equals("UI Expansion Kit"));

        public override void OnApplicationStart()
        {
            Instance = this;

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
            VRCUiPageAuthentication authPage = GetAuthPage();

            Button doneButton = authPage.transform.Find("ButtonDone (1)").GetComponent<Button>();

            Button.ButtonClickedEvent doneButton_onClick = doneButton.onClick;
            doneButton.onClick = new Button.ButtonClickedEvent();
            doneButton.onClick.AddListener(new Action(() =>
            {
                SavePlayerPrefs();
                doneButton_onClick.Invoke();
            }));

            GameObject.Find("UserInterface/MenuContent/Screens/Authentication/Login2FA/BoxLogin2FA/ButtonBack").GetComponent<Button>().onClick.AddListener(new Action(LoadPlayerPrefs));

            Transform popupcheckboxtrans = GameObject.Find("UserInterface/MenuContent/Popups/PerformanceSettingsPopup").GetComponent<PopupPerformanceOptions>().dynamicBoneOptionsPage.transform.Find("Checkboxes/LimitDynamicBoneUsage");

            GameObject newcheckbox = GameObject.Instantiate(popupcheckboxtrans.gameObject, authPage.field_Public_UiInputField_0.transform.parent);
            newcheckbox.name = "RememberMe";
            GameObject.Destroy(newcheckbox.transform.GetChild(2).gameObject);
            GameObject.Destroy(newcheckbox.transform.GetChild(3).gameObject);
            GameObject.Destroy(newcheckbox.transform.GetChild(4).gameObject);
            GameObject.Destroy(newcheckbox.transform.GetChild(5).gameObject);

            Transform newcheckbox_desc_trans = newcheckbox.transform.Find("Description");
            newcheckbox_desc_trans.GetComponent<Text>().text = "Remember Me";

            Toggle newcheckbox_toggle = newcheckbox.GetComponent<Toggle>();
            newcheckbox_toggle.isOn = SecurePlayerPrefs.HasKey(ToggleKey);
            newcheckbox_toggle.onValueChanged = new Toggle.ToggleEvent();
            newcheckbox_toggle.onValueChanged.AddListener(new Action<bool>((newvalue) =>
            {
                if (!newvalue)
                    SecurePlayerPrefs.DeleteKey(ToggleKey);
                else
                    SecurePlayerPrefs.SetString(ToggleKey, "true", SecurePlayerPrefsPassword);
            }));

            newcheckbox.transform.localPosition = new Vector3((authPage.field_Public_UiInputField_1.transform.localPosition.x - 130), (authPage.field_Public_UiInputField_1.transform.localPosition.y - 45), authPage.field_Public_UiInputField_1.transform.localPosition.z);
            authPage.field_Public_UiInputField_0.transform.localPosition = new Vector3(authPage.field_Public_UiInputField_0.transform.localPosition.x, (authPage.field_Public_UiInputField_0.transform.localPosition.y + 30), authPage.field_Public_UiInputField_0.transform.localPosition.z);
            authPage.field_Public_UiInputField_1.transform.localPosition = new Vector3(authPage.field_Public_UiInputField_1.transform.localPosition.x, (authPage.field_Public_UiInputField_1.transform.localPosition.y + 30), authPage.field_Public_UiInputField_1.transform.localPosition.z);

            ValidateTextTargetMethod = typeof(InputFieldValidator).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Single(it => it.GetParameters().Length == 1 && XrefScanner.XrefScan(it).Any(jt => jt.Type == XrefType.Global && jt.ReadAsObject()?.ToString() == "^([\\w\\.\\-\\+]+)@([\\w\\-]+)((\\.(\\w){2,3})+)$"));

            LoadPlayerPrefs();

            VRCUiManager uiManager = GetVRCUiManager();
            uiManager.field_Private_Action_1_VRCUiPage_0 = (
                (uiManager.field_Private_Action_1_VRCUiPage_0 == null)
                ? new Action<VRCUiPage>(OnPageShown)
                : Il2CppSystem.Delegate.Combine(uiManager.field_Private_Action_1_VRCUiPage_0, (Il2CppSystem.Action<VRCUiPage>)new Action<VRCUiPage>(OnPageShown)).Cast<Il2CppSystem.Action<VRCUiPage>>()
                );
        }

        private static void OnPageShown(VRCUiPage page)
        {
            if (page == null)
                return;
            if (page.name.Equals("StoreLoginPrompt") || page.name.Equals("LoginUserPass") || page.name.Equals("Login2FA"))
                LoadPlayerPrefs();
            else if (page.name.Equals("StandardPopup"))
            {
                VRCUiPopupStandard popupStandard = page.TryCast<VRCUiPopupStandard>();
                if (popupStandard == null)
                    return;
                if (popupStandard.field_Public_Text_0.text.Equals("LOGIN"))
                    SavePlayerPrefs();
            }
        }

        private static void LoadPlayerPrefs()
        {
            VRCUiPageAuthentication authPage = GetAuthPage();
            if (authPage == null)
                return;
            if (SecurePlayerPrefs.HasKey(ToggleKey))
            {
                if (SecurePlayerPrefs.HasKey(UserKey))
                {
                    authPage.field_Public_UiInputField_0.field_Private_String_0 = SecurePlayerPrefs.GetString(UserKey, SecurePlayerPrefsPassword);
                    authPage.field_Public_UiInputField_0.prop_String_0 = authPage.field_Public_UiInputField_0.field_Private_String_0;
                    ValidateTextTargetMethod.Invoke(authPage.field_Public_UiInputField_0.GetComponent<InputFieldValidator>(), new object[] { authPage.field_Public_UiInputField_0.field_Private_String_0 });
                }
                if (SecurePlayerPrefs.HasKey(PassKey))
                {
                    authPage.field_Public_UiInputField_1.field_Private_String_0 = SecurePlayerPrefs.GetString(PassKey, SecurePlayerPrefsPassword);
                    authPage.field_Public_UiInputField_1.prop_String_0 = authPage.field_Public_UiInputField_1.field_Private_String_0;
                    ValidateTextTargetMethod.Invoke(authPage.field_Public_UiInputField_1.GetComponent<InputFieldValidator>(), new object[] { authPage.field_Public_UiInputField_1.field_Private_String_0 });
                }
                return;
            }
            if (SecurePlayerPrefs.HasKey(UserKey))
                SecurePlayerPrefs.DeleteKey(UserKey);
            if (SecurePlayerPrefs.HasKey(PassKey))
                SecurePlayerPrefs.DeleteKey(PassKey);
        }

        private static void SavePlayerPrefs()
        {
            VRCUiPageAuthentication authPage = GetAuthPage();
            if (authPage == null)
                return;
            if (SecurePlayerPrefs.HasKey(ToggleKey))
            {
                if (!string.IsNullOrEmpty(authPage.field_Public_UiInputField_0.prop_String_0))
                    SecurePlayerPrefs.SetString(UserKey, authPage.field_Public_UiInputField_0.prop_String_0, SecurePlayerPrefsPassword);
                if (!string.IsNullOrEmpty(authPage.field_Public_UiInputField_1.prop_String_0))
                    SecurePlayerPrefs.SetString(PassKey, authPage.field_Public_UiInputField_1.prop_String_0, SecurePlayerPrefsPassword);
                return;
            }
            if (SecurePlayerPrefs.HasKey(UserKey))
                SecurePlayerPrefs.DeleteKey(UserKey);
            if (SecurePlayerPrefs.HasKey(PassKey))
                SecurePlayerPrefs.DeleteKey(PassKey);
        }

        private static VRCUiPageAuthentication GetAuthPage()
        {
            if (PageAuthentication == null)
                PageAuthentication = Resources.FindObjectsOfTypeAll<VRCUiPageAuthentication>().First((p) => (p.gameObject.name == "LoginUserPass"));
            return PageAuthentication;
        }

        private static VRCUiManager GetVRCUiManager()
        {
            if (VRCUiManager_Instance_get == null)
                VRCUiManager_Instance_get = typeof(VRCUiManager).GetMethods().First(x => (x.ReturnType == typeof(VRCUiManager)));
            if (VRCUiManager_Instance_get == null)
                return null;
            return (VRCUiManager)VRCUiManager_Instance_get.Invoke(null, new object[0]);
        }
    }
}
