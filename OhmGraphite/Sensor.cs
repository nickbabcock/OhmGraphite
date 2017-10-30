namespace OhmGraphite
{
    public class Sensor
    {
        public string Identifier { get; }
        public string Name { get; }
        public float Value { get; }

        public Sensor(string identifier, string name, float value)
        {
            Identifier = identifier;
            Name = name;
            Value = value;
        }
    }
}
