using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace OhmGraphite.Test
{
    public class OhmCliTest
    {
        [Fact]
        public async Task CanExecuteCliVersion()
        {
            await OhmCli.Execute(new[] { "--version" });
        }

        [Fact]
        public async Task CanExecuteCliStatusWithOldFlags()
        {
            await OhmCli.Execute(new[] { "status", "-servicename" });
        }

        [Fact]
        public async Task InitWritesConfigFiles()
        {
            var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            try
            {
                await OhmCli.Execute(new[] { "init", dir.FullName });
                Assert.True(File.Exists(Path.Combine(dir.FullName, "OhmGraphite.exe.config")));
                Assert.True(File.Exists(Path.Combine(dir.FullName, "NLog.config")));
            }
            finally
            {
                dir.Delete(true);
            }
        }

        [Fact]
        public async Task InitDoesNotOverwriteWithoutForce()
        {
            var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            var configPath = Path.Combine(dir.FullName, "OhmGraphite.exe.config");
            var nlogPath = Path.Combine(dir.FullName, "NLog.config");
            try
            {
                await File.WriteAllTextAsync(configPath, "sentinel");
                await File.WriteAllTextAsync(nlogPath, "sentinel");
                await OhmCli.Execute(new[] { "init", dir.FullName });
                Assert.Equal("sentinel", await File.ReadAllTextAsync(configPath));
                Assert.Equal("sentinel", await File.ReadAllTextAsync(nlogPath));

                await OhmCli.Execute(new[] { "init", dir.FullName, "--force" });
                Assert.NotEqual("sentinel", await File.ReadAllTextAsync(configPath));
                Assert.NotEqual("sentinel", await File.ReadAllTextAsync(nlogPath));
            }
            finally
            {
                Environment.ExitCode = 0;
                dir.Delete(true);
            }
        }

        [Fact]
        public async Task InitFailsForMissingDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                await OhmCli.Execute(new[] { "init", path });
                Assert.False(Directory.Exists(path));
            }
            finally
            {
                Environment.ExitCode = 0;
            }
        }
    }
}
