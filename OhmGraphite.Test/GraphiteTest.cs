using System;
using System.Net.Http;
using System.Threading;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.WaitStrategies;
using Xunit;

namespace OhmGraphite.Test
{
    public class GraphiteTest
    {
        [Fact, Trait("Category", "integration")]
        public async void InsertGraphiteTest()
        {
            var testContainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithDockerEndpoint(DockerUtils.DockerEndpoint())
                .WithImage("graphiteapp/graphite-statsd:1.1.8-2")
                .WithEnvironment("REDIS_TAGDB", "y")
                .WithPortBinding(2003, assignRandomHostPort: true)
                .WithPortBinding(80, assignRandomHostPort: true)
                .WithPortBinding(8080, assignRandomHostPort: true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080));

            await using var container = testContainersBuilder.Build();
            await container.StartAsync();
            var port = container.GetMappedPublicPort(2003);
            using var writer = new GraphiteWriter(container.Hostname, port, "my-pc", tags: false);
            using var client = new HttpClient();
            for (int attempts = 0; ; attempts++)
            {
                try
                {
                    await writer.ReportMetrics(DateTime.Now, TestSensorCreator.Values());

                    var resp = await client.GetAsync(
                        $"http://{container.Hostname}:{container.GetMappedPublicPort(80)}/render?format=csv&target=ohm.my-pc.intelcpu.0.temperature.cpucore.1");
                    var content = await resp.Content.ReadAsStringAsync();
                    Assert.Contains("ohm.my-pc.intelcpu.0.temperature.cpucore.1", content);
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
        public async void InsertTagGraphiteTest()
        {
            var testContainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithDockerEndpoint(DockerUtils.DockerEndpoint())
                .WithImage("graphiteapp/graphite-statsd:1.1.8-2")
                .WithEnvironment("REDIS_TAGDB", "y")
                .WithPortBinding(2003, assignRandomHostPort: true)
                .WithPortBinding(80, assignRandomHostPort: true)
                .WithPortBinding(8080, assignRandomHostPort: true)
                                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080));

            await using var container = testContainersBuilder.Build();
            await container.StartAsync();
            var port = container.GetMappedPublicPort(2003);
            using var writer = new GraphiteWriter(container.Hostname, port, "my-pc", tags: true);
            using var client = new HttpClient();
            for (int attempts = 0;; attempts++)
            {
                try
                {
                    await writer.ReportMetrics(DateTime.Now, TestSensorCreator.Values());
                    var resp = await client.GetAsync(
                        $"http://{container.Hostname}:{container.GetMappedPublicPort(80)}/render?format=csv&target=seriesByTag('sensor_type=Temperature','hardware_type=CPU')");

                    var content = await resp.Content.ReadAsStringAsync();
                    Assert.Contains("host=my-pc", content);
                    Assert.Contains("app=ohm", content);
                    Assert.Contains("sensor_type=Temperature", content);
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
