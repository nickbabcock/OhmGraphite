namespace OhmGraphite
{
    class StaticResolution : INameResolution
    {
        private readonly string _lookup;

        public StaticResolution(string lookup) => _lookup = lookup;

        public string LookupName() => _lookup;
    }
}