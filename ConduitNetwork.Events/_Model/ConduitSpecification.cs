using Asset.Model;
using ConduitNetwork.Events.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ConduitNetwork.Events.Model
{
    public class ConduitSpecification
    {
        public Guid Id { get; set; }
        public int SequenceNumber { get; set; }
        public ConduitKindEnum Kind { get; set; }
        public ConduitShapeKindEnum Shape { get; set; }
        public ConduitColorEnum Color { get; set; }
        public int OuterDiameter { get; set; }
        public int InnerDiameter { get; set; }
        public List<ConduitSpecification> ChildSpecifications { get; set; }
        public List<ProductModelInfo> ProductModels { get; set; }
    }
}
