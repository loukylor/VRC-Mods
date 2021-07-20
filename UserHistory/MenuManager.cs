using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRChatUtilityKit.Ui;
using VRChatUtilityKit.Utilities;

namespace UserHistory
{
    public class MenuManager
    {
        private static int _playerIndex = 0;
        public static int PlayerIndex
        {
            get { return _playerIndex; }
            set
            {
                if (value > UserManager.cachedPlayers.Count - 1 || value < 0)
                    return;

                _playerIndex = value;

                pageNumLabel.TextComponent.text = $"Page: {PageNum} of {LastPageNum}";

                if (PageNum == LastPageNum || LastPageNum == 1)
                    pageDown.gameObject.SetActive(false);
                else
                    pageDown.gameObject.SetActive(true);

                if (PageNum == 1)
                {
                    pageUp.gameObject.SetActive(false);
                    backButton.gameObject.SetActive(true);
                }
                else
                {
                    pageUp.gameObject.SetActive(true);
                    backButton.gameObject.SetActive(false);
                }

                for (int i = 0; i < 9; i++)
                {

                    if (_playerIndex + i >= UserManager.cachedPlayers.Count)
                    {
                        buttons[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        UserManager.CachedPlayer player = UserManager.cachedPlayers[PlayerIndex + i];
                        buttons[i].TextComponent.text = player.name;
                        buttons[i].TooltipComponent.field_Public_String_0 = $"User: {player.name} joined on {player.timeJoined:G}";
                        buttons[i].ButtonComponent.onClick = new Button.ButtonClickedEvent();
                        buttons[i].ButtonComponent.onClick.AddListener(new Action(() =>
                        {
                            if (player.user == null)
                                APIUser.FetchUser(player.id, new Action<APIUser>(OnUserReceived), null);
                            else
                                OnUserReceived(player.user);
                        }));
                        buttons[i].gameObject.SetActive(true);
                    }
                }
            }
        }

        private static void OnUserReceived(APIUser user)
        {
            UiManager.OpenBigMenu(false);
            UiManager.OpenUserInUserInfoPage(user);
        }

        public static int PageNum
        {
            get { return Mathf.CeilToInt((_playerIndex + 1) / 9f); }
        }
        public static int LastPageNum
        {
            get { return Mathf.CeilToInt(UserManager.cachedPlayers.Count / 9f); }
        }

        public static SingleButton openButton;
        public static SubMenu menu;
        private static readonly SingleButton[] buttons = new SingleButton[9];
        private static SingleButton pageUp;
        private static SingleButton pageDown;
        private static SingleButton backButton;
        private static Label pageNumLabel;

        public static void UiInit()
        {
            MelonLogger.Msg("Loading UI...");

            menu = new SubMenu(QuickMenu.prop_QuickMenu_0.gameObject, "UserHistoryMenu");

            openButton = new SingleButton(QuickMenu.prop_QuickMenu_0.transform.FindChild("ShortcutMenu").gameObject, new Vector3(Config.openButtonX.Value, Config.openButtonY.Value), "User History", new Action(OpenUserHistoryMenu), "Open User history", "UserHistoryButton");
            openButton.gameObject.SetActive(!(VRCUtils.IsUIXPresent && Config.useUIX.Value));
            Config.openButtonX.OnValueChanged += OnPositionChange;
            Config.openButtonY.OnValueChanged += OnPositionChange;

            if (VRCUtils.IsUIXPresent)
                typeof(UIXManager).GetMethod("AddOpenButtonToUIX").Invoke(null, null);

            pageUp = new SingleButton(menu.gameObject, GameObject.Find("UserInterface/QuickMenu/EmojiMenu/PageUp"), new Vector3(4, 0), "", new Action(() => PlayerIndex -= 9), $"Go up a page", "UpPageButton");
            pageDown = new SingleButton(menu.gameObject, GameObject.Find("UserInterface/QuickMenu/EmojiMenu/PageDown"), new Vector3(4, 2), "", new Action(() => PlayerIndex += 9), $"Go down a page", "DownPageButton");
            backButton = new SingleButton(menu.gameObject, new Vector3(4, 0), "Back", new Action(() => UiManager.OpenSubMenu("UserInterface/QuickMenu/ShortcutMenu")), "Press to go back to the Shortcut Menu", "BackButton", textColor: Color.yellow);
            backButton.gameObject.SetActive(false);
            pageNumLabel = new Label(menu.gameObject, new Vector3(4, 1), $"Page: 1 of {LastPageNum}", "PageNumberLabel");

            for (int i = 0; i < 9; i++)
                buttons[i] = new SingleButton(menu.gameObject, new Vector3((i % 3) + 1, Mathf.Floor(i / 3)), "Placeholder text", null, "Placeholder text", $"World Button {i + 1}", resize: true);

            MelonLogger.Msg("UI Loaded!");
        }

        public static void OpenUserHistoryMenu()
        {
            menu.OpenSubMenu();
            PlayerIndex = 0;
        }

        private static void OnPositionChange(float oldValue, float newValue)
        {
            if (oldValue == newValue) return;

            openButton.gameObject.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(Config.openButtonX.Value, Config.openButtonY.Value));
        }
    }
}
