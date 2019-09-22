using Alba;
using System;
using System.Threading.Tasks;

namespace Demo.GraphQL.Tests
{
    public class SystemTestBase<T> where T : class
    {
        protected async Task<IScenarioResult> run(Action<Scenario> configuration, Action<SystemUnderTest> systemConfigure = null)
        {
            using (var system = SystemUnderTest.ForStartup<T>())
            {
                // system.Environment.EnvironmentName = "Testing";
                systemConfigure?.Invoke(system);
                return await system.Scenario(configuration);
            }
        }
    }
}