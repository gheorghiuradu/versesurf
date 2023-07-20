using ArtManager;
using AudioProcessing;
using BackOffice.Web.Services;
using GcloudWebApiExtensions;
using LicensingService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
using PlayFabService;
using Skclusive.Material.Component;
using Skclusive.Material.Core;
using SpotifyApiService;
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

            services.ConfigureGcloudUserSecrets();
            services.ConfigureGCloudSecretManager();

            var gcloudSecretProvider = this.Configuration.GetGCloudSecretProvider();
            var spotifyJson = gcloudSecretProvider.GetSecret(nameof(SpotifyServiceOptions));
            this.Configuration.AddJson(spotifyJson);
            this.Configuration[MusixApiClient.ConfigKeyForApiKey] =
                gcloudSecretProvider.GetSecret(MusixApiClient.ConfigKeyForApiKey);
            var gcloudProjectId = this.Configuration.GetGCloudProjectId();

            services.AddTransient<MusicServerApiClientOptions>();
            services.AddScoped<MusicServerApiClient>();

            services.AddTransient(_ => new MusicEventDbClient(gcloudProjectId));
            services.AddTransient<PlayFabServiceOptions>();
            services.AddTransient<EconomyService>();
            services.AddTransient<GoogleStorageOptions>();
            services.AddTransient<GoogleStorage>();
            services.AddTransient<SpotifyServiceOptions>();
            services.AddTransient<SpotifyService>();
            services.AddTransient<LicenseService>();
            services.AddTransient<MusixApiClient>();
            services.AddTransient<AudioProcessingService>();
            services.AddTransient<MusicBrainzApiClient>();
            services.AddTransient(_ => new MusicDbClient(this.Configuration.GetGCloudProjectId()));
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
    }
}