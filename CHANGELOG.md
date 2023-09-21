## 0.31.0 - 2023-09-21

- Add prometheus metrics path customization
- Replace unmaintained windows service management library 
    - Should be a drop in replacement, no migration required
    - The basic `start`, `stop`, `install`, `uninstall` functions the same
    - Users who desire custom installs may need to directly use `sc.exe` now.
- Update LibreHardwareMonitor to latest:
    - Add Total Board Power for RX 7000 Cards
    - Add support for ASUS Z790-I GAMING WIFI with EC
    - Add support for Gigabyte Z690 Aorus Ultra
    - Add support for Asus ROG Crosshair X670E Hero
    - Add support for MS-7672 and MS-7757 boards
    - Fix it8665e control fans
    - Add CPU fan control for P55A-UD3
    - Add IT8686E Controls

## 0.30.0 - 2023-07-06

- HTTPS support for Prometheus Exporter
- Allow hiding sensors by sensor name
- Update LibrehardwareMonitor to latest:
    - Add support for Razer PWM PC Fan Controller
    - Add support for IT87952E EC on Gigabyte boards
    - Add partially support for MS-7751 boards (F71889AD)
    - Add support for ASUS ROG CROSSHAIR X670E GENE
    - Add support for Gigabyte B75M-D3H
    - Add support for ASUS ROG Z390-E Gaming Motherboard
    - Added fan control support for the EVGA X58 3X SLI motherboard
    - Create virtual sensor for maximum CPU load across all cores
    - Support asus rog strix z390-i gaming embedded controller
    - Add support for Gigabyte B550 Aorus Pro sensors
    - Support Asus ROG STRIX Z390-F GAMING
    - Add support for IPMI sensors and Supermicro IPMI fan control

### Migration Guide

To hide the new CPU Core Max sensor, one can leverage the new ability to hide sensors by sensor name:

```xml
<add key="CPU Core Max/hidden" />
```

## 0.29.0 - 2023-03-16

- [Updates LibrehardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/compare/58cef11...1131b4)
  - Fix for Windows 11 22H2 (22621) incorrect total CPU load issue
  - Fix ASUS X470-I incontrollable fans
  - Added ASUS ROG Maximus Z690 Hero - SuperIO sensors
  - Add Support IT8625E and X670E Valkyrie
  - Add support for ASRock Z790 Taichi/Z790 Taichi Carrara
  - Fix sensor asrock sensor detection
  - Added ROG Maximus X Hero (Wifi AC) - Super IO sensors
  - Added support for KrakenZ Devices
  - Cleanup thread affinity and support > 64 threads.
  - Read storage performance sensors without WMI
  - Fix battery underflow
  - Fix incorrect GPU temperature calculation
- Update client libraries:
  - postgres client from 7.0.0 to 7.0.2
  - influxdb client from 4.9.0 to 4.11.0

## 0.28.1 - 2022-12-11

- Revert update of prometheus client that was causing high CPU load

## 0.28.0 - 2022-12-11

- [Updates LibrehardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/compare/58cef11...1131b4)
  - Fix incorrect readings of GPU power wattage
  - Add support for Zen4 SMU
  - Add support for HX1500i and HX1000i
  - Add support for Nuvoton NCT6799D
  - Add support for Asus Z790 Max Hero
- Update client libraries:
  - prometheus client from 6 to 7
  - postgres client from 6.0.7 to 7
  - influxdb client from 4.6 to 4.9

## 0.27.0 - 2022-10-11

[Updates LibrehardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/compare/ae3bc2771978f458ac97efe8fc16ecab11f14fbb...58cef111a12e541bb3d1c1723a05fc4c543fb045)

- Add NZXT GRID+ V3 support
- Fix Gigabyte Z690 AORUS PRO sensor names
- Add RaptorLake
- Add more Alder Lake

## 0.26.0 - 2022-07-29

[Updates LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/compare/a51f5dbb7f994922109b4dc0d4b3f120c5e7ab6e...ae3bc2771978f458ac97efe8fc16ecab11f14fbb)

- [Fix for missing Samsung NVMe sensors](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/commit/7ab8fbb1c1bd4d418ff4c23960da8cdd14aefacf)
- [Add Aqua Computer Octo support](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/commit/b0de6e3ad762c18083b9198bd1982537e7c0f16c)
- [Add support for VID sensors per core on Intel CPUs](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/commit/eed7742e7eff26d343ada300c5d83eef7ac7d352)
- [ROG Zenith II Extreme support](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/commit/67d82e0784c8f0963202b4936322738f3914c207)
- [Add support for ROG MAXIMUS Z690 EXTREME GLACIAL](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/commit/42182d72f748329db8d8e70e0b094b9afca7baea)
- [RyzenSMU: add some more metrics for Zen3](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/commit/4b0bf583e1b0a840b39afecdf230eea50bd9ed82)

Updates to internal dependencies for influx2 and postgres

## 0.25.0 - 2022-05-08

[Updates LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/compare/f0dd72add999ac1e92f178a10e326be2de607c00...v0.9.0)

- Add Intel integrated GPU sensors
- Add support for B560M AORUS PRO and B560M AORUS PRO AX
- Better detection of Samsung NVMe drives
- Support Z690 Aorus Pro
- Add EC T_Sensor for ROG Strix Z690-A
- "CPU Core" voltage sensor for intel CPU's
- IT8613E and Biostar B660GTN support
- Add X570 Aorus Ultra
- Add Gigabyte Z690 Gaming X

## 0.24.0 - 2022-02-18

- Add support for PSU metrics (enabled by default)
- Add support for battery metrics (enabled by default)
- Fix rare null reference exception on OhmNvme updates
- Update minor dependencies to latest

Update LibrehardwareMonitor from [d3a38bf...f0dd72a](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/compare/d3a38bfa9aed1e23b1ce6b43acbe4156f4d6f481...f0dd72a).

- Add support for reporting battery metrics
- Add support for Zen 3 SMU (System Management Unit) reporting
- Add support for ASUS ROG CROSSHAIR VIII HERO (WIFI)
- Add support for the Gigabyte B360 AORUS GAMING 3 WIFI-CF mainboard

## 0.23.1 - 2021-12-23

Bugfix for postgres / timescale users where insertions would fail due to the
server expecting a datetime in UTC but OhmGraphite was sending a datetime in
the local timezone.

## 0.23.0 - 2021-12-15

The OhmGraphite download size is double the previous version but the disk size
after extraction is approximately the same. OhmGraphite has been upgraded to
NET 6.0, which shouldn't be user visible; however, changes in executable
trimming forced a migration to a new way to bundle OhmGraphite in order to keep
a reasonable executable size. This should all be transparent to the user, but
this was brought up in case issues are seen with this new version (ie: startup
performance may have worsened so I appreciate any reports about this).

For LibreHardwareMonitor, the headlining change is that Alder Lake and Jasper
Lake are better supported. The other changes are [mostly
minor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/compare/1701eb9...d3a38bf)

Addtionally, a CLI flag was added for OhmGraphite to return its version:

```ps
.\OhmGraphite.exe run --version
```

## 0.22.0 - 2021-10-29

Add configuration to disable polling of hardware:

```xml
<add key="/gpu/enabled" value="false" />
```
Update LibreHardwareMonitor from [ce882e...1701eb](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/compare/ce882e...1701eb):

- Add GPU hot spot temperature
- Fixes incorrect GPU fan readings for certain NVIDIA models
- Add support for the embedded Asus B550-I controller

## 0.21.0 - 2021-10-06

Maintenance release that updates internal libraries including
LibreHardwareMonitor from
[07beb4f...d14f1aa84](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/compare/07beb4f...ce882e3),
which contains the following fixes:

- Fix Nvidia memory report where free and used memory were mixed
- Additional ASUS EC detection
- Add Gigabyte X570 Gaming X motherboard
- Add ASUS ROG Strix B550-I
- Add ASUS Cross VIII Formula motherboard
- Fix no video card temps detected for AMD 5700XT

## 0.20.0 - 2021-07-22

Allow hiding of sensors by glob. One can use globs to ignore a group of
sensors. For instance, to hide all power sensors and hide clock sensors from an
AMD CPU:

```xml
<add key="/amdcpu/*/clock/*/hidden" />
<add key="/*/power/*/hidden" />
```

LibreHardwareMonitor (ie the sensor library) updated to latest:

- Add D3D GPU Sensors
- Add sensors for ASUS Crosshair VIII Hero
- Fixes for ASUS C8H
- Add embedded controller sensors for ROG STRIX X570-E GAMING
- Fix AMD overdrive 8
- Add ADL (fan) Sensor support
- Add Ryzen SMU support
- Add PSU sensors (only corsair)
- Increased timeouts for WMI querying HDD data
- Support reporting multiple GPU fans

Changes detected on test system. Mileage will vary based on hardware:

- Ryzen CPU package power renamed to CPU package
- Nvidia GPU owners should see many new metrics (test system saw an increase of 5 to 24 load sensors).
- All Nvidia GPU sensors appear to be reordered and so may show up as distinct metrics in your time series database of choice.

## 0.19.0 - 2021-05-29

- Add influxdb 2 support (see docs for more information)
- Add custom certificate verification for self signed cert setups (see docs for more information)
- Fix slow WMI disk operations causing no sensors to be reported
- Fix crash for intel nvme drives

## 0.18.1 - 2021-05-09

Fix regression introduced with 0.18.0 where Intel NVMe drives would cause a
failure to start.

## 0.18.0 - 2021-05-08

A purposely small release to test the new distribution. OhmGraphite is now
packaged as a single executable that does not depend on the system's .NET
framework. This should allow more users to access OhmGraphite, but I'm not sure
I've tracked all the downsides. One of the major downsides is that the
executable size has increased from 3MB to 30MB.

- Bump LibreHardwareMonitor to latest
  - Intel Gen11 Rocket Lake Support
  - Enable reporting SoC voltage for Zen+ APUs
  - Fix AMD Overdrive5 detection

## 0.17.0 - 2021-03-22

- Allow configuration file parameter when run interactively
- Bump LibreHardwareMonitor to latest
  - Add support for ASRock X570 Phantom Gaming-ITX/TB3 (NCT6683D)
  - Add support for ASRock X570 Taichi
  - Add support for Asus X470-I Gaming
  - Add fan control for IT8728F
  - Add fan control to Z390 M Gaming IT8688E
  - Fix readings for Samsung 980 Pro SSD
- Bump internal dependencies

## 0.16.0 - 2020-11-29

- Allow omission of password in config file for passwordless influxdb user
- Bump LibreHardwareMonitor to latest
  - Better support for Ryzen 5000
  - More accurate Ryzen 3000 CCD Temperatures
  - Add Processor Cache to SMBios
  - Fix possible exception when waking from standby
  - Added support for NCT6687D
  - Fix network exception when disabling IPv4
- Update internal client dependencies (prometheus and postgres) to latest major versions.

### Fix Graphite Connection Write Contention

If polling for sensors + writing to graphite takes longer than the
configured interval, then it is possible for two threads to be writing
data to the same graphite connection. This will cause corrupted data to
be sent to graphite.

The fix is the limit the number of threads that have access to the
connection to 1. Other threads that would have caused contention are
forced to wait outside. To ensure there is not an unbounded number of
threads waiting to write, if they can't acquire a lock in a second then
they jettison the write attempt.

## 0.15.0 - 2020-06-04

Two highlights from this release:

### Hiding Sensors

There may be a sensor that is faulty on a given machine. Maybe it reports negative temperatures. This can throw off monitoring software or make it harder to maintain with all the special cases. OhmGraphite allows one to exclude a sensor from being exported by modifying the OhmGraphite config and adding the `/hidden` suffix to the sensor id like so:

```xml
<add key="/lpc/nct6792d/temperature/1/hidden" />
```

### Sensor Library Update

LibreHardwareMonitor has received a significant amount of work in the past month. The most jarring change will probably be that several hardware types (Aquacomputer, AeroCool, Heatmaster, and TBalancer) have been consolidated into a single hardware type (Cooler). This is will be an inconvenience to users relying on those metric names.

Another inconvenience is that GPU sensor identifiers have also been updated, so depending on the dashboard, GPU graphs may need to be updated to reference this new identifier.

In the end, for my own personal dashboards which are derived from the sample ones listed in the readme, the only change I've noticed is that the CPU bus speed is reported alongside CPU frequencies.

## 0.14.0 - 2020-04-26

A very minor release, but since the underlying sensor library has been updated in the meantime, the usual caveats regarding new / renamed metrics apply, though in this case most users shouldn't notice anything different. The most impactful change in these last few days is the bugfix in detection of NVMe drives. While some could be detected perfectly fine (like a HP EX920), others would only be detected as a generic hard drive (like the newer HP EX950). This release should now detect more NVMe drives than previously, and as a result more metrics will be available.

## 0.13.0 - 2020-04-19

A few features in this update:

- Allow exporting of aliased metric names
- Expose some missing NVMe SMART attributes
- Bump to LibreHardwareMonitor

### Aliasing

Aliasing was introduced as it is possible that the sensor names exposed through OhmGraphite are not descriptive enough. For instance, "Fan #2" could have RPM exposed, but you know that a more descriptive name would be "CPU Fan". To have OhmGraphite export the sensor under the "CPU Fan" name, one will need to add the mapping from sensor id (+ `/name` suffix) to the desired name like so:

```xml
    <add key="/lpc/nct6792d/fan/1/name" value="CPU Fan" />
```

There are several ways to determine the sensor id of a metric:

- Postgres / Timescale and Influxdb users can examine their data store for the sensor id
- Perform the rename in LibreHardwareMonitor and copy and paste the line from `LibreHardwareMonitor.config` into `OhmGraphite.exe.config`.
- Search the `OhmGraphite.log` for the sensor's name that you'd like to rename (in the example, I'd search for "Fan #2"):

### LibreHardwareMonitor Update

The underlying sensor library has been updated, so as always there may be new
sensors, renames, and bugfixes.

Some updates:

- Fix AMD Missing GPU Temperature
- Add support for Intel Comet Lake
- Improve IT8655E support

For my personal dashboard:

- new sensor "Nvidia GPU Bus Load"
- motherboard 3VCC voltage sensor was renamed to +3.3V

### NVMe SMART attributes

NVMe drives expose SMART attributes but not all are transmitted. In this
update, OhmGraphite is now exposing the following additional NVMe SMART
sensors:

- Error Info Log Entry Count
- Media Errors
- Power Cycles
- Unsafe Shutdowns

This doesn't cover all NVMe SMART attributes, so if there is one missing feel
free to raise an issue.

## 0.12.0 - 2020-02-22

This is a smaller update to the sensor library: LibreHardwareMonitor, but this
is still being denoted in OhmGraphite as a minor version bump to communicate
that metric values may have changed. Here are the changes to the sensor
library.

- Add AMD Zen 2 CCD temperatures
- Fix possible exceptions when waking from sleep
- Capture fan rpm sensors on motherboards even if they are at 0 rpm
- Support for 7th fan for NCT6797 and NCT6798
- Support for Asrock X570 Pro4 (NCT6796D-R)
- Support for X570 AORUS MASTER
- The "Standby +3.3V" voltage sensor has been renamed "3VSB"
- Fix swapped sensor readings for Nvidia GPUs in SLI

This had the following effects on a personal dashboard (AMD 2700, gtx 1070, m.2, asrock itx):

- Even though I only have a single fan connected to the mobo (cpu fan), I now see 5 more fans (all set to 0 rpm), so with more exposed sensors, there will be more disk space usage (usually this is a non-issue, but having "useless" sensors take up disk space can be disconcerting)
- Nonsensical temperatures are no longer reported for my mobo (I had one being reported at -50 degrees)
- Even though I have a Zen 1 cpu, Core (Tdie) and Core (Tctl) were still combined under Core (Tctl/Tdie)

The only other change for this release is a internal dependency bump to the Postgres / TimecaleDB driver that ensures better stability.

You may have missed it, but there are now starter dashboards for visualizing OhmGraphite data.

- [Prometheus](https://grafana.com/grafana/dashboards/11587)
- [Graphite](https://grafana.com/grafana/dashboards/11591)
- [Postgres / Timescale](https://grafana.com/grafana/dashboards/11599)
- [Influxdb](https://grafana.com/grafana/dashboards/11601)

These are designed to jump start your own dashboard!

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
