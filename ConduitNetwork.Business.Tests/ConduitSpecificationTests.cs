using Microsoft.Extensions.DependencyInjection;
using AutoFixture;
using ConduitNetwork.Business.Commands;
using ConduitNetwork.Business.Tests.Common;
using Infrastructure.EventSourcing;
using MediatR;
using System;
using Xunit;
using ConduitNetwork.Business.Aggregates;
using System.Linq;
using RouteNetwork.Business.Commands;
using RouteNetwork.Events.Model;
using System.Collections.Generic;
using RouteNetwork.QueryService;
using ConduitNetwork.ReadModel;
using ConduitNetwork.QueryService;
using ConduitNetwork.Business.Specifications;
using ConduitNetwork.Events.Model;

namespace ConduitNetwork.Business.Tests
{
    public class ConduitSpecificationFixture : ContainerFixtureBase
    {
        public ConduitSpecificationFixture() : base("conduit_specification_tests") { }
    }

    public class ConduitSpecificationTests : IClassFixture<ConduitSpecificationFixture>
    {
        private ConduitSpecificationFixture serviceContext;

        public ConduitSpecificationTests(ConduitSpecificationFixture containerFixture)
        {
            this.serviceContext = containerFixture;
        }

        [Fact]
        public void TestRepoLookup()
        {
            var conduitSpecRepo = serviceContext.ServiceProvider.GetService<IConduitSpecificationRepository>();

            var allSpecs = conduitSpecRepo.GetConduitSpecifications();

            // Check that some multi conduit specification exists
            Assert.True(allSpecs.Count(s => s.Kind == Events.Model.ConduitKindEnum.MultiConduit) > 0);

            // Check that multi conduit spec has child specs
            Assert.True(allSpecs.Find(s => s.Kind == Events.Model.ConduitKindEnum.MultiConduit).ChildSpecifications.Count > 0);

            // Check that single conduit exists for all 12 standard colors
            for (int i = 1; i <= 12; i++)
                Assert.True(allSpecs.Exists(s => s.Kind == Events.Model.ConduitKindEnum.SingleConduit && s.Color == (ConduitColorEnum)i));

        }
    }
}
