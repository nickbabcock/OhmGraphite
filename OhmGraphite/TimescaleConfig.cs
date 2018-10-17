namespace OhmGraphite
{
    public class TimescaleConfig
    {
        public bool SetupTable { get; }
        public string Connection { get; }

        public TimescaleConfig(string connection, bool setupTable)
        {
            SetupTable = setupTable;
            Connection = connection;
        }

        internal static TimescaleConfig ParseAppSettings(IAppConfig config)
        {
            string connection = config["timescale_connection"];
            if (!bool.TryParse(config["timescale_setup"], out bool setupTable))
            {
                setupTable = false;
            }

            return new TimescaleConfig(connection, setupTable);
        }
    }
}
