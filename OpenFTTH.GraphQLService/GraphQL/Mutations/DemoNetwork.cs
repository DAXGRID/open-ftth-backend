using Asset.Model;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Events.Model;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.ReadModel.ConduitClosure;
using Demo.BuildTestNetwork.Builders;
using EquipmentService.GraphQL.Types;
using GraphQL;
using GraphQL.Types;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using RouteNetwork.QueryService;
using RouteNetwork.ReadModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EquipmentService.GraphQL.ConduitClosure
{
    public class DemoNetwork : ObjectGraphType
    {
        public DemoNetwork(IHostingEnvironment env, IDocumentStore documentStore, IMediator commandBus, IConduitClosureRepository conduitClosureRepository, IRouteNetworkQueryService routeNetwork, IConduitNetworkQueryService conduitNetwork)
        {
            
            Description = "API for invoking the demo/test data builder";

            Field<StringGraphType>(
              "rebuild",
              description: "Deletes the database and rebuild the demo data from the GeoJson files created using QGIS",
              resolve: context =>
              {
                  try
                  {
                      // First delete everything in the database
                      documentStore.Advanced.Clean.CompletelyRemoveAll();

                      // Clean everything in projected in-memory read models
                      routeNetwork.Clean();
                      conduitNetwork.Clean();
                      conduitClosureRepository.Clean();

                      var iisExpressFolder = AppDomain.CurrentDomain.BaseDirectory;

                      var pathToData = env.ContentRootPath;

                      if (iisExpressFolder.Contains("Debug\\netcoreapp"))
                          pathToData = iisExpressFolder;

                      pathToData += Path.DirectorySeparatorChar.ToString() + "Data" + Path.DirectorySeparatorChar.ToString();

                      // Rebuild demo data
                      RouteNetworkBuilder.Run(pathToData, commandBus);
                      ConduitBuilder.Run(conduitNetwork, commandBus);

                      return "Read models cleaned and test data was rebuilt.";

                  }
                  catch ( Exception ex )
                  {
                      context.Errors.Add(new ExecutionError(ex.Message, ex));
                      return "Failed";
                  }
              }
          );


        }
    }
}
