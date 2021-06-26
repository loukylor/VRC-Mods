using System.IO;
using System.Reflection;
using AskToPortal.Components;
using HarmonyLib;
using MelonLoader;
using UnhollowerRuntimeLib; 
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRChatUtilityKit.Ui;

namespace AskToPortal
{
    class MenuManager
    {
        public static GameObject detailedMenu;
        public static AskToPortalPageDetailed detailedMenuComponent;
        public static VRCUiPageTab detailedMenuTab;
        public static GameObject basicMenu;
        public static AskToPortalPageBasic basicMenuComponent;
        public static VRCUiPageTab basicMenuTab;

        private static Font dosisRegular;
        private static Font dosisSemiBold;
        private static Sprite bifrost;

        private static GameObject vrcUiTabsParent;
        private static GameObject tabsParent;

        public static void LoadAssetBundle()
        {
            // Stolen from UIExpansionKit (https://github.com/knah/VRCMods/blob/master/UIExpansionKit) #Imnotaskidiswear
            MelonLogger.Msg("Loading UI...");
            ClassInjector.RegisterTypeInIl2Cpp<AskToPortalPromptPage>();
            ClassInjector.RegisterTypeInIl2Cpp<AskToPortalPageDetailed>();
            ClassInjector.RegisterTypeInIl2Cpp<AskToPortalPageBasic>();
            GameObject screens = GameObject.Find("UserInterface/MenuContent/Screens");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AskToPortal.asktoportalui.assetbundle"))
            {
                using (var memoryStream = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(memoryStream);
                    AssetBundle assetBundle = AssetBundle.LoadFromMemory_Internal(memoryStream.ToArray(), 0);
                    detailedMenu = Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/Prefabs/AskToPortalDetailed.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), screens.transform);
                    basicMenu = Object.Instantiate(assetBundle.LoadAsset_Internal("Assets/Prefabs/AskToPortalBasic.prefab", Il2CppType.Of<GameObject>()).Cast<GameObject>(), screens.transform);
                }
            }

            detailedMenu.SetActive(false);
            detailedMenuComponent = detailedMenu.AddComponent<AskToPortalPageDetailed>();

            basicMenu.SetActive(false);
            basicMenuComponent = basicMenu.AddComponent<AskToPortalPageBasic>();

            dosisRegular = GameObject.Find("UserInterface/MenuContent/Screens/Settings/VolumePanel/VolumeUi/Label").GetComponent<Text>().font;
            dosisSemiBold = GameObject.Find("UserInterface/MenuContent/Screens/Settings/VolumePanel/TitleText (1)").GetComponent<Text>().font;
            bifrost = GameObject.Find("UserInterface/MenuContent/Screens/Settings/Footer/Logout").GetComponent<Image>().sprite;

            AddFontAndImages(detailedMenu.transform);
            AddFontAndImages(basicMenu.transform);

            detailedMenuComponent.Init();
            basicMenuComponent.Init();

            vrcUiTabsParent = GameObject.Find("UserInterface/MenuContent/Backdrop/Header/Tabs");
            tabsParent = Object.Instantiate(vrcUiTabsParent, vrcUiTabsParent.transform.parent);
            Transform tabsContent = tabsParent.transform.GetChild(0).GetChild(0);
            for (int i = tabsContent.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(tabsContent.GetChild(i).gameObject);
            tabsParent.name = "AskToPortalTabs";
            tabsContent.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            UiManager.OnBigMenuClosed += new System.Action(() => { tabsParent.SetActive(false); vrcUiTabsParent.SetActive(true); });

            GameObject basicTabGameObject = Object.Instantiate(GameObject.Find("UserInterface/MenuContent/Backdrop/Header/Tabs/ViewPort/Content/WorldsPageTab"), tabsParent.transform.Find("ViewPort/Content"));
            basicTabGameObject.name = "BasicPageTab";
            basicTabGameObject.transform.Find("Button/Text").GetComponent<Text>().text = "Basic";
            basicMenuTab = basicTabGameObject.GetComponent<VRCUiPageTab>();
            basicMenuTab.field_Public_String_1 = "UserInterface/MenuContent/Screens/AskToPortalBasic(Clone)";

            GameObject detailedTabGameObject = Object.Instantiate(GameObject.Find("UserInterface/MenuContent/Backdrop/Header/Tabs/ViewPort/Content/WorldsPageTab"), tabsParent.transform.Find("ViewPort/Content"));
            detailedTabGameObject.name = "DetailedPageTab";
            detailedTabGameObject.transform.Find("Button/Text").GetComponent<Text>().text = "Detailed";
            detailedMenuTab = detailedTabGameObject.GetComponent<VRCUiPageTab>();
            detailedMenuTab.field_Public_String_1 = "UserInterface/MenuContent/Screens/AskToPortalDetailed(Clone)";

            AskToPortalMod.Instance.HarmonyInstance.Patch(typeof(VRCUiPageTab).GetMethod("ShowPage"), new HarmonyMethod(typeof(MenuManager).GetMethod(nameof(OnTabShowPage), BindingFlags.NonPublic | BindingFlags.Static)));
        }

        // Dunno how to store only references to the assets in an assetbundle so im just gonna give tags and assign things based off tags
        private static void AddFontAndImages(Transform transform)
        {
            foreach (var child in transform)
            {
                Transform childTransform = child.Cast<Transform>();
                if (childTransform.name.Contains("{DosisRegular}"))
                {
                    childTransform.GetComponent<Text>().font = dosisRegular;
                    childTransform.name = childTransform.name.Replace(" {DosisRegular}", "");
                }
                else if (childTransform.name.Contains("{DosisSemiBold}"))
                {
                    childTransform.GetComponent<Text>().font = dosisSemiBold;
                    childTransform.name = childTransform.name.Replace(" {DosisSemiBold}", "");
                }
                else if (childTransform.name.Contains("{Bifrost}"))
                {
                    childTransform.GetComponent<Image>().sprite = bifrost;
                    childTransform.name = childTransform.name.Replace(" {Bifrost}", "");
                }

                AddFontAndImages(childTransform);
            }
        }

        public static void OnPortalEnter(RoomInfo roomInfo, PortalInternal portal, APIUser dropper, string worldId, string roomId)
        {
            UiManager.OpenBigMenu();

            vrcUiTabsParent.SetActive(false);
            tabsParent.SetActive(true);

            VRCUiManager.prop_VRCUiManager_0.HideScreen("SCREEN");
            basicMenuComponent.Setup(portal, roomInfo, dropper, worldId, roomId);
            detailedMenuComponent.Setup(portal, roomInfo, dropper, worldId, roomId);

            if (AskToPortalSettings.startOnDetailed.Value)
                detailedMenuTab.ShowPage();
            else
                basicMenuTab.ShowPage();
        }

        private static void OnTabShowPage()
        {
            // This string randomly gets corrupted and it's super important so
            basicMenuComponent.field_Public_String_0 = "SCREEN";
            detailedMenuComponent.field_Public_String_0 = "SCREEN";
        }
    }
}
