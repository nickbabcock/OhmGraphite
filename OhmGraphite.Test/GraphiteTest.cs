using System;
using System.Net.Http;
using System.Threading;
using Xunit;

namespace OhmGraphite.Test
{
    public class GraphiteTest
    {
        [Fact, Trait("Category", "integration")]
        public async void InsertGraphiteTest()
        {
            using (var writer = new GraphiteWriter("graphite", 2003, "my-pc", tags: false))
            using (var client = new HttpClient())
            {
                await writer.ReportMetrics(DateTime.Now, TestSensorCreator.Values());

                // wait for carbon to sync to disk
                Thread.Sleep(TimeSpan.FromSeconds(4));

                {
                    var resp = await client.GetAsync("http://graphite/render?format=csv&target=ohm.my-pc.intelcpu.0.temperature.cpucore.1");
                    var content = await resp.Content.ReadAsStringAsync();
                    Assert.Contains("ohm.my-pc.intelcpu.0.temperature.cpucore.1", content);
                }
            }

        }

        [Fact, Trait("Category", "integration")]
        public async void InsertTagGraphiteTest()
        {
            // Let the tag engine time to breathe
            Thread.Sleep(TimeSpan.FromSeconds(2));

            using (var writer = new GraphiteWriter("graphite", 2003, "my-pc", tags: true))
            using (var client = new HttpClient())

            {
                await writer.ReportMetrics(DateTime.Now, TestSensorCreator.Values());

                // wait for carbon to sync to disk
                Thread.Sleep(TimeSpan.FromSeconds(4));

                {
                    var resp = await client.GetAsync("http://graphite/render?format=csv&target=seriesByTag('sensor_type=Temperature','hardware_type=CPU')");
                    var content = await resp.Content.ReadAsStringAsync();
                    Assert.Contains("host=my-pc", content);
                    Assert.Contains("app=ohm", content);
                    Assert.Contains("sensor_type=Temperature", content);
                }
            }
        }
    }
}
