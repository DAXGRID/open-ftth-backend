using System;
using System.Collections.Generic;
using System.Text;

namespace Location.Model
{
    public class UnitAddressInfo
    {
        public Guid ExternalId { get; set; }
        public string FloorName { get; set; }
        public string RoomName { get; set; }
    }
}
