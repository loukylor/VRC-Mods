using System;
using MelonLoader;
using PlayerList.Config;
using PlayerList.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;

namespace PlayerList
{
    class ListPositionManager
    {
        public static bool shouldMove = false;

        public static Label snapToGridSizeLabel;

        public static void Init()
        {
            PlayerListConfig.snapToGridSize.OnValueChanged += OnSnapToGridSizeChanged;
        }
        public static void OnSnapToGridSizeChanged(int oldValue, int newValue)
        {
            snapToGridSizeLabel.TextComponent.text = $"Snap Grid\nSize: {newValue}";
        }

        public static void MovePlayerListToEndOfMenu()
        {
            GameObject temp = new GameObject("temp", new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[] { Il2CppType.Of<RectTransform>() }));
            RectTransform furthestTransform = temp.GetComponent<RectTransform>(); // Create new gameobject with recttransform on it
            foreach (var child in Constants.shortcutMenu.transform)
            {
                RectTransform childRect = child.Cast<RectTransform>();
                if (childRect.gameObject.activeSelf && childRect.anchoredPosition.x + childRect.rect.width > furthestTransform.anchoredPosition.x + furthestTransform.rect.width)
                    furthestTransform = childRect;
            }

            PlayerListConfig.playerListPosition.Value = new Vector2(furthestTransform.anchoredPosition.x + (furthestTransform.rect.width / 2), MenuManager.playerListRect.anchoredPosition.y);
            CombineQMColliderAndPlayerListRect(useConfigValues: true);
            UnityEngine.Object.Destroy(temp);
        }
        public static void MovePlayerList()
        {
            MenuManager.playerListRect.anchoredPosition = PlayerListConfig.playerListPosition.Value; // So old position var works properly
            shouldMove = true;
            MelonCoroutines.Start(WaitForPress(MenuManager.playerList, new Action<GameObject>((gameObject) =>
            {
                PlayerListConfig.playerListPosition.Value = MenuManager.playerListRect.anchoredPosition;
                gameObject.SetActive(!MenuManager.shouldStayHidden);
                UiManager.OpenSubMenu(MenuManager.playerListMenus[1].Path);
                MenuManager.playerListRect.localPosition = MenuManager.playerListRect.localPosition.SetZ(25);
            })));
            UiManager.OpenSubMenu("UserInterface/QuickMenu/ShortcutMenu");
            MenuManager.playerList.SetActive(true);
        }
        private static System.Collections.IEnumerator WaitForPress(GameObject movingGameObject, Action<GameObject> onComplete = null)
        {
            RectTransform movingGameObjectRect = movingGameObject.GetComponent<RectTransform>();
            Vector3 oldPosition = movingGameObjectRect.anchoredPosition3D;

            while (CursorUtils.IsUseInputPressed && shouldMove)
            {
                CombineQMColliderAndPlayerListRect();
                movingGameObjectRect.transform.position = CursorUtils.HitPosition;
                movingGameObjectRect.transform.localPosition = movingGameObjectRect.transform.localPosition.SetZ(oldPosition.z);
                movingGameObjectRect.anchoredPosition = movingGameObjectRect.anchoredPosition.RoundAmount(PlayerListConfig.snapToGridSize.Value);

                yield return null;
            }

            while (!CursorUtils.IsUseInputPressed && shouldMove)
            {
                CombineQMColliderAndPlayerListRect();
                movingGameObjectRect.transform.position = CursorUtils.HitPosition;
                movingGameObjectRect.transform.localPosition = movingGameObjectRect.transform.localPosition.SetZ(oldPosition.z);
                movingGameObjectRect.anchoredPosition = movingGameObjectRect.anchoredPosition.RoundAmount(PlayerListConfig.snapToGridSize.Value);

                yield return null;
            }

            if (shouldMove)
            {
                onComplete.DelegateSafeInvoke(movingGameObject);
            }
            else
            {
                movingGameObjectRect.anchoredPosition3D = oldPosition;
                CombineQMColliderAndPlayerListRect();
            }
            shouldMove = false;
        }
        public static void CombineQMColliderAndPlayerListRect(bool useConfigValues = false)
        {
            BoxCollider collider = Constants.quickMenu.GetComponent<BoxCollider>();
            float colliderLeft = -Constants.quickMenuColliderSize.x / 2;
            float colliderTop = Constants.quickMenuColliderSize.y / 2;
            float colliderRight = Constants.quickMenuColliderSize.x / 2;
            float colliderBottom = -Constants.quickMenuColliderSize.y / 2;

            float playerListLeft;
            float playerListTop;
            float playerListRight;
            float playerListBottom;

            if (!useConfigValues)
            {
                playerListLeft = MenuManager.playerListRect.anchoredPosition.x - MenuManager.playerListRect.sizeDelta.x / 2;
                playerListTop = MenuManager.playerListRect.anchoredPosition.y + (MenuManager.playerListRect.sizeDelta.y / 2);
                playerListRight = MenuManager.playerListRect.anchoredPosition.x + MenuManager.playerListRect.sizeDelta.x / 2;
                playerListBottom = MenuManager.playerListRect.anchoredPosition.y - (MenuManager.playerListRect.sizeDelta.y / 2);
            }
            else
            {
                playerListLeft = PlayerListConfig.playerListPosition.Value.x - MenuManager.playerListRect.sizeDelta.x / 2;
                playerListTop = PlayerListConfig.playerListPosition.Value.y + (MenuManager.playerListRect.sizeDelta.y / 2);
                playerListRight = PlayerListConfig.playerListPosition.Value.x + MenuManager.playerListRect.sizeDelta.x / 2;
                playerListBottom = PlayerListConfig.playerListPosition.Value.y - (MenuManager.playerListRect.sizeDelta.y / 2);
            }

            collider.size = new Vector2(Math.Abs(Math.Max(Math.Abs(Math.Min(colliderLeft, playerListLeft)), Math.Abs(Math.Max(colliderRight, playerListRight)))) * 2, Math.Abs(Math.Max(Math.Abs(Math.Min(colliderBottom, playerListBottom)), Math.Abs(Math.Max(colliderTop, playerListTop)))) * 2);
        }
    }
}
