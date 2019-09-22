using System;
using System.Collections.Generic;
using System.Text;

namespace Location.Model
{
    public class AccessAddressInfo
    {
        public Guid ExternalId { get; set; }
        public string MunicipalCode { get; set; }
        public string MunicipalRoadCode { get; set; }
        public string PostalCode { get; set; }
        public string PostalName { get; set; }
        public string StreetName { get; set; }
        public string HouseNumber { get; set; }
    }
}
