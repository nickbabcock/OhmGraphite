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
            var prometheusCollection = new PrometheusCollection(collector, "my-pc", Metrics.DefaultRegistry);
            var mserver = new MetricServer("localhost", 21881);
            var server = new PrometheusServer(mserver, collector);
            try
            {
                server.Start();
                var client = new HttpClient();
                var resp = await client.GetAsync("http://localhost:21881/metrics");
                Assert.True(resp.IsSuccessStatusCode);
                var content = await resp.Content.ReadAsStringAsync();
                Assert.Contains("# HELP intelcpu_0_temperature_0 Metric reported by open hardware sensor", content);
            }
            finally
            {
                server.Stop();
            }
        }

        [Fact]
        public async void PrometheusNicGuid()
        {
            var collector = new NicGuidSensor();
            var prometheusCollection = new PrometheusCollection(collector, "my-pc", Metrics.DefaultRegistry);
            var mserver = new MetricServer("localhost", 21881);
            var server = new PrometheusServer(mserver, collector);
            try
            {
                server.Start();
                var client = new HttpClient();
                var resp = await client.GetAsync("http://localhost:21881/metrics");
                Assert.True(resp.IsSuccessStatusCode);
                var content = await resp.Content.ReadAsStringAsync();
                Assert.Contains("Bluetooth Network Connection 2", content);
            }
            finally
            {
                server.Stop();
            }
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

            public void Stop()
            {
            }
        }
    }
}
