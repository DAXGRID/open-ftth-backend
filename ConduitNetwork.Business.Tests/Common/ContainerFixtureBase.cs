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
using ConduitNetwork.Projections.ConduitClosure;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.Business.Specifications;
using RouteNetwork.Business.Aggregates;

namespace ConduitNetwork.Business.Tests.Common
{
    public class ContainerFixtureBase
    {
        public static string ConnectionString = "Server=localhost;Database=open-ftth;User Id=postgres;Password=postgres";

        private string _schemaName = "ConduitTest";

        public ContainerFixtureBase(string schemaName)
        {
            _schemaName = schemaName;

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
                options.DatabaseSchemaName = _schemaName;
                options.Events.DatabaseSchemaName = _schemaName;
            })));

            // Add aggreate repo
            services.AddScoped<IAggregateRepository, AggregateRepository>();
            
            // Add route network aggregate
            services.AddScoped<RouteNetworkAggregate, RouteNetworkAggregate>();


            // Route network services
            services.AddSingleton<IRouteNetworkState, RouteNetworkState>();
            services.AddScoped<RouteNodeInfoProjection, RouteNodeInfoProjection>();
            services.AddScoped<RouteSegmentInfoProjection, RouteSegmentInfoProjection>();
            services.AddScoped<WalkOfInterestInfoProjection, WalkOfInterestInfoProjection>();

            // Conduit network repositories
            services.AddSingleton<IConduitClosureRepository, ConduitClosureRepository>();
            services.AddSingleton<IConduitSpecificationRepository, ConduitSpecificationRepository>();

            // Conduit network services
            services.AddSingleton<IConduitNetworkQueryService, ConduitNetworkQueryService>();


            // Conduit network projections
            services.AddScoped<MultiConduitInfoProjection, MultiConduitInfoProjection>();
            services.AddScoped<SingleConduitInfoProjection, SingleConduitInfoProjection>();
            services.AddScoped<ConduitClosureLifecyleEventProjection, ConduitClosureLifecyleEventProjection>();
            services.AddScoped<ConduitClosureAttachmentProjection, ConduitClosureAttachmentProjection>();
            services.AddScoped<ConduitClosureConduitCutAndConnectionProjection, ConduitClosureConduitCutAndConnectionProjection>();


            // Build services
            ServiceProvider = services.BuildServiceProvider();
            Store = ServiceProvider.GetService<IDocumentStore>();
            AggregateRepository = ServiceProvider.GetService<IAggregateRepository>();
            CommandBus = ServiceProvider.GetService<IMediator>();

            // Clear everything in database
            Store.Advanced.Clean.CompletelyRemoveAll();

            // Register route network projections in Marten
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<RouteNodeInfoProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<RouteSegmentInfoProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<WalkOfInterestInfoProjection>());

            // Register conduit network projections in Marten
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<MultiConduitInfoProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<SingleConduitInfoProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<ConduitClosureLifecyleEventProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<ConduitClosureAttachmentProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<ConduitClosureConduitCutAndConnectionProjection>());
        }

        public IDocumentStore Store { get; private set; }

        public ServiceProvider ServiceProvider { get; private set; }

        public IAggregateRepository AggregateRepository { get; private set; }

        public IMediator CommandBus { get; private set; }
    }
   
}
