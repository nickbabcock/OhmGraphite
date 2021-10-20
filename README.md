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
- [Prometheus](https://prometheus.io/) - [(starter dashboard)](https://grafana.com/grafana/dashboards/11587)
- [Postgres](https://www.postgresql.org/) / [TimescaleDB](https://www.timescale.com/) - [(starter dashboard)](https://grafana.com/grafana/dashboards/11599)

Hardware support is provided through [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor). Since detected sensors is hardware dependent, one can use the LibreHardwareMonitor GUI to preview a subset of metrics that will be exported by OhmGraphite. If a GUI of hardware sensors is all that is desired, and the thought of running and configuring Grafana and a metric store sounds overwhelming, I'd recommend [HWINFO](https://www.hwinfo.com/).

## Installation

- Create a directory that will be the home base for OhmGraphite (I use `C:\Apps\OhmGraphite`).
- Download the [latest zip](https://github.com/nickbabcock/OhmGraphite/releases/latest) and extract to our directory.
- Update app configuration (located at `OhmGraphite.exe.config`). See configs for [Graphite](#graphite-configuration), [InfluxDB](#influxdb-configuration), [Prometheus](#prometheus-configuration), [Timescale / Postgres](#timescaledb-configuration)
- To install the app `.\OhmGraphite.exe install`. The command will install OhmGraphite as a Windows service (so you can manage it with your favorite powershell commands or `services.msc`)
- To start the app after installation: `.\OhmGraphite.exe start` or your favorite Windows service management tool
- If immediately installing the app is unnerving, the app can be ran interactively by executing `.\OhmGraphite.exe run`. Executing as administrator will most likely increase the number of sensors found (OhmGraphite will log how many sensors are found).

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
```

Since disabling sensors at the hardware level is more efficient than a glob to hide desired sensors, disabling hardware is desirable even if the underlying hardware is stable.

When hardware is disabled, all instances of that hardware are disabled. For instance, if one has multiple storage devices and only one is unstable, disabling storage hardware will halt sensor collection from all of them.

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
