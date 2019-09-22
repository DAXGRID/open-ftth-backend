using System;

namespace Asset.Model
{
    public class AssetInfo
    {
        public Guid Id { get; set; }
        public string SerialNumber { get; set; }
        public ManufacturerInfo Manufacturer { get; set; }
        public ProductModelInfo Model { get; set; }
    }
}
