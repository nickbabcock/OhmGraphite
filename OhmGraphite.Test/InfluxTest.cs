using System;
using System.Net.Http;
using System.Threading;
using Xunit;

namespace OhmGraphite.Test
{
    public class InfluxTest
    {
        [Fact, Trait("Category", "integration")]
        public async void CanInsertIntoInflux()
        {
            var config = new InfluxConfig(new Uri("http://influx:8086"), "mydb", "my_user", "my_pass");
            var writer = new InfluxWriter(config, "my-pc");
            await writer.ReportMetrics(DateTime.Now, TestSensorCreator.Values());

            Thread.Sleep(TimeSpan.FromSeconds(1));
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync("http://influx:8086/query?pretty=true&db=mydb&q=SELECT%20*%20FROM%20Temperature");
                Assert.True(resp.IsSuccessStatusCode);
                var content = await resp.Content.ReadAsStringAsync();
                Assert.Contains("/intelcpu/0/temperature/0", content);
            }
        }
    }
}
