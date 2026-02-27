using GamePlaying.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace MusicServer.CustomMiddleware
{
    /// <summary>
    /// In development mode, when a request arrives at /devboot, it is responded with the latest created game/room code present in memory.
    /// </summary>
    public class DevelopmentBootstrap
    {
        private RequestDelegate next;
        private DevelopmentBootstrapService bootstrapService;
        private IWebHostEnvironment env;

        public DevelopmentBootstrap(RequestDelegate next, 
            DevelopmentBootstrapService bootstrapService, 
            IWebHostEnvironment env)
        {
            this.next = next;
            this.bootstrapService = bootstrapService;
            this.env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!env.IsDevelopment() || !context.Request.Path.StartsWithSegments("/devboot"))
            {
                await this.next.Invoke(context);
                return;
            }

            var latestCode = this.bootstrapService.GetLatestCode();
            if (string.IsNullOrWhiteSpace(latestCode))
            {
                await this.next.Invoke(context);
                return;
            }

            await context.Response.WriteAsync(latestCode);
        }
    }
}
