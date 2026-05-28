using System;
using System.Configuration;

namespace OhmGraphite
{
    class CustomConfig : IAppConfig
    {
        private readonly Configuration _config;

        public CustomConfig(Configuration config)
        {
            _config = config;
        }

        public string this[string name] => Expand(_config.AppSettings.Settings[name]?.Value);
        public string[] GetKeys() => _config.AppSettings.Settings.AllKeys;

        internal static string Expand(string value) =>
            string.IsNullOrEmpty(value) ? value : Environment.ExpandEnvironmentVariables(value);
    }
}