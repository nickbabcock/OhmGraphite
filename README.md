[![CI](https://github.com/nickbabcock/OhmGraphite/actions/workflows/ci.yml/badge.svg)](https://github.com/nickbabcock/OhmGraphite/actions/workflows/ci.yml)

# OhmGraphite

OhmGraphite is a Windows service that exposes hardware sensor data to a metric store, allowing one to create informative and beautiful dashboards in [Grafana](https://grafana.com/) or another time series UI:

[![dashboard](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/dashboard.png)](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/dashboard.png)

The above dashboard captures:

- Power consumption of the CPU and GPU
- CPU voltages and frequencies
- Load breakdown on individual GPU components
- CPU, GPU, disk, and motherboard temperature readings
- Disk activity, space remaining, and error monitoring
- Fan speed
- Network consumption

Supported metric stores:

- [Graphite](https://graphiteapp.org/) - [(starter dashboard)](https://grafana.com/grafana/dashboards/11591)
- [InfluxdDB](https://www.influxdata.com/) - [(starter dashboard)](https://grafana.com/grafana/dashboards/11601)
- [Prometheus](https://prometheus.io/) - [(starter dashboard)](https://grafana.com/grafana/dashboards/11587) for Grafana v8.3+
- [Postgres](https://www.postgresql.org/) / [TimescaleDB](https://www.timescale.com/) - [(starter dashboard)](https://grafana.com/grafana/dashboards/11599)

Hardware support is provided through [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor). Since detected sensors is hardware dependent, one can use the LibreHardwareMonitor GUI to preview a subset of metrics that will be exported by OhmGraphite. If a GUI of hardware sensors is all that is desired, and the thought of running and configuring Grafana and a metric store sounds overwhelming, I'd recommend [HWINFO](https://www.hwinfo.com/).

## Installation

- To allow OhmGraphite access to sensors like CPU temperature, wattage, and frequency: install [PawnIO](https://pawnio.eu/). OhmGraphite works without PawnIO, but will emit limited data.
- Create a directory that will be the home base for OhmGraphite (I use `C:\Apps\OhmGraphite`).
- Download the [latest zip](https://github.com/nickbabcock/OhmGraphite/releases/latest) and extract to our directory.
- Update app configuration (located at `OhmGraphite.exe.config`). See configs for [Graphite](#graphite-configuration), [InfluxDB](#influxdb-configuration), [Prometheus](#prometheus-configuration), [Timescale / Postgres](#timescaledb-configuration)
- **Run as Administrator**: Open PowerShell or Command Prompt
- To install the app `.\OhmGraphite.exe install`. The command will install OhmGraphite as a Windows service (so you can manage it with your favorite powershell commands or `services.msc`)
- To start the app after installation: `.\OhmGraphite.exe start` or your favorite Windows service management tool
- If immediately installing an app as an administrator is unnerving, OhmGraphite can be ran interactively with `.\OhmGraphite.exe run`. Note that running as a regular user will limit the number of sensors OhmGraphite can report on.

Congrats! Installation is done and you'll start seeing metrics flowing into your desired metric store.

## Upgrades

- Stop OhmGraphite service `.\OhmGraphite.exe stop`
- Unzip latest release and copy `OhmGraphite.exe` to your installation directory.
- Start OhmGraphite service `.\OhmGraphite.exe start`

## Uninstall

- Stop OhmGraphite service `.\OhmGraphite.exe stop`
- Run uninstall command `.\OhmGraphite.exe uninstall`
- Remove files

## Configuration

App configuration is located in the installation directory at `OhmGraphite.exe.config`.

Config updates require an app restart to take effect.

### Graphite Configuration

The config below polls our hardware every `5` seconds and sends the results to a graphite server listening on `localhost:2003`.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="host" value="localhost" />
    <add key="port" value="2003" />
    <add key="interval" value="5" />
    <add key="tags" value="false" />
  </appSettings>
</configuration>
```

Starting with Graphite v1.1.0, Graphite supports tags (similar to InfluxDB's tags). When enabled in OhmGraphite the data format switches from `<name> <value> <timestamp>` to `<name>;tag1=a;tag2=b <value> <timestamp>`.  Since tags are such a new feature, OhmGraphite has it disabled by default to prevent cumbersome usage with Graphite 0.9 and 1.0 installations.

Examples of types of tags used (same for InfluxDB):

- sensor_type: temperature, load, watts, rpms
- hardware_type: cpu, gpu, hdd
- host: my-pc
- app: ohm
- hardware: Nvidia GTX 970, Intel i7 6700k
- raw_name (sensor name): CPU DRAM, CPU graphics

For any serious interest in tags, make sure to use external db like postgres, mysql, or redis, as [sqlite won't cut it](https://github.com/graphite-project/docker-graphite-statsd/issues/32#issuecomment-378536784).

### InfluxDB Configuration

Graphite is the default export style, but if you're an InfluxDB user you can change the `type` to `influxdb` and fill out InfluxDB specific options:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="type" value="influxdb" />
    <add key="interval" value="5" />
    <add key="influx_address" value="http://localhost:8086" />
    <add key="influx_db" value="mydb" />
<!--
    <add key="influx_user" value="myuser" />
    <add key="influx_password" value="mypassword" />
    <add key="interval" value="5" />
-->
  </appSettings>
</configuration>
```

If OhmGraphite will be connecting to InfluxDB 2, the configuration will need to be changed accordingly.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="type" value="influx2" />
    <add key="influx2_address" value="http://localhost:8086" />
    <add key="influx2_org" value="myorg" />
    <add key="influx2_bucket" value="mydb" />
    <add key="influx2_token" value="thisistheinfluxdbtoken" />
    <add key="interval" value="5" />
  </appSettings>
</configuration>
```

### Prometheus Configuration

Configuring the Prometheus exporter will create a server that listens on `prometheus_port`. Instead of creating outbound data like the other exporters, OhmGraphite's Prometheus config creates inbound data. OhmGraphite will only poll the hardware sensors when scraped by the Prometheus service.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="type" value="prometheus" />
    <add key="prometheus_port" value="4445" />
    
    <!-- This is the host that OhmGraphite listens on.
         `*` means that it will listen on all interfaces.
         Consider restricting to a given IP address -->
    <add key="prometheus_host" value="*" />
    <add key="prometheus_path" value="metrics/" /> 
  </appSettings>
</configuration>
```

Then you'll need add the OhmGraphite instance to your [Prometheus config](https://prometheus.io/docs/prometheus/latest/configuration/configuration/). This can be done with the method of your choosing but for the sake of example here is a possible `prometheus.yml`:

```yaml
global:
  scrape_interval: 15s
scrape_configs:
  - job_name: 'ohmgraphite'
    static_configs:
    - targets: ['10.0.0.200:4445']
```

In the above example, the Prometheus server and OhmGraphite are not on the same machine, so Prometheus accesses OhmGraphite through the machine that is hosting OhmGraphite via the IP address (`10.0.0.200`).

If the Prometheus service accessing OhmGraphite is not on the same machine, one may have to enable the port through the windows firewall.

Here's one example of enabling it in powershell. Note that there are further ways to configure the firewall for additional tightening of access (ie: only allow certain IPs to connect).

```powershell
New-NetFirewallRule -DisplayName "Allow port 4445 for OhmGraphite" -Direction Inbound -LocalPort 4445 -Protocol TCP -Action Allow
```

#### Prometheus HTTPS Configuration

This section will walkthrough setting up HTTPS communication with a self signed certificate between OhmGraphite and Prometheus:

Execute the instructions below with an admin powershell terminal to generate the certificate, import it into the machine, and then bind the certificate to the configured port.

```pwsh
# Create a new self signed certificate with a subject equal to host used to
# access OhmGraphite. If an IP address is used to access OhmGraphite,
# you'll need the IPAddress field, otherwise the `TextExtension` param can be
# replaced with the DnsName param.
$params = @{
  FriendlyName = 'OhmGraphite'
  Subject = '10.0.0.200'
  TextExtension = @('2.5.29.17={text}&IPAddress=10.0.0.200')
}
$cert = New-SelfSignedCertificate @params
$thumb = $cert.Thumbprint

# Export and then import our cert into Windows certificate store
Export-Certificate -Cert $cert -FilePath ohmgraphite.cer
Import-Certificate -FilePath .\ohmgraphite.cer -CertStoreLocation Cert:\LocalMachine\Root

# Bind our cert to the port OhmGraphite is listening on
netsh http add sslcert ipport=0.0.0.0:4445 certhash=$thumb
```

Enable HTTPS in `OhmGraphite.exe.config` with `prometheus_https`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="type" value="prometheus" />
    <add key="prometheus_port" value="4445" />
    <add key="prometheus_host" value="*" />
    <add key="prometheus_https" value="true" />
  </appSettings>
</configuration>
```

With OhmGraphite configured, the prometheus server is next. In order to have prometheus verify against a self signed certificate, the certificate must be converted into a format prometheus understands:

```bash
# Linux:
openssl x509 -inform der -in ohmgraphite.cer -out ohmgraphite.pem

# Windows:
# certutil -encode .\ohmgraphite.cer .\ohmgraphite.pem
```

Then update the prometheus config to expect our certificate:

```diff
   - job_name: 'ohmgraphite'
+    scheme: https
+    tls_config:
+      ca_file: /etc/prometheus/ohmgraphite.pem
     static_configs:
     - targets: ['10.0.0.200:4445']
```

### TimescaleDB Configuration

One can configure OhmGraphite to send to Timescale / Postgres with the following (configuration values will differ depending on your environment):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="type" value="timescale" />
    <add key="timescale_connection" value="Host=vm-ubuntu;Username=ohm;Password=123456;Database=postgres" />
    <add key="timescale_setup" value="false" />
  </appSettings>
</configuration>
```

By leaving `timescale_setup` to `false` (the default) OhmGraphite can insert into any plain Postgres table that follows this table structure:

```sql
CREATE TABLE IF NOT EXISTS ohm_stats (
   time TIMESTAMPTZ NOT NULL,
   host TEXT,
   hardware TEXT,
   hardware_type TEXT,
   identifier TEXT,
   sensor TEXT,
   sensor_type TEXT,
   sensor_index INT,
   value REAL
);
```

Ensure that the OhmGraphite user that inserts the metrics (`ohm` in our example) has appropriate permissions:

```sql
CREATE USER ohm WITH PASSWORD 'xxx';
GRANT INSERT ON ohm_stats TO ohm;
```

If `timescale_setup` is `true` then OhmGraphite will create the following schema, so make sure Timescale is enabled on the server and the user connecting has appropriate permissions

```sql
CREATE TABLE IF NOT EXISTS ohm_stats (
   time TIMESTAMPTZ NOT NULL,
   host TEXT,
   hardware TEXT,
   hardware_type TEXT,
   identifier TEXT,
   sensor TEXT,
   sensor_type TEXT,
   sensor_index INT,
   value REAL
);

SELECT create_hypertable('ohm_stats', 'time', if_not_exists => TRUE);
CREATE INDEX IF NOT EXISTS idx_ohm_host ON ohm_stats (host);
CREATE INDEX IF NOT EXISTS idx_ohm_identifier ON ohm_stats (identifier);
```

Currently the schema and the columns are not configurable.

### Hostname Resolution

When OhmGraphite sends metrics to the desired sink, it includes the computers hostname for additional context to allow scenarios where one has a grafana template variable based on hostname. There are three possible ways for OhmGraphite to resolve the hostname: NetBIOS (the default), DNS, and a static user-configured name.

> NOTE: It's hard to say exactly how a machine's NetBIOS name and internet host name will differ, but to give an example, a NetBIOS name of `TINI` can have a host name of `Tini`.

To switch to DNS hostname resolution, update the configuration to include `name_lookup`, else any other value will be assumed to be a custom static name.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="name_lookup" value="dns" />
  </appSettings>
</configuration>
```

Prometheus setups are unaffected by `name_lookup`, as Prometheus [automatically creates the instance label when scraping](https://prometheus.io/docs/concepts/jobs_instances/). If renaming is desired, you'll want to [change the instance label](https://www.robustperception.io/controlling-the-instance-label) by either hardcoding the alias in the Prometheus config or using DNS.

### Metric Name Aliasing

It is possible that the sensor names exposed through OhmGraphite are not descriptive enough. For instance, "Fan #2" could have RPM exposed, but you know that a more descriptive name would be "CPU Fan". To have OhmGraphite export the sensor under the "CPU Fan" name, one will need to add the mapping from sensor id (+ `/name` suffix) to the desired name like so:

```xml
    <add key="/lpc/nct6792d/fan/1/name" value="CPU Fan" />
```

### Hiding Sensors

There may be a sensor that should be hidden. Maybe it's a temperature sensor that reports negative values, or maybe a sensor reporting aggregated values that has no use. Whatever the case, OhmGraphite allows one to exclude a sensor from being exported by modifying the OhmGraphite config and adding the `/hidden` suffix to the sensor id or name like so:

```xml
<add key="/lpc/nct6792d/temperature/1/hidden" />
<add key="CPU Core Max/hidden" />
```

One can use globs to ignore a group of sensors. For instance, to hide all power sensors and hide clock sensors from an AMD CPU:

```xml
<add key="/amdcpu/*/clock/*/hidden" />
<add key="/*/power/*/hidden" />
``` 

### Determine Sensor Id

There are several ways to determine the sensor id of a metric:

- Postgres / Timescale and Influxdb users can examine their data store for the sensor id
- Perform the rename in LibreHardwareMonitor and copy and paste the line from `LibreHardwareMonitor.config` into `OhmGraphite.exe.config`.
- Search the `OhmGraphite.log` for the sensor's name that you'd like to rename (in the example, I'd search for "Fan #2"):

```
Sensor added: /lpc/nct6792d/fan/1 "Fan #2"
```

### Disabling Hardware

By default, all hardware sensor collection is enabled to allow for minimal configuration in common use cases. However, some hardware may be susceptible to instability when polled. Hiding all of the sensors from unstable hardware isn't sufficient as sensor name filtering occurs after querying hardware. Thus there is configuration to determine what hardware is enabled.

The snippet below shows all the options that can be used to disable hardware.

```xml
<add key="/cpu/enabled" value="FaLsE" />
<add key="/gpu/enabled" value="false" />
<add key="/motherboard/enabled" value="false" />
<add key="/ram/enabled" value="false" />
<add key="/network/enabled" value="false" />
<add key="/storage/enabled" value="false" />
<add key="/controller/enabled" value="false" />
<add key="/psu/enabled" value="false" />
<add key="/battery/enabled" value="false" />
```

Since disabling sensors at the hardware level is more efficient than a glob to hide desired sensors, disabling hardware is desirable even if the underlying hardware is stable.

When hardware is disabled, all instances of that hardware are disabled. For instance, if one has multiple storage devices and only one is unstable, disabling storage hardware will halt sensor collection from all of them.

### Certificates

By default, OhmGraphite will fail to communicate with servers that present certificates that can't be verified. To workaround this issue, the server's certificate should be imported on the OhmGraphite machine.

Below shows an example setup where Influxdb v1 is running on a linux server with a domain name of `vm-ubuntu`:

Generate certificate:

```bash
mkdir ssl
openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes \
  -keyout ssl/ohm.key -out ssl/ohm.crt -subj "/CN=vm-ubuntu" \
  -addext "subjectAltName=DNS:vm-ubuntu,IP:172.22.24.52"
```

Run influxdb (via docker-compose) with our certificate:

```yaml
version: "3"
services:
  influxdb:
    image: influxdb:1.8
    ports:
      - "8086:8086"
    volumes:
      - influxdb:/var/lib/influxdb
      - ./ssl:/etc/ssl/
    environment:
      - INFLUXDB_DB=db0
      - INFLUXDB_ADMIN_USER=admin
      - INFLUXDB_ADMIN_PASSWORD=supersecretpassword
      - INFLUXDB_HTTP_HTTPS_ENABLED=true
      - INFLUXDB_HTTP_HTTPS_CERTIFICATE=/etc/ssl/ohm.crt
      - INFLUXDB_HTTP_HTTPS_PRIVATE_KEY=/etc/ssl/ohm.key
      - INFLUXDB_HTTP_AUTH_ENABLED=true

volumes:
  influxdb:
```

Then on the OhmGraphite machine, import the certificate with an admin powershell instance:

```pwsh
Import-Certificate -FilePath .\ohm.crt -CertStoreLocation 'Cert:\LocalMachine\Root'
```

#### `certificate_verification` (**deprecated**)

**This config option has been deprecated due to not working as intended with later .NET versions**

When connecting to a service that presents a self signed certificate, one can specify `certificate_verification`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="certificate_verification"
         value="C:\apps\OhmGraphite\influxdb-selfsigned.crt" />
   </appSettings>
</configuration>
```

The possible values:

 - True (the default): all certificates are verified
 - False: No certificates are verified (INSECURE)
 - a file path of a certificate that the server is allowed to return and
 still be considered a valid request (useful for self signed
 certificates). Recommended to be an absolute file path.

## Debugging Tips

Something wrong? Try these steps

- Enter the directory where OhmGraphite is installed
- Examine `OhmGraphite.log`, do you see any lines with an "ERROR"? Fix the error.
- To see what hardward and sensors have been detected, and when metrics are
  pushed to the destination, enable more logging in `NLog.config`. Change the
  following line
  ```xml
    <logger name="*" minlevel="Info" writeTo="file" />
  ```
  to
  ```xml
    <logger name="*" minlevel="Debug" writeTo="file" />
  ```
- Restart OhmGraphite for the logging changes to take effect
- To see every sensor update logged (WARNING: your log file will grow large),
  enable trace logging
  ```xml
    <logger name="*" minlevel="Trace" writeTo="file" />
  ```
- Stumped? Open an issue with relevant parts of the log included.
