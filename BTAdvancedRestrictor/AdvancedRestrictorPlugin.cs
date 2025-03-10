﻿using Rocket.API;
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
using HarmonyLib;

namespace AdvancedRestrictor
{
    public partial class AdvancedRestrictorPlugin : RocketPlugin<AdvancedRestrictorConfiguration>
    {
        public static AdvancedRestrictorPlugin Instance;
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

            var harmony = new Harmony("BTAdvancedRestrictions");
            harmony.PatchAll();

            UnturnedPlayerEvents.OnPlayerInventoryAdded += OnPlayerInventoryAdded;
            ItemManager.onTakeItemRequested += onTakeItemRequested;
            VehicleManager.onEnterVehicleRequested += onEnterVehicleRequested;
            PlayerCrafting.onCraftBlueprintRequested += onCraftBlueprintRequested;
            VehicleManager.onDamageTireRequested += onDamageTireRequested;
            VehicleManager.onVehicleLockpicked += onVehicleLockpicked;
            U.Events.OnPlayerConnected += OnPlayerConnected;
            VehicleManager.onSiphonVehicleRequested += onSiphonVehicleRequested;
            UnturnedPlayerEvents.OnPlayerChatted += OnPlayerChatted;
            UnturnedPlayerEvents.OnPlayerWear += OnPlayerWear;
        }
            private void OnPlayerChatted(UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        {
            if (message.StartsWith("/")) return;
            if (cancel) return;
            bool cancelMessage = false;
            foreach(var blacklistedWord in Instance.Configuration.Instance.RestrictedWords)
            {
                if (message.ToLower().Contains(blacklistedWord.Name.ToLower()))
                {
                    DebugManager.SendDebugMessage("Restricted Word: " + blacklistedWord.Name + " From " + player.CharacterName);
                    TranslationHelper.SendMessageTranslation(player.CSteamID, "MessageRestricted", blacklistedWord.Name);
                    cancel = true;
                    cancelMessage = true;
                    break;
                }
            }
            if (cancelMessage) return;
            cancel = false;
        }

        private void onSiphonVehicleRequested(InteractableVehicle vehicle, Player instigatingPlayer, ref bool shouldAllow, ref ushort desiredAmount)
        {
            if (!shouldAllow) return;
            var player = UnturnedPlayer.FromPlayer(instigatingPlayer);
            if (Instance.Configuration.Instance.VehicleOptions.RestrictSiphon && vehicle.isLocked && vehicle.lockedOwner != player.CSteamID)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "SiphonRestricted");
                DebugManager.SendDebugMessage(player.CharacterName + " Attempted to Siphon Gas out of " + vehicle.lockedOwner + " Vehicle. Siphon Restricted");
                shouldAllow = false;
                return;
            }
            shouldAllow = true;
        }
        private void OnPlayerConnected(UnturnedPlayer player)
        {
            if (player.CharacterName.Contains("<#") && Instance.Configuration.Instance.FakeColoredNames.RestrictFakeColoredNames)
            {
                DebugManager.SendDebugMessage(player.CharacterName + " Contains <#HexCode> in their Name. Kicking");
                player.Kick(Instance.Configuration.Instance.FakeColoredNames.KickMessage);
                return;
            }
            foreach(var blacklistedName in Instance.Configuration.Instance.RestrictedNames)
            {
                DebugManager.SendDebugMessage("Checking " + blacklistedName.Name + " for " + player.CharacterName);
                if (player.CharacterName.Contains(blacklistedName.Name))
                {
                    DebugManager.SendDebugMessage(player.CharacterName + " Contains " + blacklistedName.Name + " Kicking for: " + blacklistedName.kickMessage);
                    player.Kick(blacklistedName.kickMessage);
                }
            }

        }
        private void onVehicleLockpicked(InteractableVehicle vehicle, Player instigatingPlayer, ref bool allow)
        {
            if (!allow) return;
            DebugManager.SendDebugMessage("VehicleLockpick Event Triggerd");
            if (vehicle == null) return;
            DebugManager.SendDebugMessage("VehicleLockpick - Checking if Instiagiator is null");
            if (instigatingPlayer == null) return;
            var player = UnturnedPlayer.FromPlayer(instigatingPlayer);
            DebugManager.SendDebugMessage("VehicleLockPick - Checking if player is null");
            if (player == null) return;
            DebugManager.SendDebugMessage(player.CharacterName + " is lock picking a vehicle");
            if (Instance.Configuration.Instance.VehicleOptions.RestrictLockpick)
            {
                DebugManager.SendDebugMessage("Restricted Lockpick");
                TranslationHelper.SendMessageTranslation(player.CSteamID, "LockpickPrevented");
                allow = false;
                return;
            }
            allow = true;
        }

        private void onDamageTireRequested(CSteamID instigatorSteamID, InteractableVehicle vehicle, int tireIndex, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (!shouldAllow) return;
            var player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
            DebugManager.SendDebugMessage(player.CharacterName + " Has damaged a vehicle tire");
            if (Instance.Configuration.Instance.VehicleOptions.RestrictTireDamage)
            {
                DebugManager.SendDebugMessage("Tire Damage Requested Prevented");
                TranslationHelper.SendMessageTranslation(player.CSteamID, "TireDamagePrevented");
                shouldAllow = false;
            }
            
        }

        private void onCraftBlueprintRequested(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool shouldAllow)
        {
            if (!shouldAllow) return;
            bool shouldBreak = false;
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(crafting.player);
            if (player.IsAdmin && AdvancedRestrictorPlugin.Instance.Configuration.Instance.IgnoreAdmins) return;
            if (AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictCraftingGlobal)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "CraftingDisabled");
                shouldAllow = false;
                return;
            }
            var CraftingRestrictions = AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedCraftings;
            foreach (var Restriction in CraftingRestrictions)
            {
                if(shouldBreak) break;
                DebugManager.SendDebugMessage("Looking at: " + Restriction.BypassPermission);
                var CraftingIds = Restriction.Ids;
                foreach (var singleID in CraftingIds)
                {
                    if (itemID != singleID)
                    {
                        DebugManager.SendDebugMessage(itemID + " is not found in " + player.CharacterName + ".Skipping!");
                        continue;
                    }
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == Restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null)
                    {
                        DebugManager.SendDebugMessage(player.CharacterName + " has Bypass Permission for " + itemID + " Crafting Ability!");
                        shouldAllow = true;
                        // They have Bypass Perm
                        break;
                    }
                    string itemName = Assets.find(EAssetType.ITEM, itemID)?.FriendlyName;
                    StartCoroutine(sendRestrictionMessage(player, "CraftingBlacklist", itemName, Restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Crafting Prevented " + itemName + " from " + player.CharacterName + "!");

                    shouldAllow = false;
                    shouldBreak = true;
                    break;
                }
            }
        }

        private void onEnterVehicleRequested(Player user, InteractableVehicle vehicle, ref bool shouldAllow)
        {
            if (!shouldAllow) return;
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(user);
            if (player.IsAdmin && AdvancedRestrictorPlugin.Instance.Configuration.Instance.IgnoreAdmins) return;
            var VehicleRestrictions = AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedVehicles;
            foreach (var Restriction in VehicleRestrictions)
            {
                DebugManager.SendDebugMessage("Looking at: " + Restriction.BypassPermission);
                var Vehicles = Restriction.VehicleIds;
                foreach (var vehicleid in Vehicles)
                {
                    if (vehicle.id != vehicleid)
                    {
                        DebugManager.SendDebugMessage(vehicle.id + " is not found in " + player.CharacterName + ". Skipping!");
                        continue;
                    }
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == Restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null)
                    {
                        DebugManager.SendDebugMessage(player.CharacterName + " has Bypass Permission for " + vehicle.id + " Vehicle!");
                        shouldAllow = true;
                        // They have Bypass Perm
                        break;
                    }
                    shouldAllow = false;
                    string vehicleName = Assets.find(EAssetType.VEHICLE, vehicle.id)?.FriendlyName;
                    StartCoroutine(sendRestrictionMessage(player, "EnterVehicle", vehicleName, Restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Vehicle Enter Prevented " + vehicle.name + " from " + player.CharacterName + "!");
                    break;
                }
            }
        }

        private void onTakeItemRequested(Player user, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            if (!shouldAllow) return;
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(user);
            if (!AdvancedRestrictorPlugin.Instance.Configuration.Instance.ItemOptions.LeaveItemsOnGround) return;
            DebugManager.SendDebugMessage("Pickup Requested");
            if (player.IsAdmin && AdvancedRestrictorPlugin.Instance.Configuration.Instance.IgnoreAdmins) return;
            var ItemIDAdded = itemData.item.id;
            var Restrictions = AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedItems;
            foreach (var Restriction in Restrictions)
            {
                DebugManager.SendDebugMessage("Looking at: " + Restriction.BypassPermission);
                var Items = Restriction.ItemIds;
                foreach (var Item in Items)
                {
                    if (ItemIDAdded != Item)
                    {
                        DebugManager.SendDebugMessage(ItemIDAdded + " is not found in " + player.CharacterName + " inventory. Skipping!");
                        continue;
                    }
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == Restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null)
                    {
                        DebugManager.SendDebugMessage(player.CharacterName + " has Bypass Permission for " + ItemIDAdded + "!");
                        shouldAllow = true;
                        // They have Bypass Perm
                        break;
                    }
                    shouldAllow = false;
                    string itemName = Assets.find(EAssetType.ITEM, ItemIDAdded)?.FriendlyName;
                    StartCoroutine(sendRestrictionMessage(player, "PreventPickup", itemName, Restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Prevented Pickup" + itemName + " from " + player.CharacterName + "!");
                    break;
                }
            }
        }
        private void OnPlayerWear(UnturnedPlayer player, UnturnedPlayerEvents.Wearables wear, ushort id, byte? quality)
        {
            if (AdvancedRestrictorPlugin.Instance.Configuration.Instance.ItemOptions.LeaveItemsOnGround) return;
            DebugManager.SendDebugMessage("Item Added into Inventory");
            if (player.IsAdmin && AdvancedRestrictorPlugin.Instance.Configuration.Instance.IgnoreAdmins) return;
            foreach(var restriction in AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedItems)
            {
                var Items = restriction.ItemIds;
                foreach (var Item in Items)
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
                            StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearBackpack(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Glasses:
                            StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearGlasses(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Hat:
                            StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearHat(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Mask:
                            StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearMask(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Pants:
                            StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearPants(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Shirt:
                            StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearShirt(0, 0, new byte[0], true)));
                            break;
                        case UnturnedPlayerEvents.Wearables.Vest:
                            StartCoroutine(InvokeOnNextFrame(() =>
                            player.Player.clothing.askWearVest(0, 0, new byte[0], true)));
                            break;
                    }
                    string itemName = Assets.find(EAssetType.ITEM, id)?.FriendlyName;
                    StartCoroutine(sendRestrictionMessage(player, "ItemBlacklisted", itemName, restriction.BypassPermission));
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
        private void OnPlayerInventoryAdded(UnturnedPlayer player, InventoryGroup inventoryGroup, byte inventoryIndex, ItemJar P)
        {
            if (AdvancedRestrictorPlugin.Instance.Configuration.Instance.ItemOptions.LeaveItemsOnGround) return;
            DebugManager.SendDebugMessage("Item Added into Inventory");
            if (player.IsAdmin && AdvancedRestrictorPlugin.Instance.Configuration.Instance.IgnoreAdmins) return;
            var ItemIDAdded = P.item.id;
            var Restrictions = AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedItems;
            foreach(var Restriction in Restrictions)
            {
                DebugManager.SendDebugMessage("Looking at: " + Restriction.BypassPermission);
                var Items = Restriction.ItemIds;
                foreach(var Item in Items)
                {
                    if(ItemIDAdded != Item)
                    {
                        DebugManager.SendDebugMessage(ItemIDAdded + " is not found in " + player.CharacterName + " inventory. Skipping!");
                        continue;
                    }
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == Restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null)
                    {
                        DebugManager.SendDebugMessage(player.CharacterName + " has Bypass Permission for " + ItemIDAdded + "!");
                        // They have Bypass Perm
                        break;
                    }
                    player.Inventory.removeItem((byte)inventoryGroup, inventoryIndex);
                    string itemName = Assets.find(EAssetType.ITEM, ItemIDAdded)?.FriendlyName;
                    StartCoroutine(sendRestrictionMessage(player, "ItemBlacklisted", itemName, Restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Removed " + itemName + " from " + player.CharacterName + "!");
                    break;
                }
            }
        }

        protected override void Unload()
        {
            Instance = null;

            UnturnedPlayerEvents.OnPlayerInventoryAdded -= OnPlayerInventoryAdded;
            ItemManager.onTakeItemRequested -= onTakeItemRequested;
            VehicleManager.onEnterVehicleRequested -= onEnterVehicleRequested;
            PlayerCrafting.onCraftBlueprintRequested -= onCraftBlueprintRequested;
            VehicleManager.onDamageTireRequested -= onDamageTireRequested;
            VehicleManager.onVehicleLockpicked -= onVehicleLockpicked;
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            VehicleManager.onSiphonVehicleRequested -= onSiphonVehicleRequested;
            UnturnedPlayerEvents.OnPlayerChatted -= OnPlayerChatted;
            UnturnedPlayerEvents.OnPlayerWear -= OnPlayerWear;
            base.Unload();
            Logger.Log("BTAdvancedRestrictions Unloaded :) ");
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