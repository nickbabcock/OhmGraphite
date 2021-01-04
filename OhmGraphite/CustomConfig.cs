using System.Configuration;

namespace OhmGraphite.Test
{
    class CustomConfig : IAppConfig
    {
        private readonly Configuration _config;

        public CustomConfig(Configuration config)
        {
            _config = config;
        }

        public string this[string name] => _config.AppSettings.Settings[name]?.Value;
        public string[] GetKeys() => _config.AppSettings.Settings.AllKeys;
    }
}
