using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.WaitStrategies;
using InfluxDB.Client;
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

        [Fact, Trait("Category", "integration")]
        public async void CanInsertIntoInflux2()
        {
            var testContainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("influxdb:2.0-alpine")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_MODE", "setup")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_USERNAME", "my-user")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_PASSWORD", "my-password")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_BUCKET", "mydb")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_ORG", "myorg")
                .WithPortBinding(8086, assignRandomHostPort: true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8086));

            await using var container = testContainersBuilder.Build();
            await container.StartAsync();
            var baseUrl = $"http://{container.Hostname}:{container.GetMappedPublicPort(8086)}";
            var options = new InfluxDBClientOptions.Builder()
                .Url(baseUrl)
                .Authenticate("my-user", "my-password".ToCharArray())
                .Bucket("mydb")
                .Org("myorg")
                .Build();

            var config = new Influx2Config(options);

            using var writer = new Influx2Writer(config, "my-pc");
            for (int attempts = 0; ; attempts++)
            {
                try
                {
                    await writer.ReportMetrics(DateTime.Now, TestSensorCreator.Values());
                    var influxDBClient = InfluxDBClientFactory.Create(options);
                    var flux = "from(bucket:\"mydb\") |> range(start: -1h)";
                    var queryApi = influxDBClient.GetQueryApi();
                    var tables = await queryApi.QueryAsync(flux, "myorg");
                    var fields = tables.SelectMany(x => x.Records).Select(x => x.GetValueByKey("identifier"));
                    Assert.Contains("/intelcpu/0/temperature/0", fields);
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
        public async void CanInsertIntoInflux2Token()
        {
            var testContainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("influxdb:2.0-alpine")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_MODE", "setup")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_USERNAME", "my-user")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_PASSWORD", "my-password")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_BUCKET", "mydb")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_ORG", "myorg")
                .WithEnvironment("DOCKER_INFLUXDB_INIT_ADMIN_TOKEN", "thisistheinfluxdbtoken")
                .WithPortBinding(8086, assignRandomHostPort: true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8086));

            await using var container = testContainersBuilder.Build();
            await container.StartAsync();

            var baseUrl = $"http://{container.Hostname}:{container.GetMappedPublicPort(8086)}";
            var configMap = new ExeConfigurationFileMap {ExeConfigFilename = "assets/influx2.config"};
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            config.AppSettings.Settings["influx2_address"].Value = baseUrl;
            var customConfig = new CustomConfig(config);
            var results = MetricConfig.ParseAppSettings(customConfig);

            using var writer = new Influx2Writer(results.Influx2, "my-pc");
            for (int attempts = 0; ; attempts++)
            {
                try
                {
                    await writer.ReportMetrics(DateTime.Now, TestSensorCreator.Values());
                    var influxDBClient = InfluxDBClientFactory.Create(results.Influx2.Options);
                    var flux = "from(bucket:\"mydb\") |> range(start: -1h)";
                    var queryApi = influxDBClient.GetQueryApi();
                    var tables = await queryApi.QueryAsync(flux, "myorg");
                    var fields = tables.SelectMany(x => x.Records).Select(x => x.GetValueByKey("identifier"));
                    Assert.Contains("/intelcpu/0/temperature/0", fields);
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
