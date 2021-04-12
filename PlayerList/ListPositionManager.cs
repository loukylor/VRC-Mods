using System;
using MelonLoader;
using PlayerList.Config;
using PlayerList.UI;
using PlayerList.Utilities;
using UnhollowerRuntimeLib;
using UnityEngine;

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
            snapToGridSizeLabel.textComponent.text = $"Snap Grid\nSize: {newValue}";
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

            PlayerListConfig.PlayerListPosition = new Vector2(furthestTransform.anchoredPosition.x + (furthestTransform.rect.width / 2), MenuManager.playerListRect.anchoredPosition.y);
            CombineQMColliderAndPlayerListRect(useConfigValues: true);
            UnityEngine.Object.Destroy(temp);
        }
        public static void MovePlayerList()
        {
            MenuManager.playerListRect.anchoredPosition = PlayerListConfig.PlayerListPosition; // So old position var works properly
            shouldMove = true;
            MelonCoroutines.Start(WaitForPress(MenuManager.playerList, new Action<GameObject>((gameObject) =>
            {
                PlayerListConfig.PlayerListPosition = MenuManager.playerListRect.anchoredPosition;
                gameObject.SetActive(!MenuManager.shouldStayHidden);
                UIManager.OpenPage(MenuManager.playerListMenus[1].path);
                MenuManager.playerListRect.localPosition = new Vector3(MenuManager.playerListRect.localPosition.x, MenuManager.playerListRect.localPosition.y, 25);
            })));
            UIManager.OpenPage("UserInterface/QuickMenu/ShortcutMenu");
            MenuManager.playerList.SetActive(true);
        }
        private static System.Collections.IEnumerator WaitForPress(GameObject movingGameObject, Action<GameObject> onComplete = null)
        {
            RectTransform movingGameObjectRect = movingGameObject.GetComponent<RectTransform>();
            Vector3 oldPosition = movingGameObjectRect.anchoredPosition3D;

            while (InputManager.IsUseInputPressed && shouldMove)
            {
                CombineQMColliderAndPlayerListRect();
                movingGameObjectRect.transform.position = InputManager.HitPosition;
                movingGameObjectRect.transform.localPosition = new Vector3(movingGameObjectRect.transform.localPosition.x, movingGameObjectRect.transform.localPosition.y, oldPosition.z);
                movingGameObjectRect.anchoredPosition = Converters.RoundAmount(movingGameObjectRect.anchoredPosition, PlayerListConfig.snapToGridSize.Value);

                yield return null;
            }

            while (!InputManager.IsUseInputPressed && shouldMove)
            {
                CombineQMColliderAndPlayerListRect();
                movingGameObjectRect.transform.position = InputManager.HitPosition;
                movingGameObjectRect.transform.localPosition = new Vector3(movingGameObjectRect.transform.localPosition.x, movingGameObjectRect.transform.localPosition.y, oldPosition.z);
                movingGameObjectRect.anchoredPosition = Converters.RoundAmount(movingGameObjectRect.anchoredPosition, PlayerListConfig.snapToGridSize.Value);

                yield return null;
            }

            if (shouldMove)
            {
                onComplete.Invoke(movingGameObject);
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
                playerListLeft = PlayerListConfig.PlayerListPosition.x - MenuManager.playerListRect.sizeDelta.x / 2;
                playerListTop = PlayerListConfig.PlayerListPosition.y + (MenuManager.playerListRect.sizeDelta.y / 2);
                playerListRight = PlayerListConfig.PlayerListPosition.x + MenuManager.playerListRect.sizeDelta.x / 2;
                playerListBottom = PlayerListConfig.PlayerListPosition.y - (MenuManager.playerListRect.sizeDelta.y / 2);
            }

            collider.size = new Vector2(Math.Abs(Math.Max(Math.Abs(Math.Min(colliderLeft, playerListLeft)), Math.Abs(Math.Max(colliderRight, playerListRight)))) * 2, Math.Abs(Math.Max(Math.Abs(Math.Min(colliderBottom, playerListBottom)), Math.Abs(Math.Max(colliderTop, playerListTop)))) * 2);
        }
    }
}
