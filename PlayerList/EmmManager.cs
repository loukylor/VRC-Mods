using System;
using System.Reflection;
using System.Text;
using emmVRC;
using emmVRC.Hacks;
using emmVRC.Libraries;
using emmVRC.Menus;
using PlayerList.Entries;
using VRC;
using VRC.Core;

namespace PlayerList
{
    class EmmManager
    {
        public static string spoofedName;
        private static bool hasInit = false;

        public static void OnSceneWasLoaded()
        {
            if (hasInit)
                return;

            if (Configuration.JSONConfig.InfoSpoofingEnabled)
                typeof(LocalPlayerEntry).GetField("emmNameSpoofEnabled").SetValue(null, true);

            spoofedName = NameSpoofGenerator.spoofedName;
            typeof(LocalPlayerEntry).GetField("emmSpoofedName").SetValue(null, spoofedName);

            PageItem infoSpoofing = (PageItem)typeof(SettingsMenu).GetField("InfoSpoofing", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            infoSpoofing.OnAction += new Action(() =>
            {
                typeof(LocalPlayerEntry).GetField("emmNameSpoofEnabled").SetValue(null, true);
                EntryManager.localPlayerEntry?.OnConfigChanged();
                foreach (EntryBase entry in EntryManager.generalInfoEntries)
                    entry.textComponent.text = entry.textComponent.text.Replace(APIUser.CurrentUser.displayName, spoofedName); // I'm lazy ok
            });
            infoSpoofing.OffAction += new Action(() =>
            {
                typeof(LocalPlayerEntry).GetField("emmNameSpoofEnabled").SetValue(null, false);
                EntryManager.localPlayerEntry?.OnConfigChanged();
                foreach (EntryBase entry in EntryManager.generalInfoEntries)
                    entry.textComponent.text = entry.textComponent.text.Replace(spoofedName, APIUser.CurrentUser.displayName); // I'm lazy ok
            });

            hasInit = true;
        }

        public static void AddSelfToUpdateDelegate()
        {
            LocalPlayerEntry.updateDelegate += OnAddDisplayName;
        }

        public static void OnAddDisplayName(Player player, LocalPlayerEntry entry, ref StringBuilder tempString)
        {
            tempString.Append("<color=" + entry.playerColor + ">" + spoofedName + "</color>" + PlayerEntry.separator);
        }
    }
}
