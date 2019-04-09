using System.Net;

namespace OhmGraphite
{
    class DnsResolution : INameResolution
    {
        public string LookupName()
        {
            return Dns.GetHostName();
        }
    }
}
