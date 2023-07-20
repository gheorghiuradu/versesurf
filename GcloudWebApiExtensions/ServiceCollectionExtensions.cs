using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;

namespace GcloudWebApiExtensions
{
    public static class ServiceCollectionExtensions
    {
        internal const string GcloudCredentialPathKeyName = "GOOGLE_APPLICATION_CREDENTIALS";
        internal const string DefaultGcloudConfigurationKeyName = "GCLOUD_KEY";

        public static void ConfigureGcloudUserSecrets(this IServiceCollection services, string gcloudConfigurationKeyName)
        {
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<GcloudDotnetExtensions>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var jsonKeySection = configuration.GetSection(gcloudConfigurationKeyName);
            var gcloudCredentialPathSection = configuration.GetSection(GcloudCredentialPathKeyName);

            if (!jsonKeySection.Exists())
            {
                logger.LogInformation($"Gcloud key name {gcloudConfigurationKeyName} not found, exiting configuration");
                return;
            }

            if (!gcloudCredentialPathSection.Exists())
            {
                logger.LogInformation($"Gcloud key name {GcloudCredentialPathKeyName} not found, exiting configuration");
                return;
            }

            var folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().FullName.Contains("testhost") ?
                Assembly.GetExecutingAssembly().Location : Assembly.GetEntryAssembly().Location);
            var filePath = Path.GetFileName(gcloudCredentialPathSection.Value);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = "gcloud_credential.json";
            }
            var fullPath = Path.Combine(folderPath, filePath);

            GoogleCredential credential;
            using (var file = File.CreateText(fullPath))
            {
                credential = jsonKeySection.Get<GoogleCredential>();
                file.Write(credential.ToString());
            }

            configuration[GcloudCredentialPathKeyName] = fullPath;
            configuration[DefaultGcloudConfigurationKeyName] = fullPath;
            Environment.SetEnvironmentVariable(gcloudCredentialPathSection.Key, fullPath);
        }

        public static void ConfigureGcloudUserSecrets(this IServiceCollection services)
        {
            ConfigureGcloudUserSecrets(services, DefaultGcloudConfigurationKeyName);
        }

        public static void ConfigureGCloudSecretManager(this IServiceCollection services)
        {
            services.AddTransient(s => new GCloudSecretProviderOptions
            {
                SecretManagerProjectId = s.GetRequiredService<IConfiguration>().GetGCloudProjectId()
            });
            services.AddTransient<GCloudSecretProvider>();
        }
    }

    public class GcloudDotnetExtensions { }
}