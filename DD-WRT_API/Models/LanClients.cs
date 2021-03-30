using System;

namespace DD_WRT_API.Models
{
    public class LanClientBase
    {
        public string HostName{ get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }

        public override string ToString()
        {
            return $"{HostName.Trim().PadRight(20,' ').Substring(0,20)}, " +
                   $"{IpAddress.Trim().PadRight(15)}, {MacAddress}";
        }
    }

    public class ActiveClient : LanClientBase
    {
        public int ConnectionsCount { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {ConnectionsCount.ToString().PadLeft(3)}";
        }
    }

    public class DhcpClient : LanClientBase
    {
        public TimeSpan LeaseTime { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {LeaseTime}";
        }
    }
}
