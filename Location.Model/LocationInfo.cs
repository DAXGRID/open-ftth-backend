using System;
using System.Collections.Generic;
using System.Text;

namespace Location.Model
{
    public class LocationInfo
    {
        public Guid Id { get; set; }
        public AccessAddressInfo AccessAddress { get; set; }
        public UnitAddressInfo UnitAddress { get; set; }
        public string Direction { get; set; }
    }
}
