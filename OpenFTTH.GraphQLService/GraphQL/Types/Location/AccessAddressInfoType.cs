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
    public class AccessAddressInfoType : ObjectGraphType<AccessAddressInfo>
    {
        public AccessAddressInfoType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Access address info info";

            Field(x => x.ExternalId, type: typeof(IdGraphType)).Description("Guid property");
            Field(x => x.MunicipalCode, type: typeof(IdGraphType)).Description("Unique municipal code");
            Field(x => x.MunicipalRoadCode, type: typeof(IdGraphType)).Description("Unique road code with municipal");
            Field(x => x.PostalCode, type: typeof(IdGraphType)).Description("Postal code");
            Field(x => x.PostalName, type: typeof(IdGraphType)).Description("Postal area name");
            Field(x => x.StreetName, type: typeof(IdGraphType)).Description("Street name");
            Field(x => x.HouseNumber, type: typeof(IdGraphType)).Description("House number");
        }
    }
}
