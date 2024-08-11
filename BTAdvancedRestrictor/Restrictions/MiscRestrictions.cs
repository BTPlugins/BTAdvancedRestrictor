using AdvancedRestrictor.Helpers;
using AdvancedRestrictor;
using BTAdvancedRestrictor.Helpers;
using Rocket.API.Serialisation;
using Rocket.Core;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned;
using static Rocket.Unturned.Events.UnturnedEvents;
using System.Reflection;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace BTAdvancedRestrictor.Restrictions
{
    public class MiscRestrictions
    {
        public void Init()
        {
            PlayerCrafting.onCraftBlueprintRequested += onCraftBlueprintRequested;
            U.Events.OnPlayerConnected += OnPlayerConnected;
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            foreach(SteamPending pend in Provider.pending)
            {

                var steamPendingType = typeof(SteamPending);
                var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

                var skinColorField = steamPendingType.GetField("_skin", bindingFlags);
                skinColorField.SetValue(pend, new Color(46,29,20));
            }
        }

        public void Destroy()
        {
            PlayerCrafting.onCraftBlueprintRequested -= onCraftBlueprintRequested;
            U.Events.OnPlayerConnected -= OnPlayerConnected;
        }

        private void onCraftBlueprintRequested(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(crafting.player);
            if (player.IsAdmin && AdvancedRestrictorPlugin.Instance.Configuration.Instance.IgnoreAdmins) return;
            if (AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictCraftingGlobal)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "CraftingDisabled");
                shouldAllow = false;
                return;
            }
            foreach (var Restriction in AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedCraftings)
            {
                DebugManager.SendDebugMessage("Looking at: " + Restriction.BypassPermission);
                foreach (var singleID in Restriction.Ids)
                {
                    if (itemID != singleID) // ID not found while crafting
                        continue;
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == Restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null) // Has bypass Perm
                    {
                        DebugManager.SendDebugMessage(player.CharacterName + " has Bypass Permission for " + itemID + " Crafting Ability!");
                        shouldAllow = true;
                        break;
                    }
                    shouldAllow = false;
                    string itemName = Assets.find(EAssetType.ITEM, itemID)?.FriendlyName;
                    AdvancedRestrictorPlugin.Instance.StartCoroutine(AdvancedRestrictorPlugin.Instance.sendRestrictionMessage(player, "CraftingBlacklist", itemName, Restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Crafting Prevented " + itemName + " from " + player.CharacterName + "!");
                    break;
                }
            }
        }
    }
}
