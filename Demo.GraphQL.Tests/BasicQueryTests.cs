using Alba;
using GraphQL.Server.Transports.AspNetCore.Common;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Demo.GraphQL.Tests
{
    public class BasicQueryTests : SystemTestBase<EquipmentService.Startup>
    {
        [Fact]
        public async Task SimpleRouteNodeQueryTest()
        {
            using (var system = SystemUnderTest.ForStartup<EquipmentService.Startup>())
            {
                await run(_ =>
                {
                    // Just try query the sp1010 cabinet node, and see if we get id back
                    var input = new GraphQLRequest
                    {
                        Query = "{ routeNode(id: \"0b2168f2-d9be-455c-a4de-e9169f000022\") { id } }"
                    };
                    _.Post.Json(input).ToUrl("/graphql");
                    _.StatusCodeShouldBe(HttpStatusCode.OK);
                    _.ContentShouldNotContain("error");
                    _.GraphQL().ShouldContain("routeNode");
                    _.GraphQL().ShouldBeSuccess(@"{""routeNode"":{""id"":""0b2168f2-d9be-455c-a4de-e9169f000022""}}");
                });
            }
        }

        [Fact]
        public async Task SimpleRouteSegmentQueryTest()
        {
            using (var system = SystemUnderTest.ForStartup<EquipmentService.Startup>())
            {
                await run(_ =>
                {
                    // Just try query the first route segment going from cabinet sp-1010 down the lærkelunden street
                    var input = new GraphQLRequest
                    {
                        Query = "{ routeSegment(id: \"b95000fb-425d-4cd3-9f45-66e8c5000017\") { id length } }"
                    };
                    _.Post.Json(input).ToUrl("/graphql");
                    _.StatusCodeShouldBe(HttpStatusCode.OK);
                    _.ContentShouldNotContain("error");
                    _.ContentShouldContain("routeSegment");
                });
            }
        }


        [Fact]
        public async Task QuerySplicePoint1010CustomersFeededTest()
        {
            using (var system = SystemUnderTest.ForStartup<EquipmentService.Startup>())
            {
                // SP-1010: The splice point cabinet feeding the street lærkelunden
                string cabinetId = "\"0b2168f2-d9be-455c-a4de-e9169f000022\"";

                // Query all inner conduit segment lines related to cabinet SP-1010
                var graphQlQuery = @"
                {
                    routeNode(id: " + cabinetId + @") {
                        relatedConduits {
                            conduitSegment {
                                line {
                                    endRouteNode {
                                        name
                                    }
                                }
                            }
                        }
                    }
                }";


                // The result should contain all customers on lærkelunden, but just test for I100013 and I100021 due to laziness
                await run(_ =>
                {
                    var input = new GraphQLRequest
                    {
                        Query = graphQlQuery
                    };
                    _.Post.Json(input).ToUrl("/graphql");
                    _.StatusCodeShouldBe(HttpStatusCode.OK);
                    _.ContentShouldNotContain("error");
                    _.GraphQL().ShouldContain("I100013"); // First customer feeded on lærkelunden 
                    _.GraphQL().ShouldContain("I100021"); // Last customer feeded on lærkelunden 
                });
            }

        }


        [Fact]
        public async Task QueryCustomerConduitLineTest()
        {
            using (var system = SystemUnderTest.ForStartup<EquipmentService.Startup>())
            {
                // Try query line of conduits connecting customer I100014 at Lærkelunden 14 to SP-1010
                string nodeId = "\"0b2168f2-d9be-455c-a4de-e9169f000033\"";

                var graphQlQuery = @"
                {
                    routeNode(id: " + nodeId + @") {
                        relatedConduits {
                            relationType

                            conduit {
                                kind
                                name
                                children { kind }
                            }

                            conduitSegment {
                                line {
                                    startRouteNode {
                                        name
                                    }
                                    endRouteNode {
                                        name
                                    }
                                }
                            }
                        }
                    }
                }";


                await run(_ =>
                {
                    var input = new GraphQLRequest
                    {
                        Query = graphQlQuery
                    };
                    _.Post.Json(input).ToUrl("/graphql");
                    _.StatusCodeShouldBe(HttpStatusCode.OK);
                    _.ContentShouldNotContain("error");
                    _.GraphQL().ShouldContain("SP-1010"); // one end should hit the cabinet feeding the customer
                    _.GraphQL().ShouldContain("I100014"); // one end must hit the customer
                });
            }
        }

        [Fact]
        public async Task QueryFromToAllNodeAndSegmentFieldsOnAllLevelsTest()
        {
            using (var system = SystemUnderTest.ForStartup<EquipmentService.Startup>())
            {
                // SP-1010: The splice point cabinet feeding the street lærkelunden
                string cabinetId = "\"0b2168f2-d9be-455c-a4de-e9169f000022\"";

                // Query fromRouteNode, toRouteNode, allRouteNodes and allRouteSegments on both conduit and conduit segment type
                var graphQlQuery = @"
                {
                    routeNode(id: " + cabinetId + @") {
                    relatedConduits(
                          includeMultiConduits: true
                          includeInnerConduits: false
                          includeSingleConduits: true
                        ) {
                          relationType
                          conduit {
                            children { 
                              color
                            }
                            fromRouteNode { name }
                            toRouteNode { name }
                            id
                            color
                          }
                        }
                    }
                }";


                // If it don't fail, the query functionality work
                await run(_ =>
                {
                    var input = new GraphQLRequest
                    {
                        Query = graphQlQuery
                    };
                    _.Post.Json(input).ToUrl("/graphql");
                    _.StatusCodeShouldBe(HttpStatusCode.OK);
                    _.ContentShouldNotContain("error");
                });
            }

        }

        [Fact]
        public async Task QueryRelatedLineSegmentsJ1010()
        {
            using (var system = SystemUnderTest.ForStartup<EquipmentService.Startup>())
            {
                // Try query line segments of J-1010
                string nodeId = "\"0b2168f2-d9be-455c-a4de-e9169f000122\"";

                var graphQlQuery = @"
                {
                    routeNode(id: " + nodeId + @") {
                        relatedSegments {
                              id
                              relationType
                              line {
                                id
                                lineKind
                                parent {
                                  id
                                }
                                fromRouteNode {
                                  name
                                }
                                toRouteNode {
                                  name
                                }
                              }

                              children {
                                id
                                relationType
                                line {
                                  lineKind
                                  parent {
                                    id
                                  }
                                }
                              }

                              parents {
                                id
                                relationType
                              }

                              traversal {
                                allSegments { id }
                              }

                              ... on ConduitSegment {
                                conduit {
                                  color
                                  position

                                  children {
                                    kind
                                  }
                                }
                              }

                              ... on FiberCableSegment {
                                fiberCable {
                                  numberOfFibers
                                }
                              }
                            }
                    }
                }";


                await run(_ =>
                {
                    var input = new GraphQLRequest
                    {
                        Query = graphQlQuery
                    };
                    _.Post.Json(input).ToUrl("/graphql");
                    _.StatusCodeShouldBe(HttpStatusCode.OK);
                    _.ContentShouldNotContain("error");
                    _.GraphQL().ShouldContain("FIBER_CABLE"); // should return fiber cable in trench
                    _.GraphQL().ShouldContain("SINGLE_CONDUIT"); // should return single conduit in trench
                    _.GraphQL().ShouldContain("MULTI_CONDUIT"); // should return multi conduit in trench
                    _.GraphQL().ShouldContain("CO-BDAL"); // a cable should go from CO-BDAL to CO-BRED
                    _.GraphQL().ShouldContain("CO-BRED"); // a cable should go from CO-BDAL to CO-BRED
                    _.GraphQL().ShouldContain("HH-BDAL-01"); // a multi conduit should go from HH-BDAL-01 to HH-5010
                    _.GraphQL().ShouldContain("HH-5010"); // a multi conduit should go from HH-BDAL-01 to HH-5010
                });
            }
        }

        [Fact]
        public async Task QueryDiagramOnJ1010()
        {
            using (var system = SystemUnderTest.ForStartup<EquipmentService.Startup>())
            {
                // Try query line segments of J-1010
                string nodeId = "\"0b2168f2-d9be-455c-a4de-e9169f000122\"";

                var graphQlQuery = @"
                query {
                    diagramService {
                        buildRouteNodeDiagram(
                          routeNodeId: " + nodeId + @") 
                          {
                            diagramObjects {
                                refClass
                                refId
                              style
                                label
                              geometry {
                                    type
                                    coordinates
                                }
                            }
                        }
                    }
                }";


                await run(_ =>
                {
                    var input = new GraphQLRequest
                    {
                        Query = graphQlQuery
                    };
                    _.Post.Json(input).ToUrl("/graphql");
                    _.StatusCodeShouldBe(HttpStatusCode.OK);
                    _.ContentShouldNotContain("error");
                    _.GraphQL().ShouldContain("ConduitClosure"); 
                    _.GraphQL().ShouldContain("MultiConduitSegment"); 
                    _.GraphQL().ShouldContain("SingleConduit"); 
                    _.GraphQL().ShouldContain("InnerConduitWhite"); 
                    _.GraphQL().ShouldContain("MultiConduitOrange"); 
                    _.GraphQL().ShouldContain("MultiConduitRed"); 
                    _.GraphQL().ShouldContain("LabelMediumText"); 
                });
            }
        }
    }
}
