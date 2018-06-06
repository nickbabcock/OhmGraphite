using System.Configuration;
  
namespace OhmGraphite
{
    class AppConfigManager : IAppConfig
    {
        public string this[string name] => ConfigurationManager.AppSettings[name];
    }
}
