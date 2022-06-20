using Rocket.API;
using Rocket.Core.Plugins;
using System;
using Logger = Rocket.Core.Logging.Logger;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Rocket.Unturned.Enumerations;
using AdvancedRestrictor.Helpers;
using UnityEngine;
using System.Collections;
using Steamworks;

namespace AdvancedRestrictor
{
    public partial class AdvancedRestrictor : RocketPlugin<AdvancedRestrictorConfiguration>
    {
        public static AdvancedRestrictor Instance;
        public HashSet<CSteamID> Cooldowns = new HashSet<CSteamID>();
        protected override void Load()
        {
            Instance = this;
            Logger.Log("#############################################", ConsoleColor.Yellow);
            Logger.Log("###       BTAdvancedRestrictor Loaded     ###", ConsoleColor.Yellow);
            Logger.Log("###   Plugin Created By blazethrower320   ###", ConsoleColor.Yellow);
            Logger.Log("###            Join my Discord:           ###", ConsoleColor.Yellow);
            Logger.Log("###     https://discord.gg/YsaXwBSTSm     ###", ConsoleColor.Yellow);
            Logger.Log("#############################################", ConsoleColor.Yellow);
            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnPlayerInventoryAdded;
            ItemManager.onTakeItemRequested += onTakeItemRequested;
            VehicleManager.onEnterVehicleRequested += onEnterVehicleRequested;
            PlayerCrafting.onCraftBlueprintRequested += onCraftBlueprintRequested;
        }

        private void onCraftBlueprintRequested(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(crafting.player);
            if(player.IsAdmin && AdvancedRestrictor.Instance.Configuration.Instance.IgnoreAdmins)
            {
                return;
            }
            if (AdvancedRestrictor.Instance.Configuration.Instance.RestrictCraftingGlobal)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "CraftingDisabled");
                shouldAllow = false;
                return;
            }
            var CraftingRestrictions = AdvancedRestrictor.Instance.Configuration.Instance.RestrictedCraftings;
            foreach (var Restriction in CraftingRestrictions)
            {
                if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                {
                    Logger.Log("DEBUG >> Looking at: " + Restriction.BypassPermission);
                }
                var CraftingIds = Restriction.Ids;
                foreach (var singleID in CraftingIds)
                {
                    if (itemID != singleID)
                    {
                        if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                        {
                            Logger.Log("DEBUG >> " + itemID + " is not found in " + player.CharacterName + ". Skipping!");
                        }
                        continue;
                    }
                    if (player.HasPermission(Restriction.BypassPermission))
                    {
                        if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                        {
                            Logger.Log("DEBUG >> " + player.CharacterName + " has Bypass Permission for " + itemID + " Crafting Ability!");
                        }
                        shouldAllow = true;
                        // They have Bypass Perm
                        break;
                    }
                    shouldAllow = false;
                    string itemName = Assets.find(EAssetType.ITEM, itemID)?.FriendlyName;
                    sendRestrictionMessage(player, "CraftingBlacklist", itemName, Restriction.BypassPermission);
                    if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                    {
                        Logger.Log("DEBUG >> Crafting Prevented " + itemName + " from " + player.CharacterName + "!");
                    }
                    break;
                }
            }
        }

        private void onEnterVehicleRequested(Player user, InteractableVehicle vehicle, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(user);
            if (player.IsAdmin && AdvancedRestrictor.Instance.Configuration.Instance.IgnoreAdmins)
            {
                return;
            }
            var VehicleRestrictions = AdvancedRestrictor.Instance.Configuration.Instance.RestrictedVehicles;
            foreach (var Restriction in VehicleRestrictions)
            {
                if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                {
                    Logger.Log("DEBUG >> Looking at: " + Restriction.BypassPermission);
                }
                var Vehicles = Restriction.VehicleIds;
                foreach (var vehicleid in Vehicles)
                {
                    if (vehicle.id != vehicleid)
                    {
                        if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                        {
                            Logger.Log("DEBUG >> " + vehicle.id + " is not found in " + player.CharacterName + ". Skipping!");
                        }
                        continue;
                    }
                    if (player.HasPermission(Restriction.BypassPermission))
                    {
                        if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                        {
                            Logger.Log("DEBUG >> " + player.CharacterName + " has Bypass Permission for " + vehicle.id + " Vehicle!");
                        }
                        shouldAllow = true;
                        // They have Bypass Perm
                        break;
                    }
                    shouldAllow = false;
                    string itemName = Assets.find(EAssetType.ITEM, vehicle.id)?.FriendlyName;
                    sendRestrictionMessage(player, "EnterVehicle", itemName, Restriction.BypassPermission);
                    if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                    {
                        Logger.Log("DEBUG >> Vehicle Enter Prevented " + vehicle.name + " from " + player.CharacterName + "!");
                    }
                    break;
                }
            }
        }

        private void onTakeItemRequested(Player user, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(user);
            if (AdvancedRestrictor.Instance.Configuration.Instance.PreventPickup == false)
            {
                return;
            }
            if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
            {
                Logger.Log("DEBUG >> Pickup Requested");
            }
            if (player.IsAdmin && AdvancedRestrictor.Instance.Configuration.Instance.IgnoreAdmins)
            {
                return;
            }
            var ItemIDAdded = itemData.item.id;
            var Restrictions = AdvancedRestrictor.Instance.Configuration.Instance.RestrictedItems;
            foreach (var Restriction in Restrictions)
            {
                if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                {
                    Logger.Log("DEBUG >> Looking at: " + Restriction.BypassPermission);
                }
                var Items = Restriction.ItemIds;
                foreach (var Item in Items)
                {
                    if (ItemIDAdded != Item)
                    {
                        if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                        {
                            Logger.Log("DEBUG >> " + ItemIDAdded + " is not found in " + player.CharacterName + " inventory. Skipping!");
                        }
                        continue;
                    }
                    if (player.HasPermission(Restriction.BypassPermission))
                    {
                        if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                        {
                            Logger.Log("DEBUG >> " + player.CharacterName + " has Bypass Permission for " + ItemIDAdded + "!");
                        }
                        shouldAllow = true;
                        // They have Bypass Perm
                        break;
                    }
                    shouldAllow = false;
                    string itemName = Assets.find(EAssetType.ITEM, ItemIDAdded)?.FriendlyName;
                    sendRestrictionMessage(player, "PreventPickup", itemName, Restriction.BypassPermission);
                    if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                    {
                        Logger.Log("DEBUG >> Prevented Pickup" + itemName + " from " + player.CharacterName + "!");
                    }
                    Logger.Log(Item.ToString());
                    break;
                }
            }
        }

        private void OnPlayerInventoryAdded(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            if (AdvancedRestrictor.Instance.Configuration.Instance.PreventPickup)
            {
                return;
            }
            if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
            {
                Logger.Log("DEBUG >> Item Added into Inventory");
            }
            if (player.IsAdmin && AdvancedRestrictor.Instance.Configuration.Instance.IgnoreAdmins)
            {
                return;
            }
            var ItemIDAdded = P.item.id;
            var Restrictions = AdvancedRestrictor.Instance.Configuration.Instance.RestrictedItems;
            foreach(var Restriction in Restrictions)
            {
                if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                {
                    Logger.Log("DEBUG >> Looking at: " + Restriction.BypassPermission);
                }
                var Items = Restriction.ItemIds;
                foreach(var Item in Items)
                {
                    if(ItemIDAdded != Item)
                    {
                        if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                        {
                            Logger.Log("DEBUG >> " + ItemIDAdded + " is not found in " + player.CharacterName + " inventory. Skipping!");
                        }
                        continue;
                    }
                    if (player.HasPermission(Restriction.BypassPermission))
                    {
                        if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                        {
                            Logger.Log("DEBUG >> " + player.CharacterName + " has Bypass Permission for " + ItemIDAdded + "!");
                        }
                        // They have Bypass Perm
                        break;
                    }
                    player.Inventory.removeItem((byte)inventoryGroup, inventoryIndex);
                    string itemName = Assets.find(EAssetType.ITEM, P.item.id)?.FriendlyName;
                    sendRestrictionMessage(player, "ItemBlacklist", itemName, Restriction.BypassPermission);
                    if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                    {
                        Logger.Log("DEBUG >> Removed" + itemName + " from " + player.CharacterName + "!");
                    }
                    break;
                }
            }
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnPlayerInventoryAdded;
            ItemManager.onTakeItemRequested -= onTakeItemRequested;
            VehicleManager.onEnterVehicleRequested -= onEnterVehicleRequested;
            PlayerCrafting.onCraftBlueprintRequested -= onCraftBlueprintRequested;
            Logger.Log("BTAdvancedRestrictions Unloaded :) ");
        }
        public IEnumerator sendRestrictionMessage(UnturnedPlayer player, string key, params object[] placeholder)
        {
            if (Cooldowns.Contains(player.CSteamID))
            {
                if (AdvancedRestrictor.Instance.Configuration.Instance.DebugMode)
                {
                    Logger.Log("DEBUG >> " + player.CharacterName + " Triggered a Event however is on Cooldown!");
                }
                yield break;
            }
            TranslationHelper.SendMessageTranslation(player.CSteamID, key, placeholder);
            Cooldowns.Add(player.CSteamID);
            yield return new WaitForSeconds(AdvancedRestrictor.Instance.Configuration.Instance.WarningMessageCooldown);
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
