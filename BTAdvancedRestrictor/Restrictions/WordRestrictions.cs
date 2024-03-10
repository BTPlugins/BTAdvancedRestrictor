using Rocket.Unturned.Events;
using Rocket.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rocket.Unturned.Events.UnturnedEvents;
using static Rocket.Unturned.Events.UnturnedPlayerEvents;
using AdvancedRestrictor.Helpers;
using BTAdvancedRestrictor.Helpers;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using AdvancedRestrictor;

namespace BTAdvancedRestrictor.Restrictions
{
    public class WordRestrictions
    {
        public void Init()
        {
            U.Events.OnPlayerConnected += OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerChatted += OnPlayerChatted;
        }
        public void Destroy()
        {
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            UnturnedPlayerEvents.OnPlayerChatted -= OnPlayerChatted;
        }
        private void OnPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        {
            if (message.StartsWith("/")) return;
            foreach (var blacklistedWord in AdvancedRestrictorPlugin.Instance.Config.RestrictedWords)
            {
                if (message.ToLower().Contains(blacklistedWord.Name.ToLower()))
                {
                    DebugManager.SendDebugMessage("Restricted Word: " + blacklistedWord.Name + " From " + player.CharacterName);
                    TranslationHelper.SendMessageTranslation(player.CSteamID, "MessageRestricted", blacklistedWord.Name);
                    cancel = true;
                    break;
                }
            }
            if (cancel) return;
            cancel = false;
        }
        private void OnPlayerConnected(UnturnedPlayer player)
        {
            if (player.CharacterName.Contains("<#") && AdvancedRestrictorPlugin.Instance.Config.FakeColoredNames.RestrictFakeColoredNames)
            {
                DebugManager.SendDebugMessage(player.CharacterName + " Contains <#HexCode> in their Name. Kicking");
                player.Kick(AdvancedRestrictorPlugin.Instance.Config.FakeColoredNames.KickMessage);
                return;
            }
            foreach (var blacklistedName in AdvancedRestrictorPlugin.Instance.Config.RestrictedNames)
            {
                DebugManager.SendDebugMessage("Checking " + blacklistedName.Name + " for " + player.CharacterName);
                if (player.CharacterName.Contains(blacklistedName.Name))
                {
                    DebugManager.SendDebugMessage(player.CharacterName + " Contains " + blacklistedName.Name + " Kicking for: " + blacklistedName.kickMessage);
                    player.Kick(blacklistedName.kickMessage);
                }
            }
        }
    }
}
