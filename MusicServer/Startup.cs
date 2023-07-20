using GamePlaying.Application;
using GamePlaying.Domain.GameAggregate;
using GamePlaying.Domain.PlaylistMetadataAggregate;
using GamePlaying.Domain.RoomAggregate;
using GamePlaying.Repositories;
using GcloudWebApiExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicApi.Serverless.Client;
using MusicServer.CustomAuth;
using MusicServer.CustomMiddleware;
using MusicServer.Hubs;
using MusicServer.Hubs.Services;
using MusicServer.PerformanceTesting;
using MusicServer.Words;
using MusicStorageClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteamWebApi.Client;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace MusicServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            services.ConfigureGcloudUserSecrets();
            services.ConfigureGCloudSecretManager();

            //ASP.NET Core Infrastructure
            services.AddAuthentication("GameAuthentication")
                .AddScheme<BasicAuthenticationOptions, GameAuthenticationHandler>("GameAuthentication", null);
            services.AddAuthorization();
            services.AddCors();
            services.AddControllers().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opt.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            services.AddSignalR().AddJsonProtocol(opt =>
            {
                opt.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                opt.PayloadSerializerOptions.IgnoreNullValues = true;
            });
            services.AddLogging();

            //Hub Services
            services.AddSingleton<ConnectionMonitoringService>();

            //Api Serverless
            this.AddMusicApiServerless(services);

            //Gameplaying context
            this.AddGamePlayingContext(services);

            //Other services
            services.AddTransient<GoogleStorageOptions>();
            services.AddScoped<GoogleStorage>();
            services.AddScoped<WordProvider>();
            services.AddScoped<VersioningService>();

            services.AddTransient(serviceProvider => new SteamWebApiClientOptions
            {
                ApiKey = serviceProvider.GetRequiredService<GCloudSecretProvider>().GetSecret("SteamWebApiKey"),
                UseSandbox = this.Configuration.GetValue<bool>("UseSteamSandbox")
            });
            services.AddTransient<SteamWebApiClient>();

            //Performance testing
            if (this.Configuration.GetValue<bool>("EnablePerformanceTesting"))
            {
                services.AddSingleton<ConnectionCounter>();
                services.AddHostedService<ConnectionCounterHostedService>();
            }
        }

        private void AddMusicApiServerless(IServiceCollection services)
        {
            services.AddTransient(_ => new EndpointUrl(this.Configuration["MusicApiServerlessEndpoint"]));
            services.AddTransient<MusicApiServerlessOptions>();
            services.AddTransient<MusicEventClient>();
            services.AddScoped<MusicDbApiClient>();
            services.AddScoped<EconomyClient>();
            services.AddTransient<MetadataClient>();
        }

        private void AddGamePlayingContext(IServiceCollection services)
        {
            services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
            services.AddSingleton<IGameRepository, InMemoryGameRepository>();

            services.AddSingleton<IPlaylistMetadataBuffer, PlaylistMetadataBuffer>();
            services.AddScoped<MetadataService>();

            services.AddScoped<ISecurityService, RoomAppService>();
            services.AddScoped<RoomAppService>();
            services.AddScoped<GameAppService>();
            services.AddSingleton<DevelopmentBootstrapService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation(Assembly.GetExecutingAssembly().FullName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(cfg => cfg
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .SetIsOriginAllowed((host) => true)
                                    .AllowCredentials()
                                    );

                if (bool.TryParse(Configuration["DevBootstrapEnabled"], out bool devBootstrapEnabled))
                {
                    if (devBootstrapEnabled)
                    {
                        app.UseMiddleware<DevelopmentBootstrap>();
                    }
                }
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                if (this.Configuration.GetValue<bool>("EnablePerformanceTesting"))
                {
                    endpoints.MapHub<PerformanceHub>("/ws/performance");
                }
                endpoints.MapHub<GameHub>("/ws/gamehub");
                endpoints.MapControllers();
            });
        }
    }
}