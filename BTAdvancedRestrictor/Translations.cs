using Rocket.API.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedRestrictor
{
    public partial class AdvancedRestrictorPlugin
    {
        public override TranslationList DefaultTranslations => new TranslationList
        {
            {
                "ProperUsage", "[color=#FF0000]{{BTRestrictor}} [/color] [color=#F3F3F3]Proper Usage |[/color] [color=#3E65FF]{0}[/color]"
            },
            {
                "ItemBlacklisted", "[color=#FF0000]{{BTRestrictor}} [/color][color=#3E65FF] {0} Restricted![/color][color=#F3F3F3] Missing Permission: [/color][color=#3E65FF] {1}[/color]"
            },
            {
                "PreventPickup", "[color=#FF0000]{{BTRestrictor}} [/color][color=#3E65FF]{0} Prevent Pickup![/color][color=#F3F3F3] Missing Permission: [/color][color=#3E65FF] {1}[/color]"
            },
            {
                "EnterVehicle", "[color=#FF0000]{{BTRestrictor}} [/color][color=#3E65FF]{0} Vehicle Restricted![/color][color=#F3F3F3] Missing Permission: [/color][color=#3E65FF] {1}[/color]"
            },
            {
                "CraftingDisabled", "[color=#FF0000]{{BTRestrictor}} [/color][color=#F3F3F3]Global Crafting [/color][color=#3E65FF]Disabled![/color]"
            },
            {
                "CraftingBlacklist", "[color=#FF0000]{{BTRestrictor}} [/color][color=#3E65FF]{0} Crafting Restriction![/color][color=#F3F3F3] Missing Permission: [/color][color=#3E65FF] {1}[/color]"
            },
            {
                "LockpickPrevented", "[color=#FF0000]{{BTRestrictor}} [/color][color=#F3F3F3]Unable to Lockpick Vehicles[/color]"
            },
            {
                "TireDamagePrevented", "[color=#FF0000]{{BTRestrictor}} [/color][color=#F3F3F3]Unable to Damage tires[/color]"
            },
            {
                "SiphonRestricted", "[color=#FF0000]{{BTRestrictor}} [/color][color=#F3F3F3]Unable to Siphon Gas out of Vehicles[/color]"
            },
            {
                "MessageRestricted", "[color=#FF0000]{{BTRestrictor}} [/color][color=#F3F3F3]Blacklisted Word[/color] [color=#3E65FF]{0}[/color][color=#F3F3F3]! Message Restricted![/color]"
            },
        };
    }
}