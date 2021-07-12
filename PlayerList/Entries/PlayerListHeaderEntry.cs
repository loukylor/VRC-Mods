using System;
using MelonLoader;
using VRC;
using VRChatUtilityKit.Utilities;

namespace PlayerList.Entries
{
    [RegisterTypeInIl2Cpp]
    public class PlayerListHeaderEntry : EntryBase
    {
        public PlayerListHeaderEntry(IntPtr obj0) : base(obj0) { }

        public override string Name { get { return "PlayerList Header"; } }

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
