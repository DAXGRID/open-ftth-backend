using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ConduitNetwork.Business.Specifications;
using ConduitNetwork.Projections;
using ConduitNetwork.Projections.ConduitClosure;
using ConduitNetwork.QueryService;
using ConduitNetwork.QueryService.ConduitClosure;
using EquipmentService.GraphQL.Schemas;
using GraphQL;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using Infrastructure.EventSourcing;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RouteNetwork.Projections;
using RouteNetwork.QueryService;

namespace EquipmentService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            // MediatR
            var routeNetworkAssembly = AppDomain.CurrentDomain.Load("RouteNetwork.Business");
            var conduitNetworkAssembly = AppDomain.CurrentDomain.Load("ConduitNetwork.Business");
            services.AddMediatR(new Assembly[] { routeNetworkAssembly, conduitNetworkAssembly });


            // Marten document store
            var sqlConString = Environment.GetEnvironmentVariable("OpenFtthConnectionString");

            if (sqlConString == null)
                sqlConString = Configuration.GetConnectionString("sqlConString");

            // If still null, we're probably in dev env, so use localhost
            if (sqlConString == null)
                sqlConString = "Server=localhost;Database=open-ftth;User Id=postgres;Password=postgres";

            var martenSchemaName = "test";

            services.AddSingleton<IDocumentStore>(provider =>
                DocumentStore.For((options =>
                {
                    options.Connection(sqlConString);
                    options.AutoCreateSchemaObjects = AutoCreate.All;
                    options.DatabaseSchemaName = martenSchemaName;
                    options.Events.DatabaseSchemaName = martenSchemaName;
                })));

            // Add aggreate repo
            services.AddScoped<IAggregateRepository, AggregateRepository>();

            // Route network services
            services.AddSingleton<IRouteNetworkQueryService, RouteNetworkQueryService>();
            services.AddSingleton<RouteNodeInfoProjection, RouteNodeInfoProjection>();
            services.AddSingleton<RouteSegmentInfoProjection, RouteSegmentInfoProjection>();
            services.AddSingleton<WalkOfInterestInfoProjection, WalkOfInterestInfoProjection>();

            // Conduit network repositories
            services.AddSingleton<IConduitClosureRepository, ConduitClosureRepository>();

            // Conduit network services
            services.AddSingleton<IConduitNetworkQueryService, ConduitNetworkQueryService>();
            services.AddSingleton<IConduitSpecificationRepository, ConduitSpecificationRepository>();

            // Conduit network projections
            services.AddSingleton<MultiConduitInfoProjection, MultiConduitInfoProjection>();
            services.AddSingleton<SingleConduitInfoProjection, SingleConduitInfoProjection>();
            services.AddSingleton<ConduitClosureLifecyleEventProjection, ConduitClosureLifecyleEventProjection>();
            services.AddSingleton<ConduitClosureAttachmentProjection, ConduitClosureAttachmentProjection>();
            services.AddSingleton<ConduitClosureConduitCutAndConnectionProjection, ConduitClosureConduitCutAndConnectionProjection>();


            // GraphQL stuff
            services.AddScoped<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));
            services.AddScoped<EquipmentSchema>();

            services.AddGraphQL(o => { o.ExposeExceptions = false; })
                .AddGraphTypes(ServiceLifetime.Scoped)
                .AddDataLoader();

            // Cors stuff
            services.AddCors(options => options.AddPolicy("AllowAllOrigins", builder =>
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Marten stuff
            var Store = app.ApplicationServices.GetService<IDocumentStore>();

            // Register route network projections in Marten
            Store.Events.InlineProjections.Add(app.ApplicationServices.GetService<RouteNodeInfoProjection>());
            Store.Events.InlineProjections.Add(app.ApplicationServices.GetService<RouteSegmentInfoProjection>());
            Store.Events.InlineProjections.Add(app.ApplicationServices.GetService<WalkOfInterestInfoProjection>());

            // Register conduit network projections in Marten
            Store.Events.InlineProjections.Add(app.ApplicationServices.GetService<MultiConduitInfoProjection>());
            Store.Events.InlineProjections.Add(app.ApplicationServices.GetService<SingleConduitInfoProjection>());
            Store.Events.InlineProjections.Add(app.ApplicationServices.GetService<ConduitClosureLifecyleEventProjection>());
            Store.Events.InlineProjections.Add(app.ApplicationServices.GetService<ConduitClosureAttachmentProjection>());
            Store.Events.InlineProjections.Add(app.ApplicationServices.GetService<ConduitClosureConduitCutAndConnectionProjection>());

            // HTTP stuff

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseCors("AllowAllOrigins");

            app.UseGraphQL<EquipmentSchema>();
            app.UseGraphQLPlayground(options: new GraphQLPlaygroundOptions());

            app.UseMvc();
        }
    }
}
