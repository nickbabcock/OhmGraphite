using System;
using InfluxDB.Client;

namespace OhmGraphite
{
    public class Influx2Config
    {
        public InfluxDBClientOptions Options { get; }

        public Influx2Config(InfluxDBClientOptions options)
        {
            Options = options;
        }

        public static Influx2Config ParseAppSettings(IAppConfig config)
        {
            var builder = new InfluxDBClientOptions.Builder();

            string influxAddress = config["influx2_address"];
            if (!Uri.TryCreate(influxAddress, UriKind.Absolute, out var addr))
            {
                throw new ApplicationException($"Unable to parse {influxAddress} into a Uri");
            }

            builder.Url(influxAddress);

            var token = config["influx2_token"];
            var user = config["influx2_user"];
            var password = config["influx2_password"];

            if (!string.IsNullOrEmpty(token))
            {
                builder.AuthenticateToken(token);
            }
            else
            {
                builder.Authenticate(user, password.ToCharArray());
            }

            var bucket = config["influx2_bucket"];
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ApplicationException($"influx2 needs a bucket to be configured");
            }
            builder.Bucket(bucket);

            var org = config["influx2_org"];
            if (string.IsNullOrEmpty(org))
            {
                throw new ApplicationException($"influx2 needs an org to be configured");
            }
            builder.Org(org);

            var validation = MetricConfig.CertificateValidationCallback(config["certificate_verification"] ?? "True");
            builder.RemoteCertificateValidationCallback(validation);

            return new Influx2Config(builder.Build());
        }
    }
}
