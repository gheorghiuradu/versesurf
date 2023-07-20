using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Music.WebClient.CustomMiddleware
{
    /// <summary>
    /// In development mode, when a request arrives at "/" and has no query string param "code", 
    /// a request to "server/devboot" is made in order to get the latest created game/room code present in memory.
    /// </summary>
    public class DevelopmentBootstrap
    {
        private const string serverUrl = "http://localhost:5000/devboot";
        private RequestDelegate next;
        private IWebHostEnvironment env;

        public DevelopmentBootstrap(RequestDelegate next, IWebHostEnvironment env)
        {
            this.next = next; 
            this.env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (this.env.IsDevelopment()
                    && context.Request.Path.Equals("/")
                    && !context.Request.Query.ContainsKey("code"))
                {
                    var httpClient = new HttpClient();
                    var code = await httpClient.GetStringAsync(serverUrl);
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        context.Response.Redirect($"http://localhost:81?code={code}");
                        return;
                    }
                }
            }
            catch (Exception ex) 
            {
            }

            await this.next.Invoke(context);
        }
    }
}
