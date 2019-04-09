using System;

namespace OhmGraphite
{
    class NetBiosResolution : INameResolution
    {
        public string LookupName()
        {
            return Environment.MachineName;
        }
    }
}
