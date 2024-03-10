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

namespace BTAdvancedRestrictor.Restrictions
{
    public class MiscRestrictions
    {
        public void Init()
        {
            PlayerCrafting.onCraftBlueprintRequested += onCraftBlueprintRequested;
        }
        public void Destroy()
        {
            PlayerCrafting.onCraftBlueprintRequested -= onCraftBlueprintRequested;
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
