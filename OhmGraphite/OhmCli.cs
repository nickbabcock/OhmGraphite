using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.CommandLine;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;

namespace OhmGraphite
{
    public static class OhmCli
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string SampleConfig = ReadResource("SampleConfig.xml");
        private static readonly string SampleNLogConfig = ReadResource("SampleNLog.xml");

        private static string ReadResource(string name)
        {
            using var stream = typeof(OhmCli).Assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"missing embedded {name} resource");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static async Task Execute(string[] args)
        {
            var serviceName = "OhmGraphite";
            var description = "Expose hardware sensor data to Graphite / InfluxDB / Prometheus / Postgres / Timescaledb";
            var configOption = new Option<FileInfo>("--config")
            {
                Description = "OhmGraphite configuration file"
            };
            var rootCommand = new RootCommand(description)
            {
                // To maintain compabitility with old topshelf installations, we should ignore
                // command flags like " -displayname"
                TreatUnmatchedTokensAsErrors = false
            };

            rootCommand.Options.Add(configOption);
            rootCommand.SetAction(async (parseResult, cancellationToken) => await OhmCommand(parseResult, configOption, cancellationToken));

            var runCommand = new Command("run", "Runs the service from the command line (default)");
            runCommand.Options.Add(configOption);
            runCommand.SetAction(async (parseResult, cancellationToken) => await OhmCommand(parseResult, configOption, cancellationToken));
            rootCommand.Subcommands.Add(runCommand);

            var statusCommand = new Command("status", "Queries the status of the service");
            statusCommand.SetAction((parseResult) =>
            {
                if (OperatingSystem.IsWindows())
                {
                    var service = new ServiceController(serviceName);
                    Console.WriteLine(service.Status);
                }
            });
            rootCommand.Subcommands.Add(statusCommand);

            var stopCommand = new Command("stop", "Stops the service if it is running");
            stopCommand.SetAction((parseResult) =>
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
            rootCommand.Subcommands.Add(stopCommand);

            var startCommand = new Command("start", "Starts the service if it is not already running");
            startCommand.SetAction((parseResult) =>
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
            rootCommand.Subcommands.Add(startCommand);

            var installCommand = new Command("install", "Installs the service");
            installCommand.Options.Add(configOption);
            installCommand.SetAction((parseResult) =>
            {
                {
                    var procInfo = new ProcessStartInfo();
                    procInfo.ArgumentList.Add("create");
                    procInfo.ArgumentList.Add(serviceName);

                    var configFile = parseResult.GetValue(configOption);
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
            rootCommand.Subcommands.Add(installCommand);

            var uninstallCommand = new Command("uninstall", "Uninstalls the service");
            uninstallCommand.SetAction((parseResult) =>
            {
                var procInfo = new ProcessStartInfo
                {
                    Arguments = $"delete {serviceName}",
                };

                ScCommand(procInfo);
            });
            rootCommand.Subcommands.Add(uninstallCommand);

            var directoryArgument = new Argument<DirectoryInfo>("directory")
            {
                Description = "Directory to write the configuration files to (default: current directory)",
                Arity = ArgumentArity.ZeroOrOne,
                DefaultValueFactory = _ => new DirectoryInfo(Directory.GetCurrentDirectory())
            };
            var forceOption = new Option<bool>("--force")
            {
                Description = "Overwrite the configuration files if they already exist"
            };

            var initCommand = new Command("init", "Writes the initial configuration files (OhmGraphite.exe.config and NLog.config) to a directory");
            initCommand.Arguments.Add(directoryArgument);
            initCommand.Options.Add(forceOption);
            initCommand.SetAction((parseResult) =>
            {
                var directory = parseResult.GetValue(directoryArgument);
                var force = parseResult.GetValue(forceOption);

                if (!directory.Exists)
                {
                    Console.Error.WriteLine($"{directory.FullName} does not exist.");
                    Environment.ExitCode = 1;
                    return;
                }

                // Write each file independently, so that a missing file is created even if the other one exists
                WriteInitFile(Path.Join(directory.FullName, "OhmGraphite.exe.config"), SampleConfig, force);
                WriteInitFile(Path.Join(directory.FullName, "NLog.config"), SampleNLogConfig, force);
            });
            rootCommand.Subcommands.Add(initCommand);

            var parseResult = rootCommand.Parse(args);
            await parseResult.InvokeAsync();
        }

        static async Task OhmCommand(System.CommandLine.ParseResult parseResult, Option<FileInfo> configOption, CancellationToken cancellationToken = default)
        {
            var configFile = parseResult.GetValue(configOption);
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
                var token = cancellationToken;
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

        private static void WriteInitFile(string path, string contents, bool force)
        {
            if (File.Exists(path) && !force)
            {
                Console.Error.WriteLine($"{path} already exists. Use --force to overwrite it.");
                Environment.ExitCode = 1;
                return;
            }

            File.WriteAllText(path, contents);
            Console.WriteLine($"Wrote {path}");
        }

        private static string DefaultConfigPath() =>
            Path.Join(Path.GetDirectoryName(Environment.ProcessPath), "OhmGraphite.exe.config");

        private static IAppConfig CreateConfiguration(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                var fn = DefaultConfigPath();
                if (!File.Exists(fn))
                {
                    throw new ApplicationException($"unable to detect config: {fn}. Run 'OhmGraphite.exe init' to create one.");
                }

                var configMap1 = new ExeConfigurationFileMap { ExeConfigFilename = fn };
                var config1 = ConfigurationManager.OpenMappedExeConfiguration(configMap1, ConfigurationUserLevel.None);
                return new CustomConfig(config1);
            }

            if (!File.Exists(configPath))
            {
                throw new ApplicationException($"unable to detect config: {configPath}. Run 'OhmGraphite.exe init' to create one.");
            }

            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            return new CustomConfig(config);
        }
    }
}
