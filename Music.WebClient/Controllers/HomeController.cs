using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Music.WebClient.Models;
using System.Diagnostics;

namespace Music.WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;

        public HomeController(IWebHostEnvironment env, IConfiguration config)
        {
            this.env = env;
            this.config = config;
        }

        public IActionResult Index()
        {
            var serverUrl = string.Empty;
            if (this.env.IsDevelopment())
            {
                serverUrl = "http://localhost:5000";
            }

            this.ViewData["HubUrl"] = $"{serverUrl}/ws/gamehub";
            this.ViewData["ApiUrl"] = $"{serverUrl}/api";
            this.ViewData["GenericAlbumUrl"] = $"{serverUrl}/img/generic-album.png";
            this.ViewData["CharacterPath"] = this.config["CharacterPath"];

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Debug()
        {
            if (!this.env.IsDevelopment())
            {
                return this.RedirectToAction(nameof(this.Error));
            }

            this.ViewData["EnvironmentName"] = this.env.EnvironmentName;
            this.ViewData["IsProduction"] = this.env.IsProduction();
            this.ViewData["IsDevelopment"] = this.env.IsDevelopment();

            return View();
        }
    }
}