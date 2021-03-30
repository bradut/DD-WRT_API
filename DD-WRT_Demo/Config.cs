using System;
using System.Configuration;
using System.Globalization;

namespace DD_WRT_Demo
{
    public class Config
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string RouterUrl { get; set; }
        public TimeSpan DisplayDuration { get; private set; }
        public int WaitTimeSeconds { get; private set; }

        public void Load()
        {
            Username = ConfigurationManager.AppSettings["Username"];
            Password = ConfigurationManager.AppSettings["Password"];

            RouterUrl = ConfigurationManager.AppSettings["RouterUrl"];

            DisplayDuration = TimeSpan.Parse(ConfigurationManager.AppSettings["DisplayDuration"], CultureInfo.InvariantCulture);
            WaitTimeSeconds = int.Parse(ConfigurationManager.AppSettings["WaitTimeSeconds"]);

            if (WaitTimeSeconds < 0) WaitTimeSeconds = 0;
            if (DisplayDuration.TotalSeconds < 0)
            {
                WaitTimeSeconds = 0;
                DisplayDuration = TimeSpan.FromSeconds(1);
            }
        }
    }
}
