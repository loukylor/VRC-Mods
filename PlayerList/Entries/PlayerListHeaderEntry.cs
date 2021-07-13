using System;
using MelonLoader;
using UnhollowerBaseLib.Attributes;
using VRC;
using VRChatUtilityKit.Utilities;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class PlayerListHeaderEntry : EntryBase
    {
        public PlayerListHeaderEntry(IntPtr obj0) : base(obj0) { }

        [HideFromIl2Cpp]
        public override string Name => "PlayerList Header";

        [HideFromIl2Cpp]
        public override void Init(object[] parameters = null)
        {
            NetworkEvents.OnPlayerJoined += OnPlayerCountChanged;
            NetworkEvents.OnPlayerLeft += OnPlayerCountChanged;
        }
        private void OnPlayerCountChanged(Player player)
        {
            textComponent.text = OriginalText.Replace("{playercount}", PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0.Count.ToString());
        }
    }
}
