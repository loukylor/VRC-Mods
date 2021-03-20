using System;
using MelonLoader;
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
        private static int _snapToGridSize;
        public static int SnapToGridSize
        {
            get { return _snapToGridSize; }
            set
            {
                if (value <= 0) return;

                EntryManager.shouldSaveConfig = true;
                Config.snapToGridSize.Value = value;
                snapToGridSizeLabel.textComponent.text = $"Snap Grid\nSize: {value}";
                _snapToGridSize = value;
            }
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

            Config.PlayerListPosition = new Vector2(furthestTransform.anchoredPosition.x + (furthestTransform.rect.width / 2), PlayerListMod.playerListRect.anchoredPosition.y);
            CombineQMColliderAndPlayerListRect(useConfigValues: true);
            UnityEngine.Object.Destroy(temp);
            EntryManager.shouldSaveConfig = true;
        }
        public static void MovePlayerList()
        {
            PlayerListMod.playerListRect.anchoredPosition = Config.PlayerListPosition; // So old position var works properly
            shouldMove = true;
            MelonCoroutines.Start(WaitForPress(PlayerListMod.playerList, new Action<GameObject>((gameObject) =>
            {
                Config.PlayerListPosition = PlayerListMod.playerListRect.anchoredPosition;
                gameObject.SetActive(!MenuManager.shouldStayHidden);
                UIManager.OpenPage(MenuManager.playerListMenus[1].path);
                PlayerListMod.playerListRect.localPosition = new Vector3(PlayerListMod.playerListRect.localPosition.x, PlayerListMod.playerListRect.localPosition.y, 25);
                EntryManager.shouldSaveConfig = true;
            })));
            UIManager.OpenPage("UserInterface/QuickMenu/ShortcutMenu");
            PlayerListMod.playerList.SetActive(true);
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
                movingGameObjectRect.anchoredPosition = Converters.RoundAmount(movingGameObjectRect.anchoredPosition, SnapToGridSize);

                yield return null;
            }

            while (!InputManager.IsUseInputPressed && shouldMove)
            {
                CombineQMColliderAndPlayerListRect();
                movingGameObjectRect.transform.position = InputManager.HitPosition;
                movingGameObjectRect.transform.localPosition = new Vector3(movingGameObjectRect.transform.localPosition.x, movingGameObjectRect.transform.localPosition.y, oldPosition.z);
                movingGameObjectRect.anchoredPosition = Converters.RoundAmount(movingGameObjectRect.anchoredPosition, SnapToGridSize);

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
                playerListLeft = PlayerListMod.playerListRect.anchoredPosition.x - PlayerListMod.playerListRect.sizeDelta.x / 2;
                playerListTop = PlayerListMod.playerListRect.anchoredPosition.y + (PlayerListMod.playerListRect.sizeDelta.y / 2);
                playerListRight = PlayerListMod.playerListRect.anchoredPosition.x + PlayerListMod.playerListRect.sizeDelta.x / 2;
                playerListBottom = PlayerListMod.playerListRect.anchoredPosition.y - (PlayerListMod.playerListRect.sizeDelta.y / 2);
            }
            else
            {
                playerListLeft = Config.PlayerListPosition.x - PlayerListMod.playerListRect.sizeDelta.x / 2;
                playerListTop = Config.PlayerListPosition.y + (PlayerListMod.playerListRect.sizeDelta.y / 2);
                playerListRight = Config.PlayerListPosition.x + PlayerListMod.playerListRect.sizeDelta.x / 2;
                playerListBottom = Config.PlayerListPosition.y - (PlayerListMod.playerListRect.sizeDelta.y / 2);
            }

            collider.size = new Vector2(Math.Abs(Math.Max(Math.Abs(Math.Min(colliderLeft, playerListLeft)), Math.Abs(Math.Max(colliderRight, playerListRight)))) * 2, Math.Abs(Math.Max(Math.Abs(Math.Min(colliderBottom, playerListBottom)), Math.Abs(Math.Max(colliderTop, playerListTop)))) * 2);
        }
    }
}
