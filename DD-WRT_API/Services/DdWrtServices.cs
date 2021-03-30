using System;
using System.Collections.Generic;
using System.Globalization;
using DD_WRT_API.Models;
using log4net;
using log4net.Config;

namespace DD_WRT_API.Services
{
    public class DdWrtServices
    {
        private readonly IRouterConnectionService _routerConnectionService;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DdWrtServices).FullName);

        public DdWrtServices(IRouterConnectionService routerConnectionService)
        {
            _routerConnectionService = routerConnectionService;
            XmlConfigurator.Configure();
        }

        public IEnumerable<WirelessNode> GetWirelessStatus()
        {
            const string webPage = "Status_Wireless.live.asp";
            const string startString = "{active_wireless::";
            const int wirelessNodesRecordColumns = 10;

            string pageContent = _routerConnectionService.GetPageContent(webPage);

            IEnumerable<WirelessNode> wirelessNodes = GetWirelessStatusFromContent(pageContent, startString, wirelessNodesRecordColumns);

            return wirelessNodes;
        }

        public IEnumerable<ActiveClient> GetLanActiveClients()
        {
            const string webPage = "Status_Lan.live.asp";
            const string activeClientsStartString = "{arp_table:: ";
            const int activeClientsRecordColumns = 4;

            string pageSource = _routerConnectionService.GetPageContent(webPage);

            IEnumerable<ActiveClient> clients = GetLanActiveClientsFromContent(pageSource, activeClientsStartString, activeClientsRecordColumns);

            return clients;
        }

        public IEnumerable<DhcpClient> GetLanDhcpClients()
        {
            const string webPage = "Status_Lan.live.asp";
            const string dhcpClientsStartString = "{dhcp_leases::";
            const int dhcpClientsRecordColumns = 5;

            string pageSource = _routerConnectionService.GetPageContent(webPage);

            IEnumerable<DhcpClient> clients = GetLanDhcpClientsFromContent(pageSource, dhcpClientsStartString, dhcpClientsRecordColumns);

            return clients;
        }

        public (bool, string) Reboot()
        {
            try
            {
                return (_routerConnectionService.Reboot(), "");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return (false, ex.Message);
            }
        }



        private static IEnumerable<WirelessNode> GetWirelessStatusFromContent(string pageSource, string startString, int rowArrayLength)
        {
            var wirelessNodes = new List<WirelessNode>();

            IEnumerable<string[]> rowValues = GetRowValues(pageSource, startString, rowArrayLength);

            foreach (string[] rowValue in rowValues)
            {
                var i = 0;
                var node = new WirelessNode
                {
                    MacAddress = rowValue[i++],
                    Interface = rowValue[i++],
                    UpTime = GetTimeSpan(rowValue[i++]),
                    TxRateMb = int.Parse(rowValue[i++].Replace("M", "")),
                    RxRateMb = int.Parse(rowValue[i++].Replace("M", "")),
                    Info = rowValue[i++],
                    SignalLeveldB = int.Parse(rowValue[i++]),
                    NoiseLeveldB = int.Parse(rowValue[i++]),
                    SignalToNoiseRatiodB = int.Parse(rowValue[i++]),
                    SignalQuality = int.Parse(rowValue[i]) / 10
                };
                wirelessNodes.Add(node);
            }

            return wirelessNodes;
        }

        private static IEnumerable<ActiveClient> GetLanActiveClientsFromContent(string pageSource, string startString, int rowArrayLength)
        {
            IEnumerable<string[]> rowValues = GetRowValues(pageSource, startString, rowArrayLength);

            var clients = new List<ActiveClient>();

            foreach (string[] rowValue in rowValues)
            {
                var i = 0;
                var client = new ActiveClient
                {
                    HostName = rowValue[i++],
                    IpAddress = rowValue[i++],
                    MacAddress = rowValue[i++],
                    ConnectionsCount = int.Parse(rowValue[i])
                };
                clients.Add(client);
            }

            return clients;
        }

        private static IEnumerable<DhcpClient> GetLanDhcpClientsFromContent(string pageSource, string startString, int rowArrayLength)
        {
            string contentCsv = ExtractCsvFromContent(pageSource, startString);
            contentCsv = CleanUpContent(contentCsv);

            IEnumerable<string[]> rowValues = GetArrayFromCsvContent(contentCsv, rowArrayLength);

            var clients = new List<DhcpClient>();
            foreach (string[] rowValue in rowValues)
            {
                var i = 0;
                var client = new DhcpClient
                {
                    HostName = rowValue[i++],
                    IpAddress = rowValue[i++],
                    MacAddress = rowValue[i++],
                    LeaseTime = GetTimeSpan(rowValue[i])
                };
                clients.Add(client);
            }

            return clients;
        }




        private static IEnumerable<string[]> GetRowValues(string pageSource, string startString, int rowArrayLength)
        {
            string contentCsv = ExtractCsvFromContent(pageSource, startString);
            contentCsv = CleanUpContent(contentCsv);

            IEnumerable<string[]> rowValues = GetArrayFromCsvContent(contentCsv, rowArrayLength);
            return rowValues;
        }

        private static string ExtractCsvFromContent(string content, string startString)
        {

            int index1 = content.IndexOf(startString, StringComparison.Ordinal);
            if (index1 < 0)
                return string.Empty;

            int index2 = content.IndexOf(value: "}", startIndex: index1 + 1, comparisonType: StringComparison.InvariantCulture);
            string contentCsv = content.Substring(index1 + startString.Length, index2 - (index1 + startString.Length));

            return contentCsv;
        }

        private static string CleanUpContent(string content)
        {
            return content
                .Replace("'", "")
                .Replace("days,", "day_")
                .Replace("day,", "day_")
                .Replace("days ", "day_")
                .Replace("day ", "day_")
                ;
        }

        private static IEnumerable<string[]> GetArrayFromCsvContent(string contentCsv, int rowArrayLength)
        {
            string[] allValues = contentCsv.Split(',');

            if (allValues.Length == 1 && string.IsNullOrWhiteSpace(allValues[0]))
            {
                _logger.Warn("No Data retrieved from content");

                return new List<string[]>();
            }

            // These values are just one big array, but the in DD-WRT they are displayed as 'rowArrayLength' column-rows, so will group them in sets of 'rowArrayLength'
            if (allValues.Length % rowArrayLength != 0)
            {
                throw new ArgumentException($"Expected multiple of {rowArrayLength} values, but got {allValues.Length}: {string.Join(",", allValues)}");
            }

            var rowValues = new string[allValues.Length / rowArrayLength][];
            var cellNumber = 0;

            for (var rowNumber = 0; rowNumber < allValues.Length / rowArrayLength; rowNumber++)
            {
                rowValues[rowNumber] = new string[rowArrayLength];
                for (var colNumber = 0; colNumber < rowArrayLength; colNumber++)
                {
                    rowValues[rowNumber][colNumber] = allValues[cellNumber];
                    cellNumber++;
                }
            }

            return rowValues;
        }

        private static TimeSpan GetTimeSpan(string upTimeString)
        {
            if (!upTimeString.Contains("day"))
            {
                return TimeSpan.Parse(upTimeString, new CultureInfo("en-US"));
            }

            string[] dayHours = upTimeString.Split(new[] { "day_" }, StringSplitOptions.None);
            if (dayHours.Length < 2) return new TimeSpan();

            var timeSpan = TimeSpan.Parse(dayHours[1], new CultureInfo("en-US"));

            string[] days = dayHours[0].Split(' ');
            if (days.Length < 2) return new TimeSpan();

            // timeSpan is immutable: The TimeSpan.Add method returns a new object
            timeSpan = timeSpan.Add(TimeSpan.FromDays(int.Parse(days[0])));  

            return timeSpan;
        }
    }
}
