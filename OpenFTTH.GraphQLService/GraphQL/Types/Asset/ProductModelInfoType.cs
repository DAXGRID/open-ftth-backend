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
    public class ProductModelInfoType : ObjectGraphType<ProductModelInfo>
    {
        public ProductModelInfoType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "Product model info";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");
            Field(x => x.Name, type: typeof(IdGraphType)).Description("Name of the product model");
            Field(x => x.Manufacturer, type: typeof(ManufacturerInfoType)).Description("Manufacturer that this product model belongs to");
        }
    }
}
