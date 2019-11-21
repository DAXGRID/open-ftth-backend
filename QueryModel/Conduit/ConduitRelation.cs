using ConduitNetwork.Events.Model;
using ConduitNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace QueryModel.Conduit
{
    public class ConduitRelation
    {
        [Required(ErrorMessage = "Relation type is required")]
        public ConduitRelationTypeEnum RelationType { get; set; }
        public ConduitInfo Conduit { get; set; }
        public ConduitSegmentInfo ConduitSegment { get; set; }

        public bool CanBeCutAtNode { get; set; }
        public bool CanBeAttachedToConduitClosure { get; set; }
    }
}
