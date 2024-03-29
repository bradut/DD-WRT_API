﻿using DD_WRT_API.Models;
using System;
using System.Net;

namespace DD_WRT_Demo.DisplayModels
{
    public class LanNodeDto : IComparable<LanNodeDto>
    {

        public void Update(DhcpClient dhcpClient)
        {
            HostName = dhcpClient.HostName;
            IpAddress = dhcpClient.IpAddress;
            MacAddress = dhcpClient.MacAddress;
            LeaseTime = dhcpClient.LeaseTime;
        }

        public void Update(ActiveClient activeClient)
        {
            ConnectionsCount = activeClient.ConnectionsCount;
            IsActive = true;

            if (string.IsNullOrWhiteSpace(IpAddress)) IpAddress = activeClient.IpAddress;
            if (string.IsNullOrWhiteSpace(MacAddress)) MacAddress = activeClient.MacAddress;
            if (string.IsNullOrWhiteSpace(HostName)) HostName = activeClient.HostName;
        }

        public void Update(WirelessNode wirelessNode)
        {
            UpTime = wirelessNode.UpTime;
            TxRateMb = wirelessNode.TxRateMb;
            RxRateMb = wirelessNode.RxRateMb;
            SignalLeveldB = wirelessNode.SignalLeveldB;
            NoiseLeveldB = wirelessNode.NoiseLeveldB;
            SignalToNoiseRatiodB = wirelessNode.SignalToNoiseRatiodB;
            SignalQuality = wirelessNode.SignalQuality;
            IsWifi = true;

            if (string.IsNullOrWhiteSpace(MacAddress)) MacAddress = wirelessNode.MacAddress;
        }


        private string HostName { get; set; }
        private string IpAddress { get; set; }
        public string MacAddress { get; set; }

        // Active Client
        private int ConnectionsCount { get; set; }

        // Dhcp Client
        private TimeSpan LeaseTime { get; set; }


        // WiFi
        private TimeSpan UpTime { get; set; }
        private int TxRateMb { get; set; }
        private int RxRateMb { get; set; }
        private int SignalLeveldB { get; set; }
        private int NoiseLeveldB { get; set; }
        private int SignalToNoiseRatiodB { get; set; }
        private int SignalQuality { get; set; }


        public bool IsActive { get; private set; }

        public bool IsWifi { get; private set; }


        public void DisplayGeneralInfo()
        {
            var wiFiToString = IsWifi ? $"{SignalQuality.ToString().PadLeft(3)}%" : "";
            var toString = $"{HostName.Trim().PadRight(20, ' ').Substring(0, 20)}, " +
                               $"{IpAddress.Trim().PadRight(15)}, {MacAddress}, {ConnectionsCount.ToString().PadLeft(3)}, {wiFiToString}";
            var originalFontColor = Console.ForegroundColor;
            var fontColor = IsActive ? ConsoleColor.White : ConsoleColor.DarkGray;
            Console.ForegroundColor = fontColor;
            Console.WriteLine(toString);
            Console.ForegroundColor = originalFontColor;
        }

        public void DisplayActiveNodesInfo()
        {
            var wiFiToString = IsWifi ? $"{SignalQuality.ToString().PadLeft(3)}%" : "";
            var toString = $"{HostName.Trim().PadRight(20, ' ').Substring(0, 20)}, " +
                           $"{IpAddress.Trim().PadRight(15)}, {MacAddress}, {ConnectionsCount.ToString().PadLeft(3)}, {wiFiToString}";

            Console.WriteLine(toString);
        }


        public void DisplayDhcpNodesInfo()
        {
            var wiFiToString = IsWifi ? $"{SignalQuality.ToString().PadLeft(3)}%" : "";
            var toString = $"{HostName.Trim().PadRight(20, ' ').Substring(0, 20)}, " +
                           $"{IpAddress.Trim().PadRight(15)}, {MacAddress}, {LeaseTime:c}  {wiFiToString}";

            Console.WriteLine(toString);
        }


        public void DisplayWiFiNodesInfo()
        {
            var toString = $"{HostName.Trim().PadRight(20, ' ').Substring(0, 20)}, " +
                           $"{IpAddress.Trim().PadRight(15)}, {MacAddress}," +
                         //$"{ Interface}, " +
                           $"{ UpTime.ToString("c").PadLeft(11)}, { TxRateMb.ToString().PadLeft(3)}, { RxRateMb.ToString().PadLeft(3)}, " +
                           //$"{ Info}, " +
            $"{SignalLeveldB.ToString().PadLeft(3)}, {NoiseLeveldB.ToString().PadLeft(4)}, {SignalToNoiseRatiodB.ToString().PadLeft(4)}, {SignalQuality.ToString().PadLeft(3)}%";

            Console.WriteLine(toString);
        }


        public int CompareTo(LanNodeDto other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            var thisIpAddressAsNumber = IPToInt(IpAddress);
            var otherIpAddressAsNumber = IPToInt(other.IpAddress);

            if (thisIpAddressAsNumber == otherIpAddressAsNumber) return 0;
            if (thisIpAddressAsNumber < otherIpAddressAsNumber) return -1;
            return +1;
        }


        private static long IPToInt(string ip)
        {
            IPAddress address = IPAddress.Parse(ip);
            byte[] addressBytes = address.GetAddressBytes();

            // This restriction is implicit in your existing code, but it would currently just lose data...
            if (addressBytes.Length != 4)
            {
                throw new ArgumentException("Must be an IPv4 address");
            }
            int networkOrder = BitConverter.ToInt32(addressBytes, 0);

            return (uint)IPAddress.NetworkToHostOrder(networkOrder);
        }

        public override string ToString()
        {
            return $"{IpAddress} - {HostName} - {MacAddress}";
        }
    }
}
