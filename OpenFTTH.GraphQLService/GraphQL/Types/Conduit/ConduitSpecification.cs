using ConduitNetwork.Business.Specifications;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ConduitSpecificationType : ObjectGraphType<ConduitSpecification>
    {

        public ConduitSpecificationType(IRouteNetworkQueryService routeNetworkQueryService, IConduitNetworkQueryService conduitNetworkEqueryService, IConduitSpecificationRepository conduitSpecificationRepository, IDataLoaderContextAccessor dataLoader)
        {

            Description = "A specification of a conduit, that might be shared among manufacturer product models.";

            Field<ConduitKindEnumType>("Kind", "Kind of conduit (multi or single conduit)");
            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");
            Field(x => x.SequenceNumber, type: typeof(IdGraphType)).Description("The position of the conduit inside a multi conduit. Field only populated on inner conduits (conduits inside a multi conduit)");
            Field<ConduitShapeKindEnumType>("Shape", "Shape of conduit - flat, round etc.");
            Field<ConduitColorEnumType>("Color", "Color of the conduit itself");
            Field(x => x.InnerDiameter, type: typeof(IdGraphType)).Description("Inner diameter of the conduit");
            Field(x => x.OuterDiameter, type: typeof(IdGraphType)).Description("Outer diameter of the conduit");

            Field(x => x.ProductModels, type: typeof(ListGraphType<ProductModelInfoType>)).Description("Product models that use this specification");

            Field(x => x.ChildSpecifications, type: typeof(ListGraphType<ConduitSpecificationType>)).Description("Product models that use this specification");
        }
    }
}
