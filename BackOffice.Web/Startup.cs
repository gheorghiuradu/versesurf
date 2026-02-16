using ArtManager;
using AudioProcessing;
using BackOffice.Web.Services;
using LicensingService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicDbApi;
using MusicEventDbApi;
using MusicStorageClient;
using MusixClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Npgsql;
using Skclusive.Material.Component;
using Skclusive.Material.Core;
using SpotifyApiService;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaskService;
using TaskService.Jobs;

namespace BackOffice.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            var connectionString = this.Configuration.GetConnectionString("MusicDb");
            if(string.IsNullOrWhiteSpace(connectionString)) throw new Exception("Connection string 'MusicDb' is missing.");
            services.AddDbContext<MusicDbContext>(options =>
                options.UseNpgsql(connectionString), ServiceLifetime.Transient, ServiceLifetime.Transient);
            services.AddTransient<MusicDbClient>();

            services.AddDbContext<MusicEventDbContext>(options =>
                options.UseNpgsql(connectionString));
            services.AddScoped<MusicEventDbClient>();

            services.AddTransient<MusicServerApiClientOptions>();
            services.AddScoped<MusicServerApiClient>();
            services.AddTransient(_ =>
            {
                var options = new FileStorageOptions();
                Configuration.GetSection(nameof(FileStorageOptions)).Bind(options);
                return options;
            });
            services.AddTransient<FileStorage>();
            services.AddTransient<SpotifyServiceOptions>();
            services.AddTransient<SpotifyService>();
            services.AddTransient<LicenseService>();
            services.AddTransient<MusixApiClient>();
            services.AddTransient<AudioProcessingService>();
            services.AddTransient<MusicBrainzApiClient>();
            services.AddTransient<ImportSpotifyPlaylistJob>();
            services.AddTransient<CleanupUnusedStorageJob>();
            services.AddTransient<ReplaceGSPathJob>();
            services.AddTransient<GeneratePlayedSongsReportJob>();
            services.AddTransient<ExportPlaylistsJob>();
            services.AddTransient<ImportPlaylistsJob>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<JobService>();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.TryAddMaterialServices(new MaterialConfigBuilder().Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation(Assembly.GetExecutingAssembly().FullName);

            try
            {
                var connectionString = Configuration.GetConnectionString("MusicDb");
                EnsureDatabaseExists(connectionString, logger);

                using var connection = new NpgsqlConnection(connectionString);
                
                var evolve = new EvolveDb.Evolve(connection, msg => logger.LogInformation(msg))
                {
                    Locations = new[] { "db/migrations" },
                    IsEraseDisabled = true
                };
                evolve.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database migration failed.");
                throw;
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            app.UseStaticFiles();
        }

        private static void EnsureDatabaseExists(string connectionString, ILogger logger)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var targetDatabase = builder.Database;
            if (string.IsNullOrWhiteSpace(targetDatabase))
            {
                throw new InvalidOperationException("Database name is missing in the connection string.");
            }

            var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = "postgres"
            };

            using var adminConnection = new NpgsqlConnection(adminBuilder.ConnectionString);
            adminConnection.Open();

            using var existsCommand = adminConnection.CreateCommand();
            existsCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbName;";
            existsCommand.Parameters.AddWithValue("dbName", targetDatabase);
            var exists = existsCommand.ExecuteScalar() != null;

            if (exists)
            {
                logger.LogInformation("Database '{Database}' already exists.", targetDatabase);
                return;
            }
            var commandBuilder = new NpgsqlCommandBuilder();
            using var createCommand = adminConnection.CreateCommand();
            createCommand.CommandText = $"CREATE DATABASE {commandBuilder.QuoteIdentifier(targetDatabase)};";
            createCommand.ExecuteNonQuery();
            logger.LogInformation("Database '{Database}' created.", targetDatabase);
        }
    }
}