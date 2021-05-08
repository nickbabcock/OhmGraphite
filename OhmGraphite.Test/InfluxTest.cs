using System;
using System.Net.Http;
using System.Threading;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.WaitStrategies;
using Xunit;

namespace OhmGraphite.Test
{
    public class InfluxTest
    {
        [Fact, Trait("Category", "integration")]
        public async void CanInsertIntoInflux()
        {
            var testContainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("influxdb:1.8-alpine")
                .WithEnvironment("INFLUXDB_DB", "mydb")
                .WithEnvironment("INFLUXDB_USER", "my_user")
                .WithEnvironment("INFLUXDB_USER_PASSWORD", "my_pass")
                .WithPortBinding(8086, assignRandomHostPort: true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8086));

            await using var container = testContainersBuilder.Build();
            await container.StartAsync();
            var baseUrl = $"http://{container.Hostname}:{container.GetMappedPublicPort(8086)}";
            var config = new InfluxConfig(new Uri(baseUrl), "mydb", "my_user", "my_pass");
            using var writer = new InfluxWriter(config, "my-pc");
            using var client = new HttpClient();
            for (int attempts = 0; ; attempts++)
            {
                try
                {
                    await writer.ReportMetrics(DateTime.Now, TestSensorCreator.Values());

                    var resp = await client.GetAsync(
                        $"{baseUrl}/query?pretty=true&db=mydb&q=SELECT%20*%20FROM%20Temperature");
                    Assert.True(resp.IsSuccessStatusCode);
                    var content = await resp.Content.ReadAsStringAsync();
                    Assert.Contains("/intelcpu/0/temperature/0", content);
                    break;
                }
                catch (Exception)
                {
                    if (attempts >= 10)
                    {
                        throw;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }

        [Fact, Trait("Category", "integration")]
        public async void CanInsertIntoPasswordLessInfluxdb()
        {
            var testContainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("influxdb:1.8-alpine")
                .WithEnvironment("INFLUXDB_DB", "mydb")
                .WithEnvironment("INFLUXDB_USER", "my_user")
                .WithEnvironment("INFLUXDB_HTTP_AUTH_ENABLED", "false")
                .WithPortBinding(8086, assignRandomHostPort: true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8086));

            await using var container = testContainersBuilder.Build();
            await container.StartAsync();
            var baseUrl = $"http://{container.Hostname}:{container.GetMappedPublicPort(8086)}";
            var config = new InfluxConfig(new Uri(baseUrl), "mydb", "my_user", null);
            using var writer = new InfluxWriter(config, "my-pc");
            using var client = new HttpClient();
            for (int attempts = 0; ; attempts++)
            {
                try
                {
                    await writer.ReportMetrics(DateTime.Now, TestSensorCreator.Values());

                    var resp = await client.GetAsync(
                        $"{baseUrl}/query?pretty=true&db=mydb&q=SELECT%20*%20FROM%20Temperature");
                    Assert.True(resp.IsSuccessStatusCode);
                    var content = await resp.Content.ReadAsStringAsync();
                    Assert.Contains("/intelcpu/0/temperature/0", content);
                    break;
                }
                catch (Exception)
                {
                    if (attempts >= 10)
                    {
                        throw;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}
