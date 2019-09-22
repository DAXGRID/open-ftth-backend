using Asset.Model;
using ConduitNetwork.QueryService;
using EquipmentService.GraphQL.Types;
using GraphQL;
using GraphQL.Types;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.ConduitClosure
{
    public class Mutations : ObjectGraphType
    {
        public Mutations()
        {
            Description = "Route GraphQL object for sending command/mutations to the various underlying services";

            Field<ConduitServiceCommandHandler>("conduitService", resolve: context => new { });

            Field<DemoNetwork>("demoNetwork", resolve: context => new { });
        }
    }
}
