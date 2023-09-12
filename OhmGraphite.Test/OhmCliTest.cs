using System.Threading.Tasks;
using Xunit;

namespace OhmGraphite.Test
{
    public class OhmCliTest
    {
        [Fact]
        public async Task CanExecuteCliVersion() {
            await OhmCli.Execute(new[] { "--version" });
        }

        [Fact]
        public async Task CanExecuteCliStatusWithOldFlags()
        {
            await OhmCli.Execute(new[] { "status", "-servicename" });
        }
    }
}
