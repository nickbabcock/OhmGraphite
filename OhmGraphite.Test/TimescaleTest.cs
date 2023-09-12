using System;
using DotNet.Testcontainers.Builders;
using Npgsql;
using Xunit;

namespace OhmGraphite.Test
{
    public class TimescaleTest
    {
        [Fact, Trait("Category", "integration")]
        public async void CanSetupTimescale()
        {
            var testContainersBuilder = new ContainerBuilder()
                .WithDockerEndpoint(DockerUtils.DockerEndpoint())
                .WithImage("timescale/timescaledb:latest-pg12")
                .WithEnvironment("POSTGRES_PASSWORD", "123456")
                .WithPortBinding(5432, assignRandomHostPort: true)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilCommandIsCompleted("pg_isready -h 'localhost' -p '5432'"));

            await using var container = testContainersBuilder.Build();
            await container.StartAsync();

            string connStr = $"Host={container.Hostname};Username=postgres;Password=123456;Port={container.GetMappedPublicPort(5432)}";
            var epoch = new DateTime(2001, 1, 13, 0, 0, 0, DateTimeKind.Utc);

            using var writer = new TimescaleWriter(connStr, true, "my-pc");
            await using var conn = new NpgsqlConnection(connStr);
            await writer.ReportMetrics(epoch, TestSensorCreator.Values());

            conn.Open();
            await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM ohm_stats", conn);
            Assert.Equal(3, Convert.ToInt32(cmd.ExecuteScalar()));
        }

        [IgnoreOnRemoteDockerFact, Trait("Category", "integration")]
        public async void InsertOnlyTimescale()
        {
            var image = new ImageFromDockerfileBuilder()
                .WithDockerfile("timescale.dockerfile")
                .WithDockerfileDirectory("docker")
                .WithDeleteIfExists(true)
                .WithName("ohm-graphite-insert-only-timescale")
                .Build();
            await image.CreateAsync();

            var testContainersBuilder = new ContainerBuilder()
                .WithImage(image)
                .WithEnvironment("POSTGRES_PASSWORD", "123456")
                .WithPortBinding(5432, assignRandomHostPort: true)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilCommandIsCompleted("pg_isready -h 'localhost' -p '5432'"));

            await using var container = testContainersBuilder.Build();
            await container.StartAsync();

            string selectStr = $"Host={container.Hostname};Username=postgres;Password=123456;Port={container.GetMappedPublicPort(5432)};Database=timescale_built";
            var epoch = new DateTime(2001, 1, 13, 0, 0, 0, DateTimeKind.Utc);

            string connStr = $"Host={container.Hostname};Username=ohm;Password=itsohm;Port={container.GetMappedPublicPort(5432)};Database=timescale_built";
            using var writer = new TimescaleWriter(connStr, false, "my-pc");
            await using var conn = new NpgsqlConnection(selectStr);
            await writer.ReportMetrics(epoch, TestSensorCreator.Values());

            conn.Open();
            await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM ohm_stats", conn);
            Assert.Equal(3, Convert.ToInt32(cmd.ExecuteScalar()));
        }
    }
}
