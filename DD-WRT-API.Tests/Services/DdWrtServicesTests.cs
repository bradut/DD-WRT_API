using DD_WRT_API.Models;
using DD_WRT_API.Services;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;


namespace DD_WRT_API.Tests.Services
{
    [TestFixture]
    public class DdWrtServicesTests
    {
        private readonly Mock<IRouterConnectionService> _reader = new Mock<IRouterConnectionService>();


        [Test]
        [TestCase(WirelessResponseString)]
        public void GetWirelessStatus_Happy_Path(string responseString)
        {
            // Arrange
            _reader.Setup(r => r.GetPageContent(It.IsAny<string>())).Returns(responseString);
            var dddWrtServices = new DdWrtServices(_reader.Object);

            // Act
            var result = dddWrtServices.GetWirelessStatus();
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<WirelessNode>>(result);
            Assert.AreEqual(4, result.Count());
        }


        [Test]
        [TestCase(LanResponseString)]
        public void GetLanActiveClients_Happy_Path(string responseString)
        {
            // Arrange
            _reader.Setup(r => r.GetPageContent(It.IsAny<string>())).Returns(responseString);
            var dddWrtServices = new DdWrtServices(_reader.Object);

            // Act
            var result = dddWrtServices.GetLanActiveClients();
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<ActiveClient>>(result);
            Assert.AreEqual(4, result.Count());
        }


        [Test]
        [TestCase(LanResponseString)]
        public void GetLanDhcpClients_Happy_Path(string responseString)
        {
            // Arrange
            _reader.Setup(r => r.GetPageContent(It.IsAny<string>())).Returns(responseString);
            var dddWrtServices = new DdWrtServices(_reader.Object);

            // Act
            var result = dddWrtServices.GetLanDhcpClients();
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<IEnumerable<DhcpClient>>(result);
            Assert.AreEqual(8, result.Count());
        }





        private const string WirelessResponseString = @"{wl_mac::00:AF:F7:DF:00:00}
{wl_ssid::Bradut}
{wl_channel::5 (2432 MHz HT20)}
{wl_radio::Radio is On}
{wl_xmit::21 dBm}
{wl_rate::144 Mb/s}
{wl_ack::15&#181;s (2250m)}
{active_wireless::
'00:E9:FE:75:9E:00','ath0','0:19:50','34M','6M','HT20','-35','-95','60','1000',
'00:91:82:04:EA:00','ath0','0:58:09','14M','58M','HT20','-70','-95','25','600',
'00:E6:F7:68:3C:00','ath0','2:10:32','29M','115M','HT20','-56','-95','39','880',
'00:91:82:04:C9:00','ath0','5:24:41','34M','72M','HT20','-44','-95','51','1000'}
{active_wds::}
{packet_info::SWRXgoodPacket=2275379;SWRXerrorPacket=0;SWTXgoodPacket=1797067;SWTXerrorPacket=0;}
{uptime:: 21:42:43 up  5:25,  load average: 0.07, 0.15, 0.20}
{ipinfo::&nbsp;IP: 192.168.0.1}";


        private const string LanResponseString = @"
     {lan_mac::1C:AF:F7:DF:1F:FD}{lan_ip::192.168.0.1}{lan_ip_prefix::192.168.0.}{lan_netmask::255.255.255.0}{lan_gateway::0.0.0.0}{lan_dns::0.0.0.0}{lan_proto::dhcp}{dhcp_daemon::DNSMasq}{dhcp_start::100}{dhcp_num::50}{dhcp_lease_time::1440}
        
        // DHCP CLIENTS:
        // Hostname,	IP Address,	    MAC Address,	Client Lease Time, ???
       

        {dhcp_leases:: 
        'SmartTV',                  '192.168.0.145','10:00:C1:C0:00:00','1 day 00:00:00','145',
        'android-155df946551899fg', '192.168.0.136','5C:00:35:76:00:00','1 day 00:00:00','136',
        'SPA112',                   '192.168.0.126','54:00:1A:11:00:00','1 day 00:00:00','126',
        'WeMo_Smart_Plug_1',        '192.168.0.101','14:00:82:04:00:00','6 days 22:39:00','101',
        'HP_PRN',                   '192.168.0.135','10:00:E5:CB:00:00','1 day 00:00:00','135',
        'WeMo_Smart_Plug_2',        '192.168.0.102','14:00:82:04:00:00','6 days 22:39:00','102',
        'PC_LAN',                   '192.168.0.10', '98:00:96:AC:00:00','1 day 00:00:00','133',
        'Debara',                   '192.168.0.30', '00:D0:09:D2:00:00','6 days 22:39:00','30'}
        
        {pptp_leases::}{pppoe_leases::}



        // ACTIVE CLIENTS
         // Hostname,	IP Address,	MAC Address,	Conn. Count,	(???Ratio [4096])

        {arp_table:: 
        'WeMo_Smart_Plug_1', '192.168.0.101',    '14:00:82:04:00:00',    '3',
        'WeMo_Smart_Plug_2', '192.168.0.102',    '14:00:82:04:00:00',    '2',
        'SPA112',            '192.168.0.126',    '54:00:1A:11:00:00',    '1',
        'PC_LAN',            '192.168.0.10',     '98:00:96:AC:00:00',    '210'}
        
        {uptime:: 22:26:55 up 12:13, load average: 0.00, 0.01, 0.04}{ipinfo:: IP: 192.168.0.1}
        ";

    }
}
