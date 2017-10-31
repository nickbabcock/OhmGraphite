# OhmGraphite

OhmGraphite takes the hard work of extracting hardware sensors from [Open Hardware Monitor](http://openhardwaremonitor.org/) (technically [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) for most up to date hardware) and exports the data in a [graphite](https://graphiteapp.org/) compatible format. If you're missing GPU, temperature, or power metrics in Grafana or (or other graphite UI), this tool is for you!

OhmGraphite functions as a console app (cross platform) or a Windows service that periodically polls the hardware. My recommendation is that even though OhmGraphite can be run via Mono / Docker, many hardware sensors aren't available in those modes.

Don't fret if this repo hasn't been updated recently. I use this every day to create beautiful dashboards. Keep in mind, Open Hardware Monitor supported components will determine what metrics are available. Below are graphs / stats made strictly from OhmGraphite (additional Windows metrics can be exposed, see [Monitoring Windows system metrics with grafana](https://nbsoftsolutions.com/blog/monitoring-windows-system-metrics-with-grafana))

## Getting Started (Windows)

- Create a directory that will home base for OhmGraphite (I use C:\Apps\OhmGraphite).
- Download the latest zip and extract to our directory
- Update app configuration (located at `OhmGraphite.exe.config`) to include the host and port of graphite and the hardware polling interval. This config can be updated, but a restart of the app is needed for affect

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

- The app can be ran interactively by simply executing `OhmGraphite.exe`
- To install the app `.\OhmGraphite.exe install`. The command will install OhmGraphite as a Windows service (so you can manage it with your favorite powershell commands or `services.msc`)
- To start the app after installation: `.\OhmGraphite.exe start` or your favorite Windows service management tool

## Getting Started (Docker)

Since the full gambit of metrics aren't available in a Docker container, I've refrained from putting the project on docker hub lest it misleads people to think otherwise.

```bash
docker build -t nickbabcock/ohm-graphite .
docker run -v $PWD/app.config:/opt/OhmGraphite/OhmGraphite.exe.config:ro nickbabcock/ohm-graphite
```

`app.config` is in the same format as the above configuration.
