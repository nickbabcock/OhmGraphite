## 0.11.0 - 2020-01-06

Big update to the underlying LibreHardwareMonitor library, I've tried to
ensure that backwards compatibility is maintained in respect to the
metric names that are generated; however, there some breakages may slip
through. For instance, I believe that the CPU DRAM power sensor for
intel chips have been relabed to "CPU Memory", so one should double
check and adjust their dashboards as needed.

Other changes to LibreHardwareMonitor:

- Add SoC voltage for Ryzen 2
- Add support for AMD / ATI rx5700 GPUs
- Fix Ryzen temperature offsets
- Add motherboard: B350 Gaming Plus
- Add motherboard: X470 AORUS GAMING 7 WIFI-CF
- Add motherboard: ITE IT8792E
- Add liquid + plx temperatures and power usage for AMD / ATI GPUs.
- Bugfix for Asrock Pro / Steel Legend B450 motherboards
- Add detection for additional Intel architectures:
  - Goldmont
  - Goldmont Plus
  - Cannon Lake
  - Ice Lake
  - Tiger Lake
  - Tremont
- Add basic support for Aquacomputer's MPS (USB high flow)
- Discard out of range temperatures for NVMe drives (-1000, 1000)

## 0.10.0 - 2019-09-14

* **Breaking Change for Prometheus**:
  * Sub categorize percent metrics to prevent overriding values. For instance, `ohm_gpunvidia_percent` has been split into `ohm_gpunvidia_control_percent` and `ohm_gpunvidia_load_percent`.
  * Include a "hw_instance" (hardware_instance) label to prometheus metrics. In a multi cpu, gpu, hdd system where the name of the hardware would be the same (ie: two graphics cards of "NVIDIA GeForce GTX 1070"), the metric values would clobber each other. The fix is to transmit the hardware's identifier as a metric label. These identifiers will often be an number representing the index of hardware (eg: "0" and "1"). Nics will use their guid's. I'm hoping future improvements could transmit the hard disk's mount point (eg: "C:\", "D:\"), as indices can non-intuitive. Other metric reporters should not be susceptible to this issue as the sensor's identifier is transmitted as well, so no breaking change for them now, but if the hardware identifier notion proves fruitful then these changes will be ported to the other metric reporters.
* Allow one to assign a static hostname instead of DNS name or NetBIOS lookup. This is accomplished by setting an arbitrary value to `name_lookup` in the config (eg: `<add key="name_lookup" value="my-cool-machine" />`)
* Update LibreHardwareMonitor to [63dcfe9](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/tree/63dcfe94b58c60cdf203f972fdb3d76f7babeeaa):
  * Much improved list of supported NVMe drives!
  * Support for cores / threads greater than 254
  * Update for Ryzen 3000
  * Add support for MSI B450-A Pro
  * Add missing ATI GPU temperatures
* Internal dependency bump:
  * Bump NLog.Config from 4.6.5 to 4.6.7
  * Bump Npgsql from 4.0.7 to 4.0.10

## 0.9.1 - 2019-07-10

* Update LibreHardwareMonitor to [14021762](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/tree/1402176289d2c2d70f332ba10a34e1fcc0aaccbc):
  * More CPU, Mainboard, and GPU sensors
* Internal dependency update (no behavior changes should be expected):
  * Bump TopShelf from 4.2.0 to 4.2.1 
  * Bump prometheus-net from 3.1.3 to 3.1.4 

## 0.9.0 - 2019-06-09

* **Breaking Change for Prometheus**: OhmGraphite has not been following [Prometheus best practices](https://prometheus.io/docs/practices/naming/) when it came to naming metrics. Metric names now look like "ohm_cpu_celsius" with only the "hardware" and "sensor" labels remaining. The following changes have been implemented:
  * `app` metric label has been removed in favor of a metric namespace prefix of "ohm"
  * `hardware_type` metric label has been removed in favor of encapsulating it into the metric name (eg: "cpu", "nic").
  * `sensor_index` metric label has been removed. This label proved superfluous as every sensor can be uniquely identified by it's name.
  * `host` metric label has been removed: This falls in line with other prometheus exporters like node_exporter, which does not export the host as a label.
  * base unit included in metric name: (like "bytes", "revolutions per minute", etc)
  * The value that is exported to Prometheus is now converted into base units, such as converting GB (2^30) and MB (2^20) into bytes, MHz into hertz. Other units are unaffected. There are two candidates for this conversion that were unaffected:
    - Flow rate is still liters per hour, even though liters per second may seem more "base-unity", but grafana contained the former but not the latter.
    - Fan speed remains revolutions per minute, as I'm unaware of any manufacturer reporting fan speed as revolutions per second.
  * Side note: OhmGraphite now follows the [official data model naming scheme](https://prometheus.io/docs/concepts/data_model/#metric-names-and-labels) by replacing invalid characters with an underscore. 
* Only allow non-NaN and finite sensor values to be reported. Previously, NaN and infinite values could be reported which may cause downstream issues. For instance, Postgres / Prometheus will accept NaN values but Grafana will error out with a body json marshal error. These unexpected values should be quite rare, as out of the 25 million data points over the past week, 14 of those over 2 seconds were reported as NaN. It only takes a single NaN value to ruin a dashboard, so it's been fixed, and if a NaN value were to occur again, the sensor id would be logged under `DEBUG` before being discarded.
* Update LibreHardwareMonitor to [713fd30](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/tree/713fd3071b48c54cfd7d61263c77c0b9df5224cf)
  * Fix: Nvidia power usage monitor via NVML (you'll need to install Nvidia's CUDA toolkit)
* Internal dependency update (no behavior changes should be expected):
  * Bump prometheus-net from 3.1.0 to 3.1.3
  * Bump Npgsql from 4.0.5 to 4.0.7
  * Bump NLog.Config from 4.6.2 to 4.6.4

## 0.8.3 - 2019-04-08

* Allow one to switch from sending NetBIOS machine name to sending internet host name to metric sink.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="name_lookup" value="dns" />
  </appSettings>
</configuration>
```

(If you opt into this configuration, expect metric names / paths to change (eg: if the metric host was previously `TINI`, it may change to `Tini`) (ref: https://github.com/nickbabcock/OhmGraphite/issues/53))

* Improve postgres connection attempts in edge cases
* Update internal dependencies:
  * Bump prometheus-net from 3.0.3 to 3.1.0 (no apparent changes for OhmGraphite)
  * Bump Npgsql from 4.0.4 to 4.0.5 (fixes bugs)
  * Bump NLog.Config from 4.5.11 to 4.6.2 (more logging configurations for those who want it)

## 0.8.2 - 2019-02-25

Bugfix release for our Prometheus users

* Sanitize additional metric names for Prometheus ([#43](https://github.com/nickbabcock/OhmGraphite/issues/43))
* Bump prometheus-net from 3.0.1 to 3.0.3 for bugfixes

## 0.8.1 - 2019-02-07

* Fix: graphite: remove NIC guids in squirrelly parentheses
* Fix: graphite: when graphite tag functionality is enabled, format number with culture invariant
* Internal dependency update:
  * Bump prometheus-net from 2.1.3 to 3.0.1

## 0.8.0 - 2019-01-30

* Update LibreHardwareMonitor to [98969e](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/tree/98969ebc3e5fa9d896f0942a68fb2b8b27cba1ac)
  * Add: Network (NIC) sensors (data downloaded, etc)
  * Add: AsRock B85M-DGS support for Voltage sensors
  * Add: Asus ROG Maximus Apex Mobo support
  * Add: Intel SSD Airflow temperature support
  * Add: Nuvoton NCT6798D support
  * Add: Z390 mobos and IT8688E chip support
  * Add: Nvidia power usage monitor via NVML (you'll need to install Nvidia's CUDA toolkit)
* Internal dependency update:
  * Bump TopShelf from 4.1.0 to 4.2.0

## 0.7.2 - 2018-12-31

* Bugfix for postgres / timescaledb users. This release eschews the asynchronous npgsql APIs in favor of the synchronous ones due to reliability issues.
* Update Npgsql from 4.0.3 to 4.0.4

## 0.7.1 - 2018-11-29

Bugfix for the postgres / timescaledb users who have experienced the rare bug of "26000: prepared statement "\_p1" does not exist" during database operations. It is unknown whether this is a bug in the C# postgres driver or intended behavior. Regardless, OhmGraphite would enter an infinite loop trying to insert sensor data. The fix is to now on db failure, in addition to re-instantiating a connection, to purge all persisted prepared statements.

## 0.7.0 - 2018-10-17

This is strictly a breaking change for [TimescaleDB](https://www.timescale.com/) users (nothing else has changed). Previously OhmGraphite would create a schema and initialize the Timescale table. While convenient, creating tables, indices, etc automatically is not desirable for an application that could be installed on many client machines. In production, one may want to create indices on other columns, omit an index on the `host` column, or create custom constraints. OhmGraphite shouldn't dictate everything. In fact, OhmGraphite should be able to function with a minimal amount of permissions. That is why with the 0.7 release, automatic creation of tables, etc is **opt-in** via the new `timescale_setup` option.

Recommendation: create a user that can **only** insert into the `ohm_stats` table

```sql
CREATE USER ohm WITH PASSWORD 'xxx';
GRANT INSERT ON ohm_stats TO ohm;
```

Then initialize the `ohm_stats` appropriately.

If one desires OhmGraphite to automatically setup the table structure then update the configuration to include `timescale_setup`

```xml
  <add key="timescale_setup" value="true" />
```

A side benefit of this update is that OhmGraphite can now insert into traditional PostgreSQL databases.

## 0.6.0 - 2018-10-07

The big news for this release is [TimescaleDB](https://www.timescale.com/) support, so OhmGraphite can now writes to a PostgreSQL database!

One can configure OhmGraphite to send to Timescale with the following (configuration values will differ depending on your environment):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="type" value="timescale" />
    <add key="timescale_connection" value="Host=vm-ubuntu;Username=postgres;Password=123456;Database=postgres" />
  </appSettings>
</configuration>
```

OhmGraphite will create the following schema, so make sure the user connecting has appropriate permissions

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

All patch notes:

* Add [TimescaleDB](https://www.timescale.com/) support
* Update inner dependencies:
  * Update Topshelf from 4.0.4 to 4.1.0
  * Update pometheus-net from 2.1.0 to 2.1.3
* Switch packing executable from ILRepack to Costura, as ILRepack is unable to pack in Npgsql without type exceptions at runtime.

## 0.5.0 - 2018-07-26

* Add [Prometheus](https://prometheus.io/) support
  * OhmGraphite now requires .NET v4.6.1 (up from .NET v4.6). Most users should be unaffected by this change.
* More metrics! OhmGraphite now takes advantage of metrics that the underlying hardware library detects at runtime.

## 0.4.0 - 2018-07-15

* Update LibreHardwareMonitor to [4652be0](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/tree/4652be058cb263b945bbea3e67dd6c4732f96f06)
  * Support for Ryzen 2000 series processors
* As long as OhmGraphite can send to a given graphite endpoint, keep a persistent tcp connection alive. Previous behavior would open a connection every `interval` seconds. This technique should work for 99% of use cases, but when there are a limited number of ports open on the client load one can receive the error "Only one usage of each socket address (protocol/network address/port) is normally permitted". The new behavior will keep the same connection open until there is a failure, which will then trigger a reconnect.
* Update internal dependencies
  * NLog (4.4.12) -> (4.5.6)
  * TopShelf (4.0.3) -> (4.0.4)

## 0.3.0 - 2018-05-14

* Update LibreHardwareMonitor to [3460ec](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/tree/3460ec7fb27a4c9ac1aec6512364340c4bd38004)
  * Support for NCT6792D (X99)
* Add opt-in to [Graphite tag support](http://graphite.readthedocs.io/en/latest/tags.html)
* Add [InfluxDB](https://www.influxdata.com/) support

This release is backwards compatible. For those interested in Graphite tags -- they are not backwards compatible! While tagged metrics will have the same name as pre-tagged OhmGraphite metrics, Graphite will treat them separately. So one can either merge these metrics or start over.

## 0.2.1 - 2018-05-03

Bugfix for users where their computer culture doesn't use a period `.` as the decimal separator resulting in warnings and no data being stored.

## 0.2.0 - 2018-04-02

Huge shout out to @jonasbleyl who discovered a whole swath of hardware metrics that can be reported!

* Report sub-hardware sensors. For those missing motherboard temperatures, fan controllers, and voltage metrics -- those should now be reported.
* Change logging of sensors without a value from warning to debug as fan controllers often don't have a value and we want to prevent log spam.

## 0.1.3 - 2018-03-07

* Increase logging done under `DEBUG` to aid diagnostics
* Ease upgrades by packing everything into `OhmGraphite.exe`. No need for the .dll files anymore. Upgrading now consists solely of replacing the executable.

## 0.1.2 - 2018-02-06

* Update LibreHardwareMonitor to [22bd00c](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/commit/22bd00c806e4c5175a5ca3013867c5532c06f984)
  * Z270 PC MATE support
  * Add support for Coffee Lake
  * Add support for Fintek F71878AD

## 0.1.1 - 2017-12-10

* Update LibreHardwareMonitor to [60e16046](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/commit/60e1604672e33d95f4e7bb4ddfd31283f2c3efa1)
  * Adds support for PRIME X370-PRO mobo
  * Additional metrics for Ryzen 7, Threadripper, and EPYC CPUs

## 0.1.0 - 2017-11-01

* Initial release
