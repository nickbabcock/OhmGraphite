using System;
using System.Configuration;
using Xunit;

namespace OhmGraphite.Test
{
    public class ConfigTest
    {
        [Fact]
        public void CanParseGraphiteConfig()
        {
            var configMap = new ExeConfigurationFileMap {ExeConfigFilename = "test.config"};
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var customConfig = new CustomConfig(config);
            var results = MetricConfig.ParseAppSettings(customConfig);

            Assert.NotNull(results.Graphite);
            Assert.Null(results.Influx);
            Assert.Equal("myhost", results.Graphite.Host);
            Assert.Equal(2004, results.Graphite.Port);
            Assert.Equal(TimeSpan.FromSeconds(6), results.Interval);
            Assert.True(results.Graphite.Tags);
        }

        [Fact]
        public void CanParseNullConfig()
        {
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = "testdefault.config" };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var customConfig = new CustomConfig(config);
            var results = MetricConfig.ParseAppSettings(customConfig);

            Assert.NotNull(results.Graphite);
            Assert.Null(results.Influx);
            Assert.Equal("localhost", results.Graphite.Host);
            Assert.Equal(2003, results.Graphite.Port);
            Assert.Equal(TimeSpan.FromSeconds(5), results.Interval);
            Assert.False(results.Graphite.Tags);
        }

        [Fact]
        public void CanParseInfluxDbConfig()
        {
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = "influxtest.config" };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var customConfig = new CustomConfig(config);
            var results = MetricConfig.ParseAppSettings(customConfig);

            Assert.Null(results.Graphite);
            Assert.NotNull(results.Influx);
            Assert.Equal(TimeSpan.FromSeconds(6), results.Interval);
            Assert.Equal("http://192.168.1.15:8086/", results.Influx.Address.ToString());
            Assert.Equal("mydb", results.Influx.Db);
            Assert.Equal("my_user", results.Influx.User);
            Assert.Equal("my_pass", results.Influx.Password);
        }

        [Fact]
        public void CanParsePrometheusConfig()
        {
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = "prometheus.config" };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var customConfig = new CustomConfig(config);
            var results = MetricConfig.ParseAppSettings(customConfig);

            Assert.Null(results.Graphite);
            Assert.Null(results.Influx);
            Assert.NotNull(results.Prometheus);
            Assert.Equal(4446, results.Prometheus.Port);
            Assert.Equal("127.0.0.1", results.Prometheus.Host);
        }

        [Fact]
        public void CanParseTimescaleConfig()
        {
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = "timescale.config" };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var customConfig = new CustomConfig(config);
            var results = MetricConfig.ParseAppSettings(customConfig);

            Assert.Null(results.Graphite);
            Assert.Null(results.Influx);
            Assert.Null(results.Prometheus);
            Assert.NotNull(results.Timescale);
            Assert.Equal("Host=vm-ubuntu;Username=ohm;Password=123456", results.Timescale.Connection);
            Assert.False(results.Timescale.SetupTable);
        }
    }
}
