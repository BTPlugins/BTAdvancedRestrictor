using AdvancedRestrictor.Modules;
using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AdvancedRestrictor
{
    public class AdvancedRestrictorConfiguration : IRocketPluginConfiguration
    {
        public float WarningMessageCooldown { get; set; }
        public bool IgnoreAdmins { get; set; }
        public bool PreventPickup { get; set; }
        public bool RestrictCraftingGlobal { get; set; }
        [XmlArrayItem(ElementName = "Restrict")]

        public List<ItemRestrict> RestrictedItems { get; set; }
        [XmlArrayItem(ElementName = "Restrict")] 

        public List<VehicleRestrict> RestrictedVehicles { get; set; }
        [XmlArrayItem(ElementName = "Restrict")]

        public List<CraftingRestrict> RestrictedCraftings { get; set; }
        public bool DebugMode { get; set; }
        public void LoadDefaults()
        {
            WarningMessageCooldown = 2f;
            IgnoreAdmins = false; // True - Admins Bypass | False - Admins act like players
            PreventPickup = false;
            RestrictCraftingGlobal = false;
            RestrictedItems = new List<ItemRestrict>()
            {
                new ItemRestrict()
                {
                    BypassPermission = "Logs.Bypass",
                    ItemIds = new List<ushort>()
                    {
                        37,
                        39,
                    }
                },
                new ItemRestrict()
                {
                    BypassPermission = "Maplestrike.Bypass",
                    ItemIds = new List<ushort>()
                    {
                        363,
                    }
                }
            };
            RestrictedVehicles = new List<VehicleRestrict>()
            {
                new VehicleRestrict()
                {
                    BypassPermission = "Jet.Bypass",
                    VehicleIds = new List<ushort>()
                    {
                        140,
                    }
                },
                new VehicleRestrict()
                {
                    BypassPermission = "Jetski.Bypass",
                    VehicleIds = new List<ushort>()
                    {
                        98,
                        99,
                        100,
                        101,
                        102,
                        103,
                        104,
                        105,
                    }
                }
            };
            RestrictedCraftings = new List<CraftingRestrict>()
            {
                new CraftingRestrict()
                {
                    BypassPermission = "PlateCrafting.Bypass",
                    Ids = new List<ushort>()
                    {
                        1092,
                        1091,
                    }
                },
                new CraftingRestrict()
                {
                    BypassPermission = "Splint.Bypass",
                    Ids = new List<ushort>()
                    {
                        96,
                    }
                },
            };
            DebugMode = false;
        }
    }
}
