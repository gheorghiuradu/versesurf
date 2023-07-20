using GcloudWebApiExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace ArtManager.Tests
{
    public static class MockServiceConfiguration
    {
        public static void Build()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", " ");
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IConfiguration>(_ => configuration);
            serviceCollection.AddLogging();
            serviceCollection.ConfigureGcloudUserSecrets();
        }
    }
}