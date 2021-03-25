using System;
using Harmony;
using MelonLoader;
using PlayerList.Utilities;
using UnityEngine;
using VRC;
using VRC.Core;
using VRCSDK2.Validation.Performance;

namespace PlayerList.Entries
{
    public class PlayerEntry : EntryBase
    {
        // - <color={pingcolor}>{ping}ms</color> | <color={fpscolor}>{fps}</color> | {platform} | <color={perfcolor}>{perf}</color> | {relationship} | <color={rankcolor}>{displayname}</color>
        public override string Name { get { return "Player"; } }

        public static bool worldAllowed = false;

        public Player player;
        public string userID;
        public PlayerNet playerNet;

        public bool blockedYou;
        public bool youBlocked;

        private static bool spoofFriend;
        public static int highestPhotonIdLength = 0;

        public static void Patch(HarmonyInstance harmonyInstance) // All in the name of FUTUREPROOFING REEEEEEEEEEEEEEEEEEEEEE
        {
            harmonyInstance.Patch(typeof(APIUser).GetMethod("IsFriendsWith"), new HarmonyMethod(typeof(PlayerEntry).GetMethod(nameof(OnIsFriend))));
            harmonyInstance.Patch(typeof(RoomManager).GetMethod("Method_Public_Static_Boolean_ApiWorld_ApiWorldInstance_String_Int32_0"), null, new HarmonyMethod(typeof(PlayerEntry).GetMethod(nameof(OnInstanceChange))));
        }
        public override void Init(object[] parameters)
        {
            player = (Player)parameters[0];
            userID = player.field_Private_APIUser_0.id;
            playerNet = player.GetComponent<PlayerNet>();

            gameObject.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(new Action(() => OpenPlayerInQuickMenu(player)));
        }
        protected override void ProcessText(object[] parameters = null)
        {
            if (player == null) Remove(); // Sometimes ppl will desync causing the leave event to not call

            if (playerNet != null)
            {
                if (Config.pingToggle.Value)
                {
                    short ping = playerNet.prop_Int16_0;
                    AddColor(GetPingColor(ping));
                    if (ping < 9999 && ping > -999)
                        AddEndColor(ping.ToString().PadRight(4) + "ms");
                    else
                        AddEndColor(((double)(ping / 1000)).ToString("N1").PadRight(5) + "s");
                    AddSpacer();
                }

                // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
                if (Config.fpsToggle.Value)
                {
                    int fps = Mathf.Clamp((int)(1000f / playerNet.field_Private_Byte_0), -99, 999);

                    AddColor(GetFpsColor(fps));
                    if (playerNet.field_Private_Byte_0 == 0)
                        AddEndColor("?¿?");
                    else
                        AddEndColor(fps.ToString().PadRight(3));
                    AddSpacer();
                }
            }
            else
            {
                if (Config.pingToggle.Value)
                {
                    AddColoredText("#ff0000", "?¿?".PadRight(4));
                    AddSpacer();
                }

                // I STG if I have to remove fps because skids start walking up to people saying poeple's fps im gonna murder someone
                if (Config.fpsToggle.Value)
                {
                    AddColoredText("#ff0000", "?¿?".PadRight(3));
                    AddSpacer();
                }

                playerNet = player.GetComponent<PlayerNet>();
            }

            if (Config.platformToggle.Value)
            {
                AddText(ParsePlatform(player).PadRight(2));
                AddSpacer();
            }

            if (Config.perfToggle.Value)
            {
                PerformanceRating rating = player.field_Internal_VRCPlayer_0.prop_VRCAvatarManager_0.prop_AvatarPerformanceStats_0.field_Private_ArrayOf_PerformanceRating_0[(int)AvatarPerformanceCategory.Overall]; // Get from cache so it doesnt calculate perf all at once
                AddColoredText("#" + ColorUtility.ToHtmlStringRGB(VRCUiAvatarStatsPanel.Method_Private_Static_Color_AvatarPerformanceCategory_PerformanceRating_0(AvatarPerformanceCategory.Overall, rating)), ParsePerformanceText(rating));
                AddSpacer();
            }
            
            if (Config.distanceToggle.Value)
            {
                if (worldAllowed)
                {
                    float distance = (player.transform.position - Player.prop_Player_0.transform.position).magnitude;
                    if (distance < 100)
                    {
                        AddText(distance.ToString("N1").PadRight(4) + "m");
                    }
                    else if (distance < 10000)
                    {
                        AddText((distance / 1000).ToString("N1").PadRight(3) + "km");
                    }
                    else if (distance < 999900)
                    {
                        AddText((distance / 1000).ToString("N0").PadRight(3) + "km");
                    }
                    else
                    {
                        AddText((distance / 9.461e+15).ToString("N2").PadRight(3) + "ly"); // If its too large for kilometers ***just convert to light years***
                    }
                }
                else
                {
                    AddText("0.0 m");
                }
                AddSpacer();
            }

            if (Config.photonIdToggle.Value)
            {
                AddText(player.field_Internal_VRCPlayer_0.field_Private_PhotonView_0.field_Private_Int32_0.ToString().PadRight(highestPhotonIdLength));
                AddSpacer();
            }

            if (Config.displayNameToggle.Value) // Why?
            {
                switch (Config.DisplayNameColorMode)
                {
                    case PlayerListMod.DisplayNameColorMode.TrustAndFriends:
                        AddColoredText("#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(player.field_Private_APIUser_0)), player.field_Private_APIUser_0.displayName);
                        break;
                    case PlayerListMod.DisplayNameColorMode.None:
                        AddText(player.field_Private_APIUser_0.displayName);
                        break;
                    case PlayerListMod.DisplayNameColorMode.TrustOnly:
                        // ty bono for this (https://github.com/ddakebono/)
                        spoofFriend = true;
                        AddColoredText("#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(player.field_Private_APIUser_0)), player.field_Private_APIUser_0.displayName);
                        break;
                    case PlayerListMod.DisplayNameColorMode.FriendsOnly:
                        if (APIUser.IsFriendsWith(player.field_Private_APIUser_0.id))
                            AddColoredText("#" + ColorUtility.ToHtmlStringRGB(VRCPlayer.Method_Public_Static_Color_APIUser_0(player.field_Private_APIUser_0)), player.field_Private_APIUser_0.displayName);
                        else
                            AddText(player.field_Private_APIUser_0.displayName);
                        break;
                }
                AddSpacer();
            }

            if (textComponent.text.Length > 0)
                if (Config.condensedText.Value)
                    textComponent.text = textComponent.text.Remove(textComponent.text.Length - 1, 1);
                else
                    textComponent.text = textComponent.text.Remove(textComponent.text.Length - 3, 3);

            if (!Config.numberedList.Value)
                if (Config.condensedText.Value)
                    AddTextToBeginning("-");
                else
                    AddTextToBeginning(" - ");
            else
                if (Config.condensedText.Value)
                    AddTextToBeginning($"{gameObject.transform.GetSiblingIndex() - 1}.".PadRight((gameObject.transform.parent.childCount - 2).ToString().Length + 1)); // Pad by weird amount because we cant include the header and disabled template in total number of gameobjects
                else
                    AddTextToBeginning($"{gameObject.transform.GetSiblingIndex() - 1}. ".PadRight((gameObject.transform.parent.childCount - 2).ToString().Length + 2));

        }
        public static bool OnIsFriend(ref bool __result)
        {
            if (spoofFriend)
            {
                __result = false;
                spoofFriend = false;
                return false;
            }
            return true;
        }
        public static void OnInstanceChange(ApiWorld __0)
        {
            MelonLogger.Msg("Checking if world is allowed to show distance...");
            worldAllowed = false;
            if (__0 != null)
                MelonCoroutines.Start(VRCUtils.CheckWorld(__0));
        }
        public void Remove()
        {
            EntryManager.playerEntries.Remove(userID);
            EntryManager.entries.Remove(Identifier);
            UnityEngine.Object.DestroyImmediate(gameObject);
            return;
        }

        public static string ParsePlatform(Player player)
        {
            if (player.field_Private_APIUser_0.last_platform == "standalonewindows")
                if (player.field_Private_VRCPlayerApi_0.IsUserInVR())
                    return "VR".PadRight(2);
                else
                    return "PC".PadRight(2);
            else
                return "Q".PadRight(2);
        }
        
        public static void OpenPlayerInQuickMenu(Player player)
        {
            InputManager.SelectPlayer(player.field_Internal_VRCPlayer_0);
            QuickMenuContextualDisplay.Method_Public_Static_Void_VRCPlayer_0(player.field_Internal_VRCPlayer_0);
        }

        public static string GetPingColor(int ping)
        {
            if (ping <= 75)
                return "#00ff00";
            else if (ping > 75 && ping <= 125)
                return "#008000";
            else if (ping > 125 && ping <= 175)
                return "#ffff00";
            else if (ping > 175 && ping <= 225)
                return "#ffa500";
            else
                return "#ff0000";
        }
        public static string GetFpsColor(int fps)
        {
            if (fps >= 60)
                return "#00ff00";
            else if (fps < 60 && fps >= 45)
                return "#008000";
            else if (fps < 45 && fps >= 30)
                return "#ffff00";
            else if (fps < 30 && fps >= 15)
                return "#ffa500";
            else
                return "#ff0000";
        }
        public static string ParsePerformanceText(PerformanceRating rating)
        {
            switch (rating)
            {
                case PerformanceRating.VeryPoor:
                    return "Awful".PadRight(5);
                case PerformanceRating.Poor:
                    return "Poor".PadRight(5);
                case PerformanceRating.Medium:
                    return "Med".PadRight(5);
                case PerformanceRating.Good:
                    return "Good".PadRight(5);
                case PerformanceRating.Excellent:
                    return "Great".PadRight(5);
                case PerformanceRating.None:
                    return "?¿?¿?".PadRight(5);
                    // TODO: add load percentage??
                default:
                    return rating.ToString().PadRight(5);
            }
        }
    }
}
