using ConduitNetwork.QueryService;
using Demo.BuildTestNetwork.Builders;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.BuildTestNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceContext();

            RouteNetworkBuilder.Run("../../../../Demo.BuildTestNetwork/Data/", services.CommandBus);

            ConduitBuilder.Run(services.ServiceProvider.GetService<IConduitNetworkQueryService>(), services.CommandBus);

        }
    }
}
