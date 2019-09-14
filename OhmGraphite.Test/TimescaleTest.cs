using System;
using Npgsql;
using Xunit;

namespace OhmGraphite.Test
{
    public class TimescaleTest
    {
        [Fact, Trait("Category", "integration")]
        public async void CanSetupTimescale()
        {
            const string connStr = "Host=timescale;Username=postgres;Password=123456";
            var epoch = new DateTime(2001, 1, 13);

            using (var writer = new TimescaleWriter(connStr, true, "my-pc"))
            using (var conn = new NpgsqlConnection(connStr))
            {
                await writer.ReportMetrics(epoch, TestSensorCreator.Values());

                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM ohm_stats", conn))
                {
                    Assert.Equal(3, Convert.ToInt32(cmd.ExecuteScalar()));
                }
            }
        }

        [Fact, Trait("Category", "integration")]
        public async void InsertOnlyTimescale()
        {
            const string selectStr = "Host=timescale;Username=postgres;Password=123456;Database=timescale_built";
            var epoch = new DateTime(2001, 1, 13);

            const string connStr = "Host=timescale;Username=ohm;Password=itsohm;Database=timescale_built";
            using (var writer = new TimescaleWriter(connStr, false, "my-pc"))
            using (var conn = new NpgsqlConnection(selectStr))
            {
                await writer.ReportMetrics(epoch, TestSensorCreator.Values());

                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM ohm_stats", conn))
                {
                    Assert.Equal(3, Convert.ToInt32(cmd.ExecuteScalar()));
                }
            }
        }
    }
}
