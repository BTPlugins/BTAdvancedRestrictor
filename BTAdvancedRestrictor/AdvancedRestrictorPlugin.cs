using Rocket.API;
using Rocket.Core.Plugins;
using System;
using Logger = Rocket.Core.Logging.Logger;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Rocket.Unturned.Enumerations;
using AdvancedRestrictor.Helpers;
using UnityEngine;
using System.Collections;
using Steamworks;
using Rocket.Core;
using Rocket.API.Serialisation;
using BTAdvancedRestrictor.Helpers;
using Rocket.Unturned;
using static Rocket.Unturned.Events.UnturnedPlayerEvents;
using BTAdvancedRestrictor.Restrictions;
using static Rocket.Unturned.Events.UnturnedEvents;

namespace AdvancedRestrictor
{
    public partial class AdvancedRestrictorPlugin : RocketPlugin<AdvancedRestrictorConfiguration>
    {
        public static AdvancedRestrictorPlugin Instance;
        public HashSet<CSteamID> Cooldowns = new HashSet<CSteamID>();
        public AdvancedRestrictorConfiguration Config => Configuration.Instance;

        public ItemRestrictions ItemRes {  get; set; }
        public VehicleRestrictions VehicleRes { get; set; }
        public WordRestrictions WordRes { get; set; }
        public MiscRestrictions MiscRes { get; set; }
        protected override void Load()
        {
            Instance = this;
            Logger.Log("#############################################", ConsoleColor.Yellow);
            Logger.Log("###       BTAdvancedRestrictor Loaded     ###", ConsoleColor.Yellow);
            Logger.Log("###   Plugin Created By blazethrower320   ###", ConsoleColor.Yellow);
            Logger.Log("###            Join my Discord:           ###", ConsoleColor.Yellow);
            Logger.Log("###     https://discord.gg/YsaXwBSTSm     ###", ConsoleColor.Yellow);
            Logger.Log("#############################################", ConsoleColor.Yellow);
            
            ItemRes = new ItemRestrictions();
            ItemRes.Init();

            VehicleRes = new VehicleRestrictions();
            VehicleRes.Init();

            WordRes = new WordRestrictions();
            WordRes.Init();

            MiscRes = new MiscRestrictions();
            MiscRes.Init();
        }

        protected override void Unload()
        {
            Logger.Log("BTAdvancedRestrictions Unloaded :) ");
            ItemRes.Destroy();
            VehicleRes.Destroy();
            WordRes.Destroy();
            MiscRes.Destroy();
            Cooldowns = null;
            Instance = null;
            base.Unload();
        }
        public IEnumerator sendRestrictionMessage(UnturnedPlayer player, string key, params object[] placeholder)
        {
            if (Cooldowns.Contains(player.CSteamID))
            {
                DebugManager.SendDebugMessage(player.CharacterName + " Triggered a Event however is on Cooldown!");
                yield break;
            }
            TranslationHelper.SendMessageTranslation(player.CSteamID, key, placeholder);
            Cooldowns.Add(player.CSteamID);
            yield return new WaitForSeconds(AdvancedRestrictorPlugin.Instance.Configuration.Instance.ItemOptions.WarningMessageCooldown);
            Cooldowns.Remove(player.CSteamID);
        }
    }
}


// Restrictions
// - Items
// - Prevent Item Pickup
// - Enter Vehicle
// - Item Crafting
// - Disable Global Crafting