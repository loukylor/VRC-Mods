using System;
using InstanceHistory.UI;
using InstanceHistory.Utilities;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace InstanceHistory
{
    class MenuManager
    {
        private static int _instanceIndex = 0;
        public static int InstanceIndex
        {
            get { return _instanceIndex; }
            set
            {
                if (value > InstanceManager.instances.Count - 1 || value < 0)
                    return;

                _instanceIndex = value;

                pageNumLabel.textComponent.text = $"Page: {PageNum} of {LastPageNum}";

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

                    if (_instanceIndex + i >= InstanceManager.instances.Count)
                    {
                        buttons[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        InstanceManager.WorldInstance instance = InstanceManager.instances[InstanceIndex + i];
                        buttons[i].textComponent.text = instance.worldName + ": " + instance.instanceId.Split('~')[0];
                        buttons[i].buttonComponent.onClick = new Button.ButtonClickedEvent();
                        buttons[i].buttonComponent.onClick.AddListener(new Action(() => WorldManager.EnterWorld(instance.worldId + ":" + instance.instanceId)));
                        buttons[i].gameObject.SetActive(true);
                    }
                }
            }
        }
        public static int PageNum
        {
            get { return Mathf.CeilToInt((_instanceIndex + 1) / 9f); }
        }
        public static int LastPageNum
        {
            get { return Mathf.CeilToInt(InstanceManager.instances.Count / 9f); }
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

            menu = new SubMenu(QuickMenu.prop_QuickMenu_0.gameObject, "InstanceHistoryModMenu");

            openButton = new SingleButton(QuickMenu.prop_QuickMenu_0.transform.FindChild("ShortcutMenu").gameObject, new Vector3(Config.openButtonX.Value, Config.openButtonY.Value), "Instance History", new Action(OpenInstanceHistoryMenu), "Open instance history", "InstancenHistoryButton");
            openButton.gameObject.SetActive(!(InstanceHistoryMod.HasUIX && Config.useUIX.Value));
            Config.openButtonX.OnValueChanged += OnPositionChange;
            Config.openButtonY.OnValueChanged += OnPositionChange;

            if (InstanceHistoryMod.HasUIX)
                typeof(UIXManager).GetMethod("AddOpenButtonToUIX").Invoke(null, null);

            pageUp = new SingleButton(menu.gameObject, "UserInterface/QuickMenu/EmojiMenu/PageUp", new Vector3(4, 0), "", new Action(() => InstanceIndex -= 9), $"Go up a page", "UpPageButton");
            pageDown = new SingleButton(menu.gameObject, "UserInterface/QuickMenu/EmojiMenu/PageDown", new Vector3(4, 2), "", new Action(() => InstanceIndex += 9), $"Go down a page", "DownPageButton");
            backButton = new SingleButton(menu.gameObject, new Vector3(4, 0), "Back", new Action(() => UIManager.OpenPage("UserInterface/QuickMenu/ShortcutMenu")), "Press to go back to the Shortcut Menu", "BackButton", color: Color.yellow);
            backButton.gameObject.SetActive(false);
            pageNumLabel = new Label(menu.gameObject, new Vector3(4, 1), $"Page: 1 of {LastPageNum}", "PageNumberLabel");

            for (int i = 0; i < 9; i++)
                buttons[i] = new SingleButton(menu.gameObject, new Vector3((i % 3) + 1, Mathf.Floor(i / 3)), "Placeholder text", null, "Placeholder text", $"World Button {i + 1}", resize: true);

            MelonLogger.Msg("UI Loaded!");
        }

        public static void OpenInstanceHistoryMenu()
        {
            UIManager.OpenPage(menu.path);
            InstanceIndex = 0;
        }

        private static void OnPositionChange(float oldValue, float newValue)
        {
            if (oldValue == newValue) return;

            openButton.gameObject.transform.localPosition = Converters.ConvertToUnityUnits(new Vector3(Config.openButtonX.Value, Config.openButtonY.Value));
        }
    }
}
