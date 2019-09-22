using System;
using System.Collections.Generic;
using System.Text;
using Alba;
using GraphQL;
using GraphQL.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Demo.GraphQL.Tests
{
    public abstract class GraphQLAssertion : IScenarioAssertion
    {
        public abstract void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex);

        protected ExecutionResult CreateQueryResult(string result, ExecutionErrors errors = null)
        {
            object data = null;
            if (!string.IsNullOrWhiteSpace(result))
            {
                data = JObject.Parse(result);
            }

            return new ExecutionResult { Data = data, Errors = errors };
        }
    }

    public class GraphQLExpectations
    {
        private readonly Scenario _scenario;

        public GraphQLExpectations(Scenario scenario)
        {
            _scenario = scenario;
        }

        public GraphQLExpectations ShouldBeSuccess(string result, bool ignoreExtensions = true)
        {
            _scenario.AssertThat(new SuccessResultAssertion(result, ignoreExtensions));
            return this;
        }

        public GraphQLExpectations ShouldContain(string contain, bool ignoreExtensions = true)
        {
            _scenario.AssertThat(new SuccessResultContainsAssertion(contain, ignoreExtensions));
            return this;
        }
    }


    public static class ScenarioExtensions
    {
        public static GraphQLExpectations GraphQL(this Scenario scenario)
        {
            return new GraphQLExpectations(scenario);
        }
    }

    public class SuccessResultAssertion : GraphQLAssertion
    {
        private readonly string _result;
        private readonly bool _ignoreExtensions;

        public SuccessResultAssertion(string result, bool ignoreExtensions)
        {
            _result = result;
            _ignoreExtensions = ignoreExtensions;
        }

        public override void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex)
        {
            var writer = new DocumentWriter();
            var expectedResult = writer.WriteToStringAsync(CreateQueryResult(_result)).GetAwaiter().GetResult();

            var body = ex.ReadBody(context);

            if (!body.Equals(expectedResult))
            {
                if (_ignoreExtensions)
                {
                    var json = JObject.Parse(body);
                    json.Remove("extensions");
                    var bodyWithoutExtensions = json.ToString(Newtonsoft.Json.Formatting.None);
                    if (bodyWithoutExtensions.Equals(expectedResult))
                        return;
                }

                ex.Add($"Expected '{expectedResult}' but got '{body}'");
            }
        }
    }

    public class SuccessResultContainsAssertion : GraphQLAssertion
    {
        private readonly string _result;
        private readonly bool _ignoreExtensions;

        public SuccessResultContainsAssertion(string contains, bool ignoreExtensions)
        {
            _result = contains;
            _ignoreExtensions = ignoreExtensions;
        }

        public override void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex)
        {
            var body = ex.ReadBody(context);

            if (!body.Contains(_result))
                ex.Add($"Expected to find '{_result}' but got '{body}'");
            
        }
    }

}
