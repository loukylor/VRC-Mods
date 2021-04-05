using System;
using InstanceHistory.UI;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UIExpansionKit.API;

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
                        string instanceText = instance.worldName + instance.instanceId.Split('~')[0];
                        buttons[i].textComponent.text = instanceText;
                        buttons[i].textComponent.text = instanceText;
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

            //new SingleButton(QuickMenu.prop_QuickMenu_0.transform.FindChild("ShortcutMenu").gameObject, new Vector3(0, 0), "Instance History", new Action(OpenInstanceHistoryMenu), "Open instance history", "InstancenHistoryButton");
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Open Instance History", new Action(OpenInstanceHistoryMenu));

            pageUp = new SingleButton(menu.gameObject, "UserInterface/QuickMenu/EmojiMenu/PageUp", new Vector3(4, 0), "", new Action(() => InstanceIndex -= 9), $"Go up a page", "UpPageButton");
            pageDown = new SingleButton(menu.gameObject, "UserInterface/QuickMenu/EmojiMenu/PageDown", new Vector3(4, 2), "", new Action(() => InstanceIndex += 9), $"Go down a page", "DownPageButton");
            backButton = new SingleButton(menu.gameObject, new Vector3(4, 0), "Back", new Action(() => UIManager.OpenPage("UserInterface/QuickMenu/ShortcutMenu")), "Press to go back to the Shortcut Menu", "BackButton", color: Color.yellow);
            backButton.gameObject.SetActive(false);
            pageNumLabel = new Label(menu.gameObject, new Vector3(4, 1), $"Page: 1 of {LastPageNum}", "PageNumberLabel");

            for (int i = 0; i < 9; i++)
                buttons[i] = new SingleButton(menu.gameObject, new Vector3((i % 3) + 1, Mathf.Floor(i / 3)), "Placeholder text", null, "Placeholder text", $"World Button {i + 1}", resize: true);
        }

        public static void OpenInstanceHistoryMenu()
        {
            UIManager.OpenPage(menu.path);
            InstanceIndex = 0;
        }
    }
}
