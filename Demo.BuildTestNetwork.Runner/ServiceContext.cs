using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Marten;
using Infrastructure.EventSourcing;
using RouteNetwork.Projections;
using RouteNetwork.QueryService;
using System.Reflection;
using ConduitNetwork.Projections;
using ConduitNetwork.QueryService;
using ConduitNetwork.Business.Specifications;
using ConduitNetwork.QueryService.ConduitClosure;
using ConduitNetwork.Projections.ConduitClosure;

namespace Demo.BuildTestNetwork
{

    public class ServiceContext
    {
        // local db
        public static string ConnectionString = "Server=localhost;Database=open-ftth;User Id=postgres;Password=postgres";

        private string _schemaName = "test";

        public ServiceContext()
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
                    options.DatabaseSchemaName = _schemaName;
                    options.Events.DatabaseSchemaName = _schemaName;

                    // projections
                    //options.Events.InlineProjections.Add(routeNodeInfoProjection);
                })));

            // Add aggreate repo
            services.AddScoped<IAggregateRepository, AggregateRepository>();

            // Route network services
            services.AddSingleton<IRouteNetworkQueryService, RouteNetworkQueryService>();
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
        }

        public ServiceProvider ServiceProvider { get; private set; }

        public IAggregateRepository AggregateRepository { get; private set; }

        public IMediator CommandBus { get; private set; }

        public IDocumentStore Store { get; private set; }

        public IDocumentSession DocumentSession
        {
            get
            {
                return ServiceProvider.GetService<IDocumentStore>().LightweightSession();
            }
        }
    }
}
