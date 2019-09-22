using Asset.Model;
using GraphQL.DataLoader;
using GraphQL.Types;
using QueryModel.Conduit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.Types
{
    public class ManufacturerInfoType : ObjectGraphType<ManufacturerInfo>
    {
        public ManufacturerInfoType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Manufaturer info";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");
            Field(x => x.Name, type: typeof(IdGraphType)).Description("Name of the manufacturer");
        }
    }
}
