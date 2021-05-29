[![CI](https://github.com/nickbabcock/OhmGraphite/actions/workflows/ci.yml/badge.svg)](https://github.com/nickbabcock/OhmGraphite/actions/workflows/ci.yml)

# OhmGraphite

OhmGraphite takes the hard work of extracting hardware sensors from [Open Hardware Monitor](http://openhardwaremonitor.org/) (technically [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) for most up to date hardware) and exports the data in a [graphite](https://graphiteapp.org/) (or [InfluxdDB](https://www.influxdata.com/) / [Prometheus](https://prometheus.io/) / [TimescaleDB](https://www.timescale.com/)) compatible format. OhmGraphite is for those missing any of the following in [Grafana](https://grafana.com/) or (other time series UI):

- Breakdown of GPU utilization
- Fan speed
- Temperature for hard drives, CPU cores, GPU, Motherboard
- Voltage readings

## Who's this for?

- People who are familiar with Graphite (or InfluxDB / Prometheus / TimescaleDB) / Grafana and may have an instance running on their home or cloud server. If you're not familiar with those applications, it may be overwhelming to setup and maintain them. If you're just looking for a UI for hardware sensors, I'd recommend [HWINFO](https://www.hwinfo.com/)
- People who know how to execute commands on Windows Command Prompt or other terminal
- People who like lightweight (8MB of RAM and neglible CPU usage), portable (can run off usb), and straightforward applications

## System Recommendations

- Windows
- Administrator privileges

## Introduction

OhmGraphite functions as a console app or a Windows service that periodically polls the hardware. My recommendation is that even though OhmGraphite can be run via Mono / Docker, many hardware sensors aren't available in those modes.

I use this every day to create beautiful dashboards. Keep in mind, Open Hardware Monitor supported components will determine what metrics are available. Below are graphs / stats made with OhmGraphite (couple of the panels are complemented with [telegraf](https://github.com/influxdata/telegraf) as demonstrated in [Monitoring Windows system metrics with grafana](https://nbsoftsolutions.com/blog/monitoring-windows-system-metrics-with-grafana))

[![dashboard](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/dashboard.png)](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/dashboard.png)

## Getting Started (Windows)

- Create a directory that will be the home base for OhmGraphite (I use C:\Apps\OhmGraphite).
- Download the [latest zip](https://github.com/nickbabcock/OhmGraphite/releases/latest) and extract to our directory.
- Update app configuration (located at `OhmGraphite.exe.config`). See configs for [Graphite](#graphite-configuration), [InfluxDB](#influxdb-configuration), [Prometheus](#prometheus-configuration), [Timescale / Postgres](#timescaledb-configuration)
- This config can be updated in the future, but will require a restart of the app for effect.
- The app can be ran interactively by executing `.\OhmGraphite.exe run`. Executing as administrator will most likely increase the number of sensors found (OhmGraphite will log how many sensors are found).
- To install the app `.\OhmGraphite.exe install`. The command will install OhmGraphite as a Windows service (so you can manage it with your favorite powershell commands or `services.msc`)
- To start the app after installation: `.\OhmGraphite.exe start` or your favorite Windows service management tool

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

### Grafana Configuration

While not necessary, there are dashboards tailored to OhmGraphite that one can use in Grafana to jump start their own dashboards:

- [Prometheus](https://grafana.com/grafana/dashboards/11587)
- [Graphite](https://grafana.com/grafana/dashboards/11591)
- [Postgres / Timescale](https://grafana.com/grafana/dashboards/11599)
- [Influxdb](https://grafana.com/grafana/dashboards/11601)
- [Influxdb (User submitted)](https://github.com/nickbabcock/OhmGraphite/blob/master/assets/dashboards/ohm-influx-alt.json)

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

### Metric Name Aliasing

It is possible that the sensor names exposed through OhmGraphite are not descriptive enough. For instance, "Fan #2" could have RPM exposed, but you know that a more descriptive name would be "CPU Fan". To have OhmGraphite export the sensor under the "CPU Fan" name, one will need to add the mapping from sensor id (+ `/name` suffix) to the desired name like so:

```xml
    <add key="/lpc/nct6792d/fan/1/name" value="CPU Fan" />
```

### Hiding Sensors

There may be a sensor that is faulty on a given machine. Maybe it reports negative temperatures. This can throw off monitoring software or make it harder to maintain with all the special cases. OhmGraphite allows one to exclude a sensor from being exported by modifying the OhmGraphite config and adding the `/hidden` suffix to the sensor id like so:

```xml
<add key="/lpc/nct6792d/temperature/1/hidden" />
```

### Determine Sensor Id

There are several ways to determine the sensor id of a metric:

- Postgres / Timescale and Influxdb users can examine their data store for the sensor id
- Perform the rename in LibreHardwareMonitor and copy and paste the line from `LibreHardwareMonitor.config` into `OhmGraphite.exe.config`.
- Search the `OhmGraphite.log` for the sensor's name that you'd like to rename (in the example, I'd search for "Fan #2"):

```
Sensor added: /lpc/nct6792d/fan/1 "Fan #2"
```

### Certificates

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

## Upgrades

- Stop OhmGraphite service `.\OhmGraphite.exe stop`
- Unzip latest release and copy `OhmGraphite.exe` to your installation directory.
- Start OhmGraphite service `.\OhmGraphite.exe start`

## Uninstall

- Stop OhmGraphite service `.\OhmGraphite.exe stop`
- Run uninstall command `.\OhmGraphite.exe uninstall`
- Remove files

## Debugging Tips

Something wrong? Try these steps

- Enter the directory where OhmGraphite is installed
- Examine `OhmGraphite.log`, do you see any lines with an "ERROR"? Fix the error.
- If not, enable more verbose logging in `NLog.config`. Change the following line

```xml
    <logger name="*" minlevel="Info" writeTo="file" />
```

to

```xml
    <logger name="*" minlevel="Debug" writeTo="file" />
```

- Restart OhmGraphite for the logging changes to take effect
- At the bottom of `OhmGraphite.log` `DEBUG` statements should be present informing one of all the hardware sensors detected and whenever metrics are pushed somewhere.
- Stumped? Open an issue with relevant parts of the log included.
