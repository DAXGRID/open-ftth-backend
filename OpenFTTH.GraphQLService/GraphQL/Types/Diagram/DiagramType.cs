using ConduitNetwork.ReadModel;
using DiagramLayout.Model;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class DiagramType : ObjectGraphType<Diagram>
    {
        public DiagramType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Diagram";

            Field(x => x.DiagramObjects, type: typeof(ListGraphType<DiagramObjectType>)).Description("All diagram objects contained by the diagram.");
        }
    }
}
