using GcloudWebApiExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MusicDbApi;
using MusicEventDbApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PlayFabService;
using System.Collections.Generic;
using System.Reflection;

namespace MusicApi.Serverless
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
            services.ConfigureGcloudUserSecrets();
            services.ConfigureGCloudSecretManager();
            var gcloudProjectId = this.Configuration.GetGCloudProjectId();

            services.AddScoped(_ => new MusicEventDbClient(gcloudProjectId));
            services.AddScoped(_ => new MusicDbClient(gcloudProjectId));
            services.AddScoped<PlayFabServiceOptions>();
            services.AddScoped<EconomyService>();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            services.AddControllers().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opt.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = typeof(Program).Assembly.FullName,
                    Version = typeof(Program).Assembly.GetName().Version.Major.ToString()
                });
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation(Assembly.GetExecutingAssembly().FullName);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", typeof(Program).Assembly.FullName);
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}