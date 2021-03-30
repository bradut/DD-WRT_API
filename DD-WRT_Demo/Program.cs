using DD_WRT_API.Models;
using DD_WRT_API.Services;
using DD_WRT_Demo.DisplayModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using log4net.Config;

namespace DD_WRT_Demo
{
    /// <summary>
    /// Project to demonstrate the capabilities of the DD-WRT API: 
    /// Display LAN info or/and reboot.
    /// </summary>
    internal static class Program
    {
        private struct ArgsIndex
        {
            public const int CommandNameArg = 0;
            public const int UserNameArg = 1;
            public const int PasswordArg = 2;
            public const int RouterUrlArg = 3;
        }

        private struct Commands
        {
            public const string WiFi = "wifi";
            public const string Lan = "lan";
            public const string Active = "active";
            public const string All = "all";
            public const string Reboot = "reboot";

            public static IEnumerable<string> GetCommands()
            {
                var commands = typeof(Commands).GetFields()
                    .Select(fieldInfo => fieldInfo.GetRawConstantValue().ToString());

                return commands;
            }

            public static bool IsValid(string command)
            {
                return GetCommands()
                    .Any(cmd => string.Equals(cmd, command, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        private static readonly Config _configuration = new Config();
        private static string _command = string.Empty;
        private static bool _isValidCommand;
        private static List<LanNodeDto> _lanNodesToDisplay;
        private static DdWrtServices _ddWrtService;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program).FullName);

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            try
            {
                _configuration.Load();
                DisplayConfiguration();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the configuration:{Environment.NewLine}{ex.Message}");
                _logger.Error(ex);
                PressAnyKeyToExit();

                return;
            }


            if (args.Length > 1 && args.Length < 4)
            {
                DisplayHelpUsageInfo();
            }
            else
            {
                ReadConsoleArguments(args);
            }

            if (_isValidCommand)
            {
                ExecuteCommand();
            }

            PressAnyKeyToExit();
        }
        
        private static void ReadConsoleArguments(IList<string> args)
        {
            switch (args.Count)
            {
                case 0:
                    _command = Commands.All;

                    _isValidCommand = true;
                    break;

                case 1:
                    _command = args[ArgsIndex.CommandNameArg].ToLower();

                    if (!Commands.IsValid(_command))
                    {
                        DisplayHelpInvalidCommand(_command);
                        DisplayHelpUsageInfo();

                        _isValidCommand = false;
                        break;
                    }

                    _isValidCommand = true;
                    break;

                case 4:
                    _command = args[ArgsIndex.CommandNameArg].ToLower();
                    _configuration.Username = args[ArgsIndex.UserNameArg];
                    _configuration.Password = args[ArgsIndex.PasswordArg];
                    _configuration.RouterUrl = args[ArgsIndex.RouterUrlArg];

                    _isValidCommand = true;
                    break;

                default:
                    throw new ArgumentException($"Unexpected # of arguments: {args.Count}, Expected: 0,1 or 4");
            }
        }

        private static void ExecuteCommand()
        {
            try
            {
                _ddWrtService = ServiceFactory.Create(_configuration.Username, _configuration.Password,
                                                      _configuration.RouterUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating {nameof(ServiceFactory)}:{Environment.NewLine}{ex.Message}");
                _logger.Error(ex);

                return;
            }

            if (_command.ToLower() == Commands.Reboot)
            {
                ExecuteRebootRouter();

                return;
            }

            ExecuteReadCommandsAndDisplayNodes();
        }

        private static void ExecuteRebootRouter()
        {
            (bool success, string message) = _ddWrtService.Reboot();
            Console.WriteLine(success
                ? "Router is rebooting"
                : $"{Environment.NewLine}Error rebooting the router:{Environment.NewLine}{message}");
        }

        private static (IEnumerable<DhcpClient> _dhcpClients, IEnumerable<ActiveClient> _activeClients, IEnumerable<WirelessNode> _wirelessNodes)
            ReadRouterInfo()

        {
            var _dhcpClients = _ddWrtService.GetLanDhcpClients();
            var _activeClients = _ddWrtService.GetLanActiveClients();
            var _wirelessNodes = _ddWrtService.GetWirelessStatus();

            return (_dhcpClients, _activeClients, _wirelessNodes);
        }

        private static void ExecuteReadCommandsToUpdateLanNodesForDisplay()
        {
            const int maxRetries = 5;
            var retries = maxRetries;
            IEnumerable<DhcpClient> dhcpClients = null;
            IEnumerable<ActiveClient> activeClients = null;
            IEnumerable<WirelessNode> wirelessNodes = null;

            while (retries > 0)
            {
                try
                {
                    (dhcpClients, activeClients, wirelessNodes) = ReadRouterInfo();
                    break;
                }
                catch (System.Net.WebException ex)
                {
                    if (ex.Message.ToLower().Contains("unauthorized"))
                    {
                        Console.WriteLine(
                            $"{Environment.NewLine}Could not connect to router, check username & password:{Environment.NewLine}" +
                            $"{ex.Message} - {ex.InnerException?.Message}.{Environment.NewLine}");

                        retries = 0;
                    }
                    else
                    {
                        Console.WriteLine(
                            $"{Environment.NewLine}Could not read data from router, maybe router is rebooting or URL is incorrect:{Environment.NewLine}" +
                            $"{ex.Message} - {ex.InnerException?.Message}.{Environment.NewLine}" +
                            $"Attempt {maxRetries - retries + 1} of {maxRetries}");
                        retries--;

                        Thread.Sleep(5000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        $"{Environment.NewLine}Could not connect to router:{Environment.NewLine}" +
                        $"{e.Message} - {e.InnerException?.Message}.{Environment.NewLine}");

                    retries = 0;

                    _logger.Error(e);
                }
            }

            _lanNodesToDisplay = new List<LanNodeDto>();

            if (dhcpClients == null || activeClients == null || wirelessNodes == null)
            {
                Console.WriteLine("Could not read data from router.");

                return;
            }

            foreach (var dhcpClient in dhcpClients)
            {
                var lanNodeToDisplay = new LanNodeDto();
                lanNodeToDisplay.Update(dhcpClient);
                _lanNodesToDisplay.Add(lanNodeToDisplay);
            }

            foreach (var activeClient in activeClients)
            {
                var lanNodeToDisplay =
                    _lanNodesToDisplay.FirstOrDefault(node => node.MacAddress == activeClient.MacAddress);
                lanNodeToDisplay?.Update(activeClient);
            }

            foreach (var wirelessNode in wirelessNodes)
            {
                var lanNodeToDisplay =
                    _lanNodesToDisplay.FirstOrDefault(node => node.MacAddress == wirelessNode.MacAddress);
                lanNodeToDisplay?.Update(wirelessNode);
            }

            _lanNodesToDisplay.Sort();
        }

        private static void ExecuteReadCommandsAndDisplayNodes()
        {
            var startTime = DateTime.Now;

            while (startTime.Add(_configuration.DisplayDuration) > DateTime.Now)
            {
                ExecuteReadCommandsToUpdateLanNodesForDisplay();

                if (!_lanNodesToDisplay.Any())
                    break;

                DisplayHeader();

                foreach (var nodeDto in _lanNodesToDisplay.Where(FilterNodeByCommandType))
                {
                    switch (_command)
                    {
                        case Commands.Active:
                            nodeDto.DisplayActiveNodesInfo();
                            break;

                        case Commands.Lan:
                            nodeDto.DisplayDhcpNodesInfo();
                            break;

                        case Commands.WiFi:
                            nodeDto.DisplayWiFiNodesInfo();
                            break;

                        case Commands.All:
                            nodeDto.DisplayGeneralInfo();
                            break;
                    }
                }

                DisplayFooter();

                Sleep();
            }

        }

        private static bool FilterNodeByCommandType(LanNodeDto node)
        {
            switch (_command)
            {
                case Commands.Active:
                    return node.IsActive;

                case Commands.WiFi:
                    return node.IsWifi;


                case Commands.All:
                case Commands.Lan:
                    return true;

                default:
                    return true;
            }
        }



        private static void Sleep()
        {
            if (_configuration.WaitTimeSeconds <= 0) return;

            var orgFontColor = Console.ForegroundColor;
            const ConsoleColor fontColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = fontColor;

            var sleepMessage = ("Sleeping " + _configuration.WaitTimeSeconds + @"""");
            Console.Write(sleepMessage, ConsoleColor.DarkGreen, false);
            for (var i = 0; i < _configuration.WaitTimeSeconds; i++)
            {
                Console.Write(".");
                Thread.Sleep(1000);
            }
            Console.WriteLine($"|{Environment.NewLine}");

            Console.ForegroundColor = orgFontColor;
        }
        
        private static void PressAnyKeyToExit()
        {
            Console.WriteLine("\nPress any key ....");
            Console.ReadKey();
        }
      
        private static void DisplayHeader()
        {
            var headerText = string.Empty;
            var headerLine = string.Empty;

            switch (_command)
            {
                case Commands.Lan:
                    headerText = " Host name               IP              MAC              LeaseTime   WiFi";
                    headerLine = "--------------------------------------------------------------------------";
                    break;

                case Commands.WiFi:
                    headerText = " Host name               IP              MAC                 UpTime    Tx   Rx  Signal Noise SNR WiFi";
                    headerLine = "-----------------------------------------------------------------------Mb---Mb---dB----dB----dB--Qlty-";
                    break;

                case Commands.Active:
                case Commands.All:
                    headerText = " Host name               IP              MAC            #Conn, WiFi";
                    headerLine = "--------------------------------------------------------------------";
                    break;
            }


            var orgFontColor = Console.ForegroundColor;
            const ConsoleColor fontColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = fontColor;
            Console.WriteLine(headerText);
            Console.WriteLine(headerLine);
            Console.ForegroundColor = orgFontColor;
        }
    
        private static void DisplayFooter()
        {
            var footer = string.Empty;

            switch (_command)
            {
                case Commands.Active:

                    footer = $"---[Active]----------------------------------- {DateTime.Now:yyy-MM-dd HH:mm:ss}--\n\n";
                    break;

                case Commands.Lan:
                    footer = $"---[LAN]-------------------------------------------- {DateTime.Now:yyy-MM-dd HH:mm:ss}--\n\n";
                    break;

                case Commands.WiFi:
                    footer = $"---[WiFi]----------------------------------------------------------------------- {DateTime.Now:yyy-MM-dd HH:mm:ss}--\n\n";
                    break;

                case Commands.All:
                    footer =
                        $"--[ All nodes]-------------------------------- {DateTime.Now:yyy-MM-dd HH:mm:ss}--\n\n";
                    break;
            }

            var orgFontColor = Console.ForegroundColor;
            const ConsoleColor fontColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = fontColor;
            Console.WriteLine(footer);
            Console.ForegroundColor = orgFontColor;
        }


        private static void DisplayHelpUsageInfo()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("Call with arguments:       <command>       <username>   <password>     <router url>");
            Console.WriteLine($"Example:             {Commands.GetCommands().FirstOrDefault()}    admin       admin       http://192.168.0.1");
            Console.WriteLine($"Example:             {Commands.GetCommands().FirstOrDefault()}   - [if username, password, url are stored in app.config]");
            Console.WriteLine();
            Console.WriteLine($"Valid commands: {string.Join(", ", Commands.GetCommands())}");
        }

        private static void DisplayHelpInvalidCommand(string cmd)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("****************************************************************");
            Console.WriteLine($"Invalid command {cmd}. Expected one of these: {string.Join(", ", Commands.GetCommands())}");
            Console.WriteLine("****************************************************************\n");
            Console.ForegroundColor = color;
        }


        private static void DisplayConfiguration()
        {
            Console.WriteLine();
            var consoleForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
 
            Console.WriteLine("----[ Settings ---------------------------------------------");

            Console.WriteLine("Router URL".PadRight(22) +_configuration.RouterUrl);
            Console.WriteLine("DisplayDuration".PadRight(22) + _configuration.DisplayDuration);
            Console.WriteLine("WaitTimeSeconds".PadRight(22) + _configuration.WaitTimeSeconds);

            Console.WriteLine("-------------------------------------------------------------");

            Console.WriteLine($"{Environment.NewLine}{Environment.NewLine}");

            Console.ForegroundColor = consoleForegroundColor;
        }
    }
}
