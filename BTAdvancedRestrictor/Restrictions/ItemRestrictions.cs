using AdvancedRestrictor;
using BTAdvancedRestrictor.Helpers;
using Rocket.API.Serialisation;
using Rocket.Core;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Rocket.Unturned.Events.UnturnedPlayerEvents;

namespace BTAdvancedRestrictor.Restrictions
{
    public class ItemRestrictions
    {
        public void Init()
        {
            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnPlayerInventoryAdded;
            ItemManager.onTakeItemRequested += onTakeItemRequested;
            UnturnedPlayerEvents.OnPlayerWear += OnPlayerWear;
        }
        public void Destroy()
        {
            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnPlayerInventoryAdded;
            ItemManager.onTakeItemRequested -= onTakeItemRequested;
            UnturnedPlayerEvents.OnPlayerWear -= OnPlayerWear;
        }

        private void onTakeItemRequested(Player user, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            if (!AdvancedRestrictorPlugin.Instance.Configuration.Instance.ItemOptions.LeaveItemsOnGround) return;
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(user);
            if (player.IsAdmin && AdvancedRestrictorPlugin.Instance.Configuration.Instance.IgnoreAdmins) return;
            var ItemIDAdded = itemData.item.id;
            foreach (var Restriction in AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedItems)
            {
                foreach (var Item in Restriction.ItemIds)
                {
                    if (ItemIDAdded != Item) // Item not in Inventory
                        continue;
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == Restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null) // They have the bypass Perm
                    {
                        shouldAllow = true;
                        break;
                    }
                    shouldAllow = false;
                    string itemName = Assets.find(EAssetType.ITEM, ItemIDAdded)?.FriendlyName;
                    AdvancedRestrictorPlugin.Instance.StartCoroutine(AdvancedRestrictorPlugin.Instance.sendRestrictionMessage(player, "PreventPickup", itemName, Restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Prevented Pickup" + itemName + " from " + player.CharacterName + "!");
                    break;
                }
            }
        }
        private void OnPlayerInventoryAdded(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            if (AdvancedRestrictorPlugin.Instance.Config.ItemOptions.LeaveItemsOnGround || player.IsAdmin && AdvancedRestrictorPlugin.Instance.Config.IgnoreAdmins) return;
            foreach (var Restriction in AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedItems)
            {
                DebugManager.SendDebugMessage("Looking at: " + Restriction.BypassPermission); 
                foreach (var Item in Restriction.ItemIds)
                {
                    if (P.item.id != Item) // Item not in inventory
                        continue;
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == Restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null) // Has bypass Perm
                        break;
                    player.Inventory.removeItem((byte)inventoryGroup, inventoryIndex);
                    string itemName = Assets.find(EAssetType.ITEM, P.item.id)?.FriendlyName;
                    AdvancedRestrictorPlugin.Instance.StartCoroutine(AdvancedRestrictorPlugin.Instance.sendRestrictionMessage(player, "ItemBlacklisted", itemName, Restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Removed " + itemName + " from " + player.CharacterName + "!");
                    break;
                }
            }
        }
        private void OnPlayerWear(UnturnedPlayer player, UnturnedPlayerEvents.Wearables wear, ushort id, byte? quality)
        {
            if (AdvancedRestrictorPlugin.Instance.Configuration.Instance.ItemOptions.LeaveItemsOnGround || player.IsAdmin && AdvancedRestrictorPlugin.Instance.Configuration.Instance.IgnoreAdmins) return;
            foreach (var restriction in AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedItems)
            {
                foreach (var Item in restriction.ItemIds)
                {
                    if (id != Item)
                    {
                        DebugManager.SendDebugMessage(id + " is not found in " + player.CharacterName + " inventory. Skipping!");
                        continue;
                    }
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null)
                    {
                        DebugManager.SendDebugMessage(player.CharacterName + " has Bypass Permission for " + id + "!");
                        // They have Bypass Perm
                        break;
                    }
                    switch (wear)
                    {
                        case UnturnedPlayerEvents.Wearables.Backpack:
                            AdvancedRestrictorPlugin.Instance.StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearBackpack(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Glasses:
                            AdvancedRestrictorPlugin.Instance.StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearGlasses(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Hat:
                            AdvancedRestrictorPlugin.Instance.StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearHat(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Mask:
                            AdvancedRestrictorPlugin.Instance.StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearMask(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Pants:
                            AdvancedRestrictorPlugin.Instance.StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearPants(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Shirt:
                            AdvancedRestrictorPlugin.Instance.StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearShirt(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Vest:
                            AdvancedRestrictorPlugin.Instance.StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearVest(0, 0, new byte[0], true)));
                            break;
                    }
                    string itemName = Assets.find(EAssetType.ITEM, id)?.FriendlyName;
                    AdvancedRestrictorPlugin.Instance.StartCoroutine(AdvancedRestrictorPlugin.Instance.sendRestrictionMessage(player, "ItemBlacklisted", itemName, restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Removed " + itemName + " from " + player.CharacterName + "!");
                    break;
                }
            }
        }
        private IEnumerator InvokeOnNextFrame(System.Action action)
        {
            yield return new WaitForFixedUpdate();
            action();
        }
    }
}
