using AdvancedRestrictor;
using AdvancedRestrictor.Helpers;
using BTAdvancedRestrictor.Helpers;
using Rocket.API.Serialisation;
using Rocket.Core;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTAdvancedRestrictor.Restrictions
{
    public class VehicleRestrictions
    {
        public void Init()
        {
            VehicleManager.onDamageTireRequested += onDamageTireRequested;
            VehicleManager.onVehicleLockpicked += onVehicleLockpicked;
            VehicleManager.onEnterVehicleRequested += onEnterVehicleRequested;
            VehicleManager.onSiphonVehicleRequested += onSiphonVehicleRequested;
        }
        public void Destroy()
        {
            VehicleManager.onDamageTireRequested -= onDamageTireRequested;
            VehicleManager.onVehicleLockpicked -= onVehicleLockpicked;
            VehicleManager.onEnterVehicleRequested -= onEnterVehicleRequested;
            VehicleManager.onSiphonVehicleRequested -= onSiphonVehicleRequested;
        }

        private void onSiphonVehicleRequested(InteractableVehicle vehicle, Player instigatingPlayer, ref bool shouldAllow, ref ushort desiredAmount)
        {
            var player = UnturnedPlayer.FromPlayer(instigatingPlayer);
            if (AdvancedRestrictorPlugin.Instance.Config.VehicleOptions.RestrictSiphon && vehicle.isLocked && vehicle.lockedOwner != player.CSteamID)
            {
                TranslationHelper.SendMessageTranslation(player.CSteamID, "SiphonRestricted");
                DebugManager.SendDebugMessage(player.CharacterName + " Attempted to Siphon Gas out of " + vehicle.lockedOwner + " Vehicle. Siphon Restricted");
                shouldAllow = false;
                return;
            }
            shouldAllow = true;
        }
        private void onVehicleLockpicked(InteractableVehicle vehicle, Player instigatingPlayer, ref bool allow)
        {            
            if (vehicle == null || instigatingPlayer == null) return;
            var player = UnturnedPlayer.FromPlayer(instigatingPlayer);
            if (player == null) return;
            if (AdvancedRestrictorPlugin.Instance.Config.VehicleOptions.RestrictLockpick)
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
            var player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
            DebugManager.SendDebugMessage(player.CharacterName + " Has damaged a vehicle tire");
            if (AdvancedRestrictorPlugin.Instance.Config.VehicleOptions.RestrictTireDamage)
            {
                DebugManager.SendDebugMessage("Tire Damage Requested Prevented");
                TranslationHelper.SendMessageTranslation(player.CSteamID, "TireDamagePrevented");
                shouldAllow = false;
            }
        }
        private void onEnterVehicleRequested(Player user, InteractableVehicle vehicle, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(user);
            if (player.IsAdmin && AdvancedRestrictorPlugin.Instance.Configuration.Instance.IgnoreAdmins) return;
            foreach (var Restriction in AdvancedRestrictorPlugin.Instance.Configuration.Instance.RestrictedVehicles)
            {
                DebugManager.SendDebugMessage("Looking at: " + Restriction.BypassPermission);
                foreach (var vehicleid in Restriction.VehicleIds)
                {
                    if (vehicle.id != vehicleid) // Vehicle ID does not match
                        continue;
                    RocketPermissionsGroup? group = R.Permissions.GetGroups(player, true).Where(k => k.Permissions.FirstOrDefault(p => p.Name == Restriction.BypassPermission) != null).FirstOrDefault();
                    if (group != null) // Bypass Perm
                    {
                        shouldAllow = true;
                        break;
                    }
                    shouldAllow = false;
                    string vehicleName = Assets.find(EAssetType.VEHICLE, vehicle.id)?.FriendlyName;
                    AdvancedRestrictorPlugin.Instance.StartCoroutine(AdvancedRestrictorPlugin.Instance.sendRestrictionMessage(player, "EnterVehicle", vehicleName, Restriction.BypassPermission));
                    DebugManager.SendDebugMessage("Vehicle Enter Prevented " + vehicle.name + " from " + player.CharacterName + "!");
                    break;
                }
            }
        }
    }
}
