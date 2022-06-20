using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AdvancedRestrictor.Modules
{
    public class ItemRestrict
    {
        public string BypassPermission { get; set; }
        public List<ushort> ItemIds { get; set; }
    }
    public class VehicleRestrict
    {
        public string BypassPermission { get; set; }
        public List<ushort> VehicleIds { get; set; }
    }
    public class CraftingRestrict
    {
        public string BypassPermission { get; set; }
        public List<ushort> Ids { get; set; }
    }
}
