using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MusicServer.Models;

namespace MusicServer.Controllers
{
    public class HomeController(IWebHostEnvironment env, IConfiguration config) : Controller
    {
        public IActionResult Index()
        {
            this.ViewData["HubUrl"] = "/ws/gamehub";
            this.ViewData["ApiUrl"] = "/api";
            this.ViewData["GenericAlbumUrl"] = "/img/generic-album.png";
            this.ViewData["CharacterPath"] = config["CharacterPath"];

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
            if (!env.IsDevelopment())
            {
                return this.RedirectToAction(nameof(this.Error));
            }

            this.ViewData["EnvironmentName"] = env.EnvironmentName;
            this.ViewData["IsProduction"] = env.IsProduction();
            this.ViewData["IsDevelopment"] = env.IsDevelopment();

            return View();
        }
    }
}