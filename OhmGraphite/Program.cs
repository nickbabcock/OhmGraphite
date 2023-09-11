using System.Threading.Tasks;

namespace OhmGraphite
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await OhmCli.Execute(args);
        }
    }
}