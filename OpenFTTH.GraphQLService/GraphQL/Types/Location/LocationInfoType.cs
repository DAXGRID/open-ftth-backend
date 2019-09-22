using Asset.Model;
using GraphQL.DataLoader;
using GraphQL.Types;
using Location.Model;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class LocationInfoType : ObjectGraphType<LocationInfo>
    {
        public LocationInfoType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "General location info";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");

            Field(x => x.Direction, type: typeof(IdGraphType)).Description("Direction info - i.e. basement etc.");

            Field(x => x.AccessAddress, type: typeof(AccessAddressInfoType)).Description("The street address");
            //Field(x => x.UnitAddress, type: typeof(ManufacturerInfoType)).Description("Manufacturer of the asset");
        }
    }
}
