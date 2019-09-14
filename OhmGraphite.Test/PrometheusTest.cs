using System.Collections.Generic;
using System.Net.Http;
using OpenHardwareMonitor.Hardware;
using Prometheus;
using Xunit;

namespace OhmGraphite.Test
{
    public class PrometheusTest
    {
        [Fact]
        public async void PrometheusTestServer()
        {
            var collector = new TestSensorCreator();
            var registry = PrometheusCollection.SetupDefault(collector);
            var mserver = new MetricServer("localhost", 21881, registry: registry);
            using (var server = new PrometheusServer(mserver, collector))
            {
                server.Start();
                var client = new HttpClient();
                var resp = await client.GetAsync("http://localhost:21881/metrics");
                Assert.True(resp.IsSuccessStatusCode);
                var content = await resp.Content.ReadAsStringAsync();
                Assert.Contains("# HELP ohm_cpu_celsius Metric reported by open hardware sensor", content);
            }
        }

        [Fact]
        public async void PrometheusNicGuid()
        {
            var collector = new NicGuidSensor();
            var registry = PrometheusCollection.SetupDefault(collector);
            var mserver = new MetricServer("localhost", 21882, registry: registry);
            using (var server = new PrometheusServer(mserver, collector))
            {
                server.Start();
                var client = new HttpClient();
                var resp = await client.GetAsync("http://localhost:21882/metrics");
                Assert.True(resp.IsSuccessStatusCode);
                var content = await resp.Content.ReadAsStringAsync();
                Assert.Contains("Bluetooth Network Connection 2", content);
            }
        }

        [Fact]
        public void PrometheusStopServerWithoutStarting()
        {
            var collector = new NicGuidSensor();
            var registry = PrometheusCollection.SetupDefault(collector);
            var mserver = new MetricServer("localhost", 21883, registry: registry);
            var server = new PrometheusServer(mserver, collector);

            // Asserting that the following doesn't throw when the server
            // is disposed before starting
            server.Dispose();
        }

        public class NicGuidSensor : IGiveSensors
        {
            public IEnumerable<ReportedValue> ReadAllSensors()
            {
                yield return new ReportedValue("/nic/{my-guid}/throughput/7", "Bluetooth Network Connection 2", 1.06f, SensorType.Throughput, "cpu", HardwareType.NIC, 7);
            }

            public void Start()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
