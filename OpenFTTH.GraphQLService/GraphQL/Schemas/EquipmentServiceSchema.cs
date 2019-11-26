using EquipmentService.GraphQL.ConduitClosure;
using EquipmentService.GraphQL.Queries;
using EquipmentService.GraphQL.Types;
using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Schemas
{
    public class EquipmentSchema : Schema
    {
        public EquipmentSchema(IDependencyResolver resolver)
            : base(resolver)
        {
            Query = resolver.Resolve<EquipmentServiceQuery>();
            Mutation = resolver.Resolve<Mutations>();

            RegisterType<ConduitSegmentType>();
      
            RegisterType<ConduitSegment>();
            RegisterType<FiberCableSegment>();
        
        }
    }
}
