using System;
using System.Collections.Generic;
using System.Text;

namespace Asset.Model
{
    public class ProductModelInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ManufacturerInfo Manufacturer { get; set; }
    }
}
