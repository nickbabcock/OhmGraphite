[![Build status](https://ci.appveyor.com/api/projects/status/c8b43wg7qtwicjj6/branch/master?svg=true)](https://ci.appveyor.com/project/nickbabcock/ohmgraphite/branch/master)

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
- .NET v4.6.1. If you have Windows 10 you are all set. If not, you may have to download a more recent version.
- Administrator privileges

## Introduction

OhmGraphite functions as a console app or a Windows service that periodically polls the hardware. My recommendation is that even though OhmGraphite can be run via Mono / Docker, many hardware sensors aren't available in those modes.

I use this every day to create beautiful dashboards. Keep in mind, Open Hardware Monitor supported components will determine what metrics are available. Below are graphs / stats made with OhmGraphite (couple of the panels are complemented with [telegraf](https://github.com/influxdata/telegraf) as demonstrated in [Monitoring Windows system metrics with grafana](https://nbsoftsolutions.com/blog/monitoring-windows-system-metrics-with-grafana))

[![dashboard](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/dashboard.png)](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/dashboard.png)

## Getting Started (Windows)

- Create a directory that will be the home base for OhmGraphite (I use C:\Apps\OhmGraphite).
- Download the [latest zip](https://github.com/nickbabcock/OhmGraphite/releases/latest) and extract to our directory.
- Update app configuration (located at `OhmGraphite.exe.config`) using either the Graphite config or InfluxDB config
- This config can be updated in the future, but will require a restart of the app for effect.
- The app can be ran interactively by executing `.\OhmGraphite.exe run`. Executing as administrator will most likely increase the number of sensors found (OhmGraphite will log how many sensors are found).
- To install the app `.\OhmGraphite.exe install`. The command will install OhmGraphite as a Windows service (so you can manage it with your favorite powershell commands or `services.msc`)
- To start the app after installation: `.\OhmGraphite.exe start` or your favorite Windows service management tool

### Hostname Resolution

When OhmGraphite sends metrics to the desired sink, it includes the computers hostname for additional context to allow scenarios where one has a grafana template variable based on hostname. There two possible ways for OhmGraphite to resolved the hostname: NetBIOS (the default) and DNS. It's hard to say exactly how a machine's NetBIOS name and internet host name will differ, but to give an example, a NetBIOS name of `TINI` can have a host name of `Tini`.

To switch to DNS hostname resolution, update the configuration to include `name_lookup`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="name_lookup" value="dns" />
  </appSettings>
</configuration>
```

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

### Prometheus Configuration

The Prometheus will create a server that listens on `prometheus_port`. The Prometheus configuration does not routinely poll the sensor instead it only polls them when a Prometheus server requests data.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="type" value="prometheus" />
    <add key="prometheus_port" value="4445" />
    <add key="prometheus_host" value="*" />
  </appSettings>
</configuration>
```

If the machine accessing the OhmGraphite metrics is not the same machine hosting the metrics, one may have to enable the port through the windows firewall.

Here's one example of enabling it in powershell, though note that there are further ways to configure the firewall for additional tightening of access (ie: only allow certain IPs to connect).

```powershell
New-NetFirewallRule -DisplayName "Allow port 4445 for OhmGraphite" -Direction Inbound -LocalPort 4445 -Protocol TCP -Action Allow
```

### TimescaleDB Configuration

One can configure OhmGraphite to send to Timescale with the following (configuration values will differ depending on your environment):

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

By leaving `timescale_setup` to `false` (the default) OhmGraphite will be expecting the following table structure to insert into:

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

Then give the `ohm` user appropriate permissions:

```sql
CREATE USER ohm WITH PASSWORD 'xxx';
GRANT INSERT ON ohm_stats TO ohm;
```

In this mode, Postgres is supported.

If `timescale_setup` is `true` then OhmGraphite will create the following schema, so make sure the user connecting has appropriate permissions

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

Currenlty the schema and the columns are not configurable.

### Upgrades

- Stop OhmGraphite service `.\OhmGraphite.exe stop`
- Unzip latest release and copy `OhmGraphite.exe` to your installation directory.
- Start OhmGraphite service `.\OhmGraphite.exe start`

### Uninstall

- Stop OhmGraphite service `.\OhmGraphite.exe stop`
- Run uninstall command `.\OhmGraphite.exe uninstall`
- Remove files

### Debugging Tips

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
