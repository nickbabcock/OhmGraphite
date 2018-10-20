using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            var prometheusCollection = new PrometheusCollection(collector, "my-pc");
            var mserver = new MetricServer("localhost", 21881);
            var server = new PrometheusServer(mserver, collector, prometheusCollection);
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
    }
}
