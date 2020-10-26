using System.Collections.Generic;
using TFM.Data.Models.Ranking;

namespace TFM.Data.Models.Configuration
{
    public class AppSettings
    {
        public string TFMConnectionString { get; set; }
        public int NumGamesToRetrieve { get; set; }

        public Dictionary<Platform, PlatformConfiguration> Platforms { get; set; }
        public MailConfiguration Mail { get; set; }
        public MetacriticApiConfiguration MetacriticApi { get; set; }
        public JobsConfiguration Jobs { get; set; }

        public class PlatformConfiguration
        {
            public string ScrapingUrl { get; set; }
            public string ApiUrl { get; set; }
        }

        public class MailConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string DisplayName { get; set; }
            public string To { get; set; }
        }

        public class MetacriticApiConfiguration
        {
            public string Host { get; set; }
            public string Key { get; set; }
        }

        public class PingJobServiceConfiguration
        {
            public int FrequencyInMinutes { get; set; }
            public string PingUrl { get; set; }
        }

        public class JobsConfiguration
        {
            public PingJobServiceConfiguration PingJobService { get; set; }
        }
    }
}
