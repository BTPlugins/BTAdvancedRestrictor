using AdvancedRestrictor;
using BTAdvancedRestrictor.Helpers;
using HarmonyLib;
using Rocket.API.Serialisation;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTAdvancedRestrictor.Patches
{
    [HarmonyPatch(typeof(PlayerInventory), "ReceiveDragItem")]
    public class ReceiveDragItemPatch
    {
        [HarmonyPrefix]
        static bool Prefix(
            PlayerInventory __instance,
            byte page_0,
              byte x_0,
              byte y_0,
              byte page_1,
              byte x_1,
              byte y_1,
              byte rot_1)
        {
            var shouldAllow = true;
            var player = UnturnedPlayer.FromPlayer(__instance.player);
            if (!(page_0 == PlayerInventory.STORAGE)) return true;
            DebugManager.SendDebugMessage("Item In storage... Checking");
            var storage = player.Inventory.storage;
            if (storage == null)
                return true;

            var index = storage.items.getIndex(x_0, y_0);

            var item = storage.items.getItem(index);
            DebugManager.SendDebugMessage("Item: " + item.item.id);

            var Restrictions = AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedItems;
            foreach (var Restriction in Restrictions)
            {
                DebugManager.SendDebugMessage("Looking at: " + Restriction.BypassPermission);
                var Items = Restriction.ItemIds;
                foreach (var Item in Items)
                {
                    if (item.item.id != Item)
                    {
                        DebugManager.SendDebugMessage(item.item.id + " is not found in " + player.CharacterName + " Storage. Skipping!");
                        continue;
                    }
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == Restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null)
                    {
                        DebugManager.SendDebugMessage(player.CharacterName + " has Bypass Permission for " + item.item.id + "!");
                        shouldAllow = true;
                        // They have Bypass Perm
                        break;
                    }
                    shouldAllow = false;
                    string itemName = Assets.find(EAssetType.ITEM, item.item.id)?.FriendlyName;
                    player.Player.StartCoroutine(AdvancedRestrictorPlugin.Instance.sendRestrictionMessage(player, "PreventPickup", itemName, Restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Prevented Pickup" + itemName + " from " + player.CharacterName + "!");
                    break;
                }
            }



            DebugManager.SendDebugMessage("Should Allow: " + shouldAllow.ToString());
            return shouldAllow;
        }
    }
}
