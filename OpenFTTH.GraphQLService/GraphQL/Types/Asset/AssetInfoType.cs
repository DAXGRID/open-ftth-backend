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
    public class AssetInfoType : ObjectGraphType<AssetInfo>
    {
        public AssetInfoType(IDataLoaderContextAccessor dataLoader)
        {
            Description = "General asset info";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("Guid property");
            Field(x => x.SerialNumber, type: typeof(IdGraphType)).Description("Serialnumber of the asset");
            Field(x => x.Manufacturer, type: typeof(ManufacturerInfoType)).Description("Manufacturer of the asset");
            Field(x => x.Model, type: typeof(ProductModelInfoType)).Description("Product model/type of the asset");
        }
    }
}
