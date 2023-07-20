using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace GcloudWebApiExtensions
{
    public static class ConfigurationExtensions
    {
        internal const string MetadataProjectIdUrl = "http://metadata.google.internal/computeMetadata/v1/project/project-id";
        internal const string MetadataEmailUrl = "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/email";

        public static string GetGCloudProjectId(this IConfiguration configuration)
        {
            var projectID = configuration["GCLOUD_PROJECTID"] ??
                Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT") ??
                Environment.GetEnvironmentVariable("GCLOUD_PROJECT");

            if (string.IsNullOrWhiteSpace(projectID))
            {
                projectID = GetGoogleCredential(configuration)?.project_id ?? GetProjectIdFromMetadataServer();

                configuration["GCLOUD_PROJECTID"] = projectID;
            }

            return projectID;
        }

        public static string GetGcloudCredentialEmail(this IConfiguration configuration)
        {
            return configuration.GetGoogleCredential()?.client_email ?? GetEmailFromMetadataServer();
        }

        public static GoogleCredential GetGoogleCredential(this IConfiguration configuration)
        {
            var filePath = configuration[ServiceCollectionExtensions.GcloudCredentialPathKeyName];
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            var json = File.ReadAllText(filePath);

            return GoogleCredential.FromJson(json);
        }

        public static GCloudSecretProvider GetGCloudSecretProvider(this IConfiguration configuration)
        {
            GCloudSecretProviderOptions options = null;
            try
            {
                options = new GCloudSecretProviderOptions { SecretManagerProjectId = configuration.GetGCloudProjectId() };
            }
            catch (Exception)
            {
            }
            return new GCloudSecretProvider(options);
        }

        public static void Add<T>(this IConfiguration configuration, T @object)
            where T : class
        {
            var json = JsonConvert.SerializeObject(@object);
            AddJson(configuration, json);
        }

        public static void AddJson(this IConfiguration configuration, string json)
        {
            var newConfig = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json))).Build();
            foreach (var item in newConfig.AsEnumerable())
            {
                configuration[item.Key] = item.Value;
            }
        }

        private static string GetProjectIdFromMetadataServer()
        {
            var projectId = string.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Metadata-Flavor", "Google");
                projectId = client.GetStringAsync(MetadataProjectIdUrl).Result;
            }

            return projectId;
        }

        private static string GetEmailFromMetadataServer()
        {
            var email = string.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Metadata-Flavor", "Google");
                email = client.GetStringAsync(MetadataEmailUrl).Result;
            }

            return email;
        }
    }
}