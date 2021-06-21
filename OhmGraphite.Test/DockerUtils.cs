using System;
using System.Runtime.InteropServices;
using Xunit;

namespace OhmGraphite.Test
{
    public static class DockerUtils
    {
        public static string DockerEndpoint()
        {
            var host = Environment.GetEnvironmentVariable("DOCKER_HOST");
            if (host != null)
            {
                return host;
            }

            // https://github.com/HofmeisterAn/dotnet-testcontainers/tree/aa3d573129045a26de08e1fe57ccc180b1e04108/src/DotNet.Testcontainers/Services
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "unix:/var/run/docker.sock"
                : "npipe://./pipe/docker_engine";
        }
    }

    public sealed class IgnoreOnRemoteDockerFactAttribute : FactAttribute
    {
        public IgnoreOnRemoteDockerFactAttribute()
        {
            if (Environment.GetEnvironmentVariable("DOCKER_HOST") != null)
            {
                Skip = "Ignore when executing against a remote docker instance";
            }
        }
    }
}