using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.CommandLine;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;

namespace OhmGraphite
{
    public static class OhmCli
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task Execute(string[] args)
        {
            var serviceName = "OhmGraphite";
            var description = "Expose hardware sensor data to Graphite / InfluxDB / Prometheus / Postgres / Timescaledb";
            var configOption = new Option<FileInfo>(name: "--config", description: "OhmGraphite configuration file");
            var rootCommand = new RootCommand(description)
            {
                // To maintain compabitility with old topshelf installations, we should ignore
                // command flags like " -displayname"
                TreatUnmatchedTokensAsErrors = false
            };

            rootCommand.AddOption(configOption);
            rootCommand.SetHandler(async (context) => await OhmCommand(context, configOption));

            var runCommand = new Command("run", "Runs the service from the command line (default)");
            runCommand.AddOption(configOption);
            runCommand.SetHandler(async (context) => await OhmCommand(context, configOption));
            rootCommand.AddCommand(runCommand);

            var statusCommand = new Command("status", "Queries the status of the service");
            statusCommand.SetHandler((context) =>
            {
                if (OperatingSystem.IsWindows())
                {
                    var service = new ServiceController(serviceName);
                    Console.WriteLine(service.Status);
                }
            });
            rootCommand.AddCommand(statusCommand);

            var stopCommand = new Command("stop", "Stops the service if it is running");
            stopCommand.SetHandler((context) =>
            {
                if (OperatingSystem.IsWindows())
                {
                    var service = new ServiceController(serviceName);
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    if (service.Status != ServiceControllerStatus.Stopped)
                    {
                        Console.Error.WriteLine($"Unable to stop {serviceName}");
                    }
                    else
                    {
                        Console.WriteLine($"{serviceName} stopped");
                    }
                }
            });
            rootCommand.AddCommand(stopCommand);

            var startCommand = new Command("start", "Starts the service if it is not already running");
            startCommand.SetHandler((context) =>
            {
                if (OperatingSystem.IsWindows())
                {
                    var service = new ServiceController(serviceName);
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        Console.Error.WriteLine($"Unable to start {serviceName}");
                    }
                    else
                    {
                        Console.WriteLine($"{serviceName} started");
                    }
                }
            });
            rootCommand.AddCommand(startCommand);

            var installCommand = new Command("install", "Installs the service");
            installCommand.AddOption(configOption);
            installCommand.SetHandler((context) =>
            {
                {
                    var procInfo = new ProcessStartInfo();
                    procInfo.ArgumentList.Add("create");
                    procInfo.ArgumentList.Add(serviceName);

                    var configFile = context.ParseResult.GetValueForOption(configOption);
                    var configService = configFile == null ? "" : $@" --config ""{configFile.FullName}""";
                    procInfo.ArgumentList.Add($@"binpath=""{Environment.ProcessPath}"" {configService}");
                    procInfo.ArgumentList.Add(@"DisplayName=Ohm Graphite");
                    procInfo.ArgumentList.Add("start=auto");
                    ScCommand(procInfo);
                }

                {
                    var procInfo = new ProcessStartInfo();
                    procInfo.ArgumentList.Add("description");
                    procInfo.ArgumentList.Add(serviceName);
                    procInfo.ArgumentList.Add(description);

                    ScCommand(procInfo);
                }
            });
            rootCommand.AddCommand(installCommand);

            var uninstallCommand = new Command("uninstall", "Uninstalls the service");
            uninstallCommand.SetHandler((context) =>
            {
                var procInfo = new ProcessStartInfo
                {
                    Arguments = $"delete {serviceName}",
                };

                ScCommand(procInfo);
            });
            rootCommand.AddCommand(uninstallCommand);

            await rootCommand.InvokeAsync(args);
        }

        static async Task OhmCommand(System.CommandLine.Invocation.InvocationContext context, Option<FileInfo> configOption)
        {
            var configFile = context.ParseResult.GetValueForOption(configOption);
            var configPath = configFile == null ? string.Empty : configFile.FullName;
            var configDisplay = configFile == null ? "default" : configFile.Name;
            var config = Logger.LogFunction($"parse config {configDisplay}", () => MetricConfig.ParseAppSettings(CreateConfiguration(configPath)));

            if (WindowsServiceHelpers.IsWindowsService())
            {
                var builder = Host.CreateDefaultBuilder()
                    .UseWindowsService(options =>
                    {
                        options.ServiceName = "OhmGraphite";
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton(config);
                        services.AddHostedService<Worker>();
                    });

                builder.Build().Run();
            }
            else
            {
                var token = context.GetCancellationToken();
                var worker = new Worker(config);
                await worker.StartAsync(token);
                await worker.ExecuteTask;
            }
        }

        private static int ScCommand(ProcessStartInfo procInfo)
        {
            procInfo.FileName = "sc.exe";
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError = true;
            procInfo.CreateNoWindow = true;
            procInfo.UseShellExecute = false;

            using var process = new Process() { StartInfo = procInfo };
            process.Start();
            process.WaitForExit();
            var error = process.StandardError.ReadToEnd();
            var stdout = process.StandardOutput.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
            }

            if (!string.IsNullOrEmpty(stdout))
            {
                Console.WriteLine(stdout);
            }

            return process.ExitCode;
        }

        private static IAppConfig CreateConfiguration(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                // https://github.com/dotnet/runtime/issues/13051#issuecomment-510267727
                var processModule = Process.GetCurrentProcess().MainModule;
                if (processModule != null)
                {
                    var pt = processModule.FileName;
                    var fn = Path.Join(Path.GetDirectoryName(pt), "OhmGraphite.exe.config");
                    var configMap1 = new ExeConfigurationFileMap { ExeConfigFilename = fn };
                    var config1 = ConfigurationManager.OpenMappedExeConfiguration(configMap1, ConfigurationUserLevel.None);
                    return new CustomConfig(config1);
                }
            }

            if (!File.Exists(configPath))
            {
                throw new ApplicationException($"unable to detect config: ${configPath}");
            }

            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            return new CustomConfig(config);
        }
    }
}
