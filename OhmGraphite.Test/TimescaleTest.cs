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
            var writer = new TimescaleWriter(connStr, true, "my-pc");
            var epoch = new DateTime(2001, 1, 13);
            writer.ReportMetrics(epoch, TestSensorCreator.Values());
            using (var conn = new NpgsqlConnection(connStr))
            {
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

            const string connStr = "Host=timescale;Username=ohm;Password=itsohm;Database=timescale_built";
            var writer = new TimescaleWriter(connStr, false, "my-pc");
            var epoch = new DateTime(2001, 1, 13);
            writer.ReportMetrics(epoch, TestSensorCreator.Values());
            using (var conn = new NpgsqlConnection(selectStr))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM ohm_stats", conn))
                {
                    Assert.Equal(3, Convert.ToInt32(cmd.ExecuteScalar()));
                }
            }
        }
    }
}
