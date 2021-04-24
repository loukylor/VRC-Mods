using System;
using System.Linq;
using System.Reflection;
using Harmony;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI;

[assembly: MelonInfo(typeof(PreviewScroller.PreviewScrollerMod), "PreviewScroller", "0.0.1", "loukylor", "https://github.com/loukylor/VRC-Mods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace PreviewScroller
{
    public class PreviewScrollerMod : MelonMod
    {
        public override void VRChat_OnUiManagerInit()
        {
            foreach (MethodInfo method in typeof(PageAvatar).GetMethods().Where(mi => mi.Name.StartsWith("Method_Private_Void_String_GameObject_AvatarPerformanceStats_")))
                Harmony.Patch(method, null, new HarmonyMethod(typeof(PreviewScrollerMod).GetMethod(nameof(OnPedestalAvatarInstantiated), BindingFlags.NonPublic | BindingFlags.Static)));
            GameObject scrollerContainer = new GameObject("ScrollerContainer", new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[2] { Il2CppType.Of<RectMask2D>(), Il2CppType.Of<RectTransform>() }));
            RectTransform scrollerContainerRect = scrollerContainer.GetComponent<RectTransform>();
            scrollerContainerRect.SetParent(GameObject.Find("UserInterface/MenuContent/Screens/Avatar").transform);
            scrollerContainerRect.anchoredPosition3D = new Vector3(-565, 20, 1);
            scrollerContainerRect.localScale = Vector3.one;
            scrollerContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
            scrollerContainerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 650);

            GameObject scrollerContent = new GameObject("ScrollerContent", new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[2] { Il2CppType.Of<Image>(), Il2CppType.Of<RectTransform>() }));
            RectTransform scrollerContentRect = scrollerContent.GetComponent<RectTransform>();
            scrollerContentRect.SetParent(scrollerContainerRect);

            scrollerContentRect.anchoredPosition3D = Vector3.zero;
            scrollerContentRect.localScale = Vector3.one;
            scrollerContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 800);
            scrollerContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1300);
            //scrollerContentRect.GetComponent<Image>().sprite = GameObject.Find("UserInterface/QuickMenu/UIElementsMenu/NameplatesOnButton").GetComponent<Image>().sprite;
            scrollerContentRect.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            ScrollRect scrollRect = scrollerContainer.AddComponent<ScrollRect>();
            scrollRect.content = scrollerContentRect;
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            scrollRect.decelerationRate = 0.03f;
            scrollRect.scrollSensitivity = 6;
            scrollRect.onValueChanged = new ScrollRect.ScrollRectEvent();
            GameObject pedestal = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase/MainRoot/MainModel");
            pedestal.transform.localPosition += new Vector3(0, 0.5f);
            MonoBehaviour autoTurn = pedestal.GetComponents<MonoBehaviour>().First(c => c.GetIl2CppType().FullName == "UnityStandardAssets.Utility.AutoMoveAndRotate");
            UnityEngine.Object.DestroyImmediate(autoTurn);

            Vector2 lastPos = Vector2.zero;

            scrollRect.onValueChanged.AddListener(new Action<Vector2>((pos) =>
            {
                Vector2 velocity = pos - lastPos;

                lastPos = pos;
                Vector2 scrollRectVelocity = scrollRect.velocity;
                if (scrollRect.verticalNormalizedPosition > 1 && velocity.y > 0)
                {
                    scrollRect.verticalNormalizedPosition = 0;
                    lastPos.y = 0;
                }
                else if (scrollRect.verticalNormalizedPosition < 0 && velocity.y < 0)
                {
                    scrollRect.verticalNormalizedPosition = 1;
                    lastPos.y = 1;
                }
                if (scrollRect.horizontalNormalizedPosition > 1 && velocity.x > 0)
                {
                    scrollRect.horizontalNormalizedPosition = 0;
                    lastPos.x = 0;
                }
                else if (scrollRect.horizontalNormalizedPosition < 0 && velocity.x < 0)
                {
                    scrollRect.horizontalNormalizedPosition = 1;
                    lastPos.x = 1;
                }
                pedestal.transform.Rotate(new Vector2(velocity.normalized.y, velocity.normalized.x), velocity.magnitude * 375 * Time.deltaTime);
                pedestal.transform.parent.localPosition = new Vector3(0, -0.5f);
                scrollRect.velocity = scrollRectVelocity;
            }));
        }
        private static void OnPedestalAvatarInstantiated(GameObject __1)
        {
            if (__1 == null) return;

            __1.transform.localPosition = new Vector3(0, -0.5f);
            __1.transform.parent.localRotation = new Quaternion();
        }
    }
}
