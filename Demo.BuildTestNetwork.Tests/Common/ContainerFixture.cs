using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Infrastructure.EventSourcing;
using Marten;
using ConduitNetwork.Projections;
using RouteNetwork.QueryService;
using RouteNetwork.Projections;
using System.Reflection;
using ConduitNetwork.QueryService;

namespace ConduitNetwork.Business.Tests.Common
{
    public class ContainerFixture
    {
        public static string ConnectionString = "Server=localhost;Database=open-ftth;User Id=postgres;Password=postgres";

        public static string SchemaName = "test";

        public ContainerFixture()
        {
            var services = new ServiceCollection();

            // MediatR
            var routeNetworkAssembly = AppDomain.CurrentDomain.Load("RouteNetwork.Business");
            var conduitNetworkAssembly = AppDomain.CurrentDomain.Load("ConduitNetwork.Business");
            services.AddMediatR(new Assembly[] { routeNetworkAssembly, conduitNetworkAssembly });


            // Marten document store
            services.AddScoped<IDocumentStore>(provider =>
                DocumentStore.For((options =>
            {
                options.Connection(ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.DatabaseSchemaName = SchemaName;
                options.Events.DatabaseSchemaName = SchemaName;
            })));

            // Add aggreate repo
            services.AddScoped<IAggregateRepository, AggregateRepository>();

            // Route network services
            services.AddSingleton<IRouteNetworkQueryService, RouteNetworkQueryService>();
            services.AddScoped<RouteNodeInfoProjection, RouteNodeInfoProjection>();
            services.AddScoped<RouteSegmentInfoProjection, RouteSegmentInfoProjection>();
            services.AddScoped<WalkOfInterestInfoProjection, WalkOfInterestInfoProjection>();

            // Conduit network service
            services.AddSingleton<IConduitNetworkQueryService, ConduitNetworkQueryService>();
            services.AddScoped<MultiConduitInfoProjection, MultiConduitInfoProjection>();
            services.AddScoped<SingleConduitInfoProjection, SingleConduitInfoProjection>();

            // Build services
            ServiceProvider = services.BuildServiceProvider();
            Store = ServiceProvider.GetService<IDocumentStore>();
            AggregateRepository = ServiceProvider.GetService<IAggregateRepository>();
            CommandBus = ServiceProvider.GetService<IMediator>();

            // Register route network projections in Marten
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<RouteNodeInfoProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<RouteSegmentInfoProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<WalkOfInterestInfoProjection>());

            // Register conduit network projections in Marten
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<MultiConduitInfoProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<SingleConduitInfoProjection>());
        }

        public IDocumentStore Store { get; private set; }

        public ServiceProvider ServiceProvider { get; private set; }

        public IAggregateRepository AggregateRepository { get; private set; }

        public IMediator CommandBus { get; private set; }
    }
   
}
