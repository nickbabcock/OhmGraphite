using System;

namespace OhmGraphite
{
    public class InfluxConfig
    {
        public InfluxConfig(Uri address, string db, string user, string password)
        {
            Address = address;
            Db = db;
            User = user;
            Password = password;
        }

        public Uri Address { get; }
        public string Db { get; }
        public string User { get; }
        public string Password { get; }

        public static InfluxConfig ParseAppSettings(IAppConfig config)
        {
            string influxAddress = config["influx_address"];
            if (!Uri.TryCreate(influxAddress, UriKind.Absolute, out var addr))
            {
                throw new ApplicationException($"Unable to parse {influxAddress} into a Uri");
            }

            var db = config["influx_db"];
            if (string.IsNullOrEmpty(db))
            {
                throw new ApplicationException("influx_db must be specified in the config");
            }

            var user = config["influx_user"];
            var password = config["influx_password"];

            return new InfluxConfig(addr, db, user, password);
        }
    }
}
