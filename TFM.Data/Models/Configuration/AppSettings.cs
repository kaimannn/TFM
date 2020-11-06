using Newtonsoft.Json;
using System.Collections.Generic;
using TFM.Data.Models.Enums;

namespace TFM.Data.Models.Configuration
{
    public class AppSettings
    {
        public DatabaseConfiguration Database { get; set; }
        public RankingConfigruation Ranking { get; set; }
        public GoogleCredentialsConfiguration GoogleCredentials { get; set; }
        public MailConfiguration Mail { get; set; }
        public MetacriticConfiguration Metacritic { get; set; }
        public JobsConfiguration Jobs { get; set; }

        public class DatabaseConfiguration
        {
            public string DevConnectionString { get; set; }
            public string ProdConnectionString { get; set; }
        }
        public class RankingConfigruation
        {
            public int DefaultNumGamesToShow { get; set; }
        }
        public class PlatformConfiguration
        {
            public string ScrapingUrl { get; set; }
        }

        public class MetacriticConfiguration
        {
            public string BaseUrl { get; set; }
            public Dictionary<Platform, PlatformConfiguration> Platforms { get; set; }
            public int NumGamesToRetrieve { get; set; }
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

        public class PingJobServiceConfiguration
        {
            public int FrequencyInMinutes { get; set; }
            public string PingUrl { get; set; }
        }

        public class LoadRankingJobServiceConfiguration
        {
            public int ExecutionHour { get; set; }
            public int ExecutionMinute { get; set; }
        }

        public class JobsConfiguration
        {
            public PingJobServiceConfiguration PingJobService { get; set; }
            public LoadRankingJobServiceConfiguration LoadRankingJobService { get; set; }
        }

        public class GoogleCredentialsConfiguration
        {
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("project_id")]
            public string ProjectId { get; set; }
            [JsonProperty("private_key_id")]
            public string PrivateKeyId { get; set; }
            [JsonProperty("private_key")]
            public string PrivateKey { get; set; }
            [JsonProperty("client_email")]
            public string ClientEmail { get; set; }
            [JsonProperty("client_id")]
            public string ClientId { get; set; }
            [JsonProperty("auth_uri")]
            public string AuthUri { get; set; }
            [JsonProperty("token_uri")]
            public string TokenUri { get; set; }
            [JsonProperty("auth_provider_x509_cert_url")]
            public string AuthProviderX509CertUrl { get; set; }
            [JsonProperty("client_x509_cert_url")]
            public string ClientX509CertUrl { get; set; }
        }
    }
}
