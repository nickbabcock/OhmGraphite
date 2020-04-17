namespace OhmGraphite.Test
{
    class BlankConfig : IAppConfig
    {
        public string this[string name] => null;

        public string[] GetKeys() => new string[] { };
    }
}
