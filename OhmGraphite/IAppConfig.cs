namespace OhmGraphite
{
    public interface IAppConfig
    {
        string this[string name] { get; }
    }
}
