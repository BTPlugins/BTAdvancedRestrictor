using AdvancedRestrictor.Modules;
using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AdvancedRestrictor
{
    public class AdvancedRestrictorConfiguration : IRocketPluginConfiguration
    {
        public bool IgnoreAdmins { get; set; }
        public bool RestrictCraftingGlobal { get; set; }
        public ItemOptions ItemOptions { get; set; }
        //
        [XmlArrayItem(ElementName = "Restriction")]
        public List<ItemRestrict> RestrictedItems { get; set; }
        public VehicleOptions VehicleOptions { get; set; }
        [XmlArrayItem(ElementName = "Restriction")]
        public List<VehicleRestrict> RestrictedVehicles { get; set; }
        [XmlArrayItem(ElementName = "Restriction")]
        public List<CraftingRestrict> RestrictedCraftings { get; set; }
        [XmlArrayItem("Player")]
        public List<RestrictedNames> RestrictedNames { get; set; }
        [XmlArrayItem("Word")]
        public List<RestrictedWords> RestrictedWords { get; set; }
        public bool DebugMode { get; set; }
        public void LoadDefaults()
        {
            IgnoreAdmins = false; // True - Admins Bypass | False - Admins act like players
            RestrictCraftingGlobal = false;
            ItemOptions = new ItemOptions()
            {
                WarningMessageCooldown = 2f,
                LeaveItemsOnGround = false,
            };
            RestrictedItems = new List<ItemRestrict>()
            {
                new ItemRestrict()
                {
                    BypassPermission = "Logs.Bypass",
                    ItemIds = new List<ushort>()
                    {
                        36,
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
            VehicleOptions = new VehicleOptions()
            {
                RestrictLockpick = false,
                RestrictTireDamage = false,
                RestrictSiphon = false,
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
            RestrictedNames = new List<RestrictedNames>
            {
                new RestrictedNames()
                {
                    Name = "Playit.gg",
                    kickMessage = "Playit.gg is Prohibited in your Username",
                },
                new RestrictedNames()
                {
                    Name = "www.",
                    kickMessage = "Website URLs not allowed",
                },
            };
            RestrictedWords = new List<RestrictedWords>
            {
                new RestrictedWords()
                {
                    Name = "fuck",
                },
                new RestrictedWords()
                {
                    Name = "Teemo",
                }
            };
            DebugMode = false;
        }
    }
}
