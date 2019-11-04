using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using RouteNetwork.Business;
using Marten;
using Infrastructure.EventSourcing;
using RouteNetwork.Projections;
using RouteNetwork.QueryService;
using System.Reflection;
using RouteNetwork.Business.Aggregates;
using System.Threading;

namespace RouteNetwork.Business.Tests.Common
{
    public class ContainerFixtureBase
    {
        public static string ConnectionString = "Server=localhost;Database=open-ftth;User Id=postgres;Password=postgres";

        private string _schemaName = "EventStore";

        public ContainerFixtureBase(string schemaName)
        {
            _schemaName = schemaName;

            var services = new ServiceCollection();

            // MediatR
            var routeNetworkAssembly = AppDomain.CurrentDomain.Load("RouteNetwork.Business");
            services.AddMediatR(new Assembly[] { routeNetworkAssembly, routeNetworkAssembly });

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

            // Add route network query service implementation
            services.AddScoped<IRouteNetworkState, RouteNetworkState>();

            // Add aggreate repo
            services.AddScoped<IAggregateRepository, AggregateRepository>();

            // Add route network aggregate
            services.AddScoped<RouteNetworkAggregate, RouteNetworkAggregate>();

            // Add projections
            services.AddScoped<RouteNodeInfoProjection, RouteNodeInfoProjection>();
            services.AddScoped<RouteSegmentInfoProjection, RouteSegmentInfoProjection>();
            services.AddScoped<WalkOfInterestInfoProjection, WalkOfInterestInfoProjection>();

            ServiceProvider = services.BuildServiceProvider();

            Store = ServiceProvider.GetService<IDocumentStore>();
            AggregateRepository = ServiceProvider.GetService<IAggregateRepository>();
            CommandBus = ServiceProvider.GetService<IMediator>();

            // Clear everything
            Store.Advanced.Clean.CompletelyRemoveAll();

            // Register projections in Marten
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<RouteNodeInfoProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<RouteSegmentInfoProjection>());
            Store.Events.InlineProjections.Add(ServiceProvider.GetService<WalkOfInterestInfoProjection>());
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
