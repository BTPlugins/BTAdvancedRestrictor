using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AdvancedRestrictor.Modules
{
    public class ItemOptions
    {
        public float WarningMessageCooldown {get; set;}
        public bool LeaveItemsOnGround { get; set; }
    }
    public class ItemRestrict
    {
        public string BypassPermission { get; set; }
        [XmlArrayItem("ItemID")]
        public List<ushort> ItemIds { get; set; }
    }
    public class VehicleOptions
    {
        public bool RestrictLockpick { get; set; }
        public bool RestrictTireDamage { get; set; }
        public bool RestrictSiphon { get; set; }
    }
    public class VehicleRestrict
    {
        public string BypassPermission { get; set; }
        [XmlArrayItem("VehicleID")]
        public List<ushort> VehicleIds { get; set; }
    }
    public class CraftingRestrict
    {
        public string BypassPermission { get; set; }
        [XmlArrayItem("ItemID")]
        public List<ushort> Ids { get; set; }
    }
    public class RestrictedNames
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string kickMessage { get; set; }
    }
    public class RestrictedWords
    {
        [XmlAttribute]
        public string Name { get; set; }
    }
}
