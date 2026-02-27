using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using GamePlaying.Application;
using GamePlaying.Domain.GameAggregate;
using GamePlaying.Domain.PlaylistMetadataAggregate;
using GamePlaying.Domain.RoomAggregate;
using GamePlaying.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicDbApi;
using MusicServer.CustomAuth;
using MusicServer.CustomMiddleware;
using MusicServer.Hubs;
using MusicServer.Hubs.Services;
using MusicServer.PerformanceTesting;
using MusicServer.Words;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteamWebApi.Client;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace MusicServer;

public class Startup(IConfiguration configuration)
{
    private IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

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
            opt.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        services.AddRazorPages();
        services.AddLogging();

        //Hub Services
        services.AddSingleton<ConnectionMonitoringService>();

        //Gameplaying context
        AddGamePlayingContext(services);

        //Other services
        services.AddScoped<VersioningService>();
        services.AddSingleton<MusicDbClient>();
        services.AddScoped<WordProvider>();

        services.AddTransient(serviceProvider => new SteamWebApiClientOptions
        {
            ApiKey = Configuration["SteamWebApiKey"] ?? "",
            UseSandbox = Configuration.GetValue<bool>("UseSteamSandbox")
        });
        services.AddTransient<SteamWebApiClient>();

        //Performance testing
        if (Configuration.GetValue<bool>("EnablePerformanceTesting"))
        {
            services.AddSingleton<ConnectionCounter>();
            services.AddHostedService<ConnectionCounterHostedService>();
        }
    }

    private void AddGamePlayingContext(IServiceCollection services)
    {
        services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
        services.AddSingleton<IGameRepository, InMemoryGameRepository>();

        services.AddSingleton<MetadataClient>();
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
                .SetIsOriginAllowed(host => true)
                .AllowCredentials()
            );

            if (bool.TryParse(Configuration["DevBootstrapEnabled"], out var devBootstrapEnabled))
                if (devBootstrapEnabled)
                    app.UseMiddleware<DevelopmentBootstrap>();
        }

        //app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            if (Configuration.GetValue<bool>("EnablePerformanceTesting")) endpoints.MapHub<PerformanceHub>("/ws/performance");
            endpoints.MapHub<GameHub>("/ws/gamehub");
            endpoints.MapControllers();
            endpoints.MapDefaultControllerRoute();
        });
    }
}