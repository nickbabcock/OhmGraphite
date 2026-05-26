using System;
using System.Configuration;
using System.Text.RegularExpressions;
using NLog;

namespace OhmGraphite
{
    class CustomConfig : IAppConfig
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Matches ${NAME} where NAME is a typical env-var identifier. 
        // %NAME% is handled separately by Environment.ExpandEnvironmentVariables.
        private static readonly Regex DollarBraceVar = new Regex(
            @"\$\{([A-Za-z_][A-Za-z0-9_]*)\}",
            RegexOptions.Compiled);

        private readonly Configuration _config;

        public CustomConfig(Configuration config)
        {
            _config = config;
        }

        public string this[string name] => Expand(name, _config.AppSettings.Settings[name]?.Value);
        public string[] GetKeys() => _config.AppSettings.Settings.AllKeys;

        internal static string Expand(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            // %VAR%: native Windows form. Unknown names are left literal by
            // ExpandEnvironmentVariables, so we don't get warnings on those,
            // but that's the documented Windows behavior and users get the
            // literal back, which is easy to spot.
            var expanded = Environment.ExpandEnvironmentVariables(value);

            // ${VAR}: cross-shell/Docker form. 
            // Unset variables resolve to an empty string and warn.
            expanded = DollarBraceVar.Replace(expanded, match =>
            {
                var name = match.Groups[1].Value;
                var resolved = Environment.GetEnvironmentVariable(name);
                if (resolved == null)
                {
                    Logger.Warn("config key '{0}' references unset environment variable '{1}'; expanding to empty string", key, name);
                    return string.Empty;
                }
                return resolved;
            });

            return expanded;
        }
    }
}
