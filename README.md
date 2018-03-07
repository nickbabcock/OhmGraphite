[![Build status](https://ci.appveyor.com/api/projects/status/c8b43wg7qtwicjj6/branch/master?svg=true)](https://ci.appveyor.com/project/nickbabcock/ohmgraphite/branch/master)

# OhmGraphite

OhmGraphite takes the hard work of extracting hardware sensors from [Open Hardware Monitor](http://openhardwaremonitor.org/) (technically [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) for most up to date hardware) and exports the data in a [graphite](https://graphiteapp.org/) compatible format. If you're missing GPU, temperature, or power metrics in [Grafana](https://grafana.com/) or (or other graphite UI), this tool is for you!

## Who's this for?

- People who are familiar with Graphite / Grafana and may have an instance running on their home or cloud server. If you're not familiar with those applications, it may be overwhelming to setup and maintain them. If you're just looking for a UI for hardware sensors, I'd recommend [HWINFO](https://www.hwinfo.com/)
- People who have administrative privileges
- People who know how to execute commands on Windows Command Prompt or other terminal
- People who like lightweight (8MB of RAM and neglible CPU usage), portable (can run off usb), and straightforward applications

## Introduction

OhmGraphite functions as a console app (cross platform) or a Windows service that periodically polls the hardware. My recommendation is that even though OhmGraphite can be run via Mono / Docker, many hardware sensors aren't available in those modes.

Don't fret if this repo hasn't been updated recently. I use this every day to create beautiful dashboards. Keep in mind, Open Hardware Monitor supported components will determine what metrics are available. Below are graphs / stats made strictly from OhmGraphite (additional Windows metrics can be exposed, see [Monitoring Windows system metrics with grafana](https://nbsoftsolutions.com/blog/monitoring-windows-system-metrics-with-grafana))

[![GPU Utilization](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/gpu-utilization.png)](#gpu-utilization)
[![Temperatures](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/temperatures.png)](#temperatures)
[![Power](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/power.png)](#power)
[![Text1](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/text1.png)](#text1)
[![Text2](https://github.com/nickbabcock/OhmGraphite/raw/master/assets/text2.png)](#text2)

## Getting Started (Windows)

- Create a directory that will home base for OhmGraphite (I use C:\Apps\OhmGraphite).
- Download the [latest zip](https://github.com/nickbabcock/OhmGraphite/releases/latest) and extract to our directory.
- Update app configuration (located at `OhmGraphite.exe.config`). The config below polls our hardware every `5` seconds and sends the results to a graphite server listening on `localhost:2003`.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="host" value="localhost" />
    <add key="port" value="2003" />
    <add key="interval" value="5" />
  </appSettings>
</configuration>
```

- This config can be updated in the future, but will require a restart of the app for effect.
- The app can be ran interactively by executing `.\OhmGraphite.exe run`. Executing as administrator *may* make difference in the number of sensors found (OhmGraphite will log how many sensors are found).
- To install the app `.\OhmGraphite.exe install`. The command will install OhmGraphite as a Windows service (so you can manage it with your favorite powershell commands or `services.msc`)
- To start the app after installation: `.\OhmGraphite.exe start` or your favorite Windows service management tool

### Upgrades

- Stop OhmGraphite service `.\OhmGraphite.exe stop`
- Unzip latest release and copy `OhmGraphite.exe` to your installation directory.
- Start OhmGraphite service `.\OhmGraphite.exe start`

## Getting Started (Docker)

Since the full gambit of metrics aren't available in a Docker container, I've refrained from putting the project on docker hub lest it misleads people to think otherwise.

```bash
docker build -t nickbabcock/ohm-graphite .
docker run -v $PWD/app.config:/opt/OhmGraphite/OhmGraphite.exe.config:ro nickbabcock/ohm-graphite
```

`app.config` is in the same format as the above configuration.
