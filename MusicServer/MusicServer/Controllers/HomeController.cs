using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MusicServer.Models;

namespace MusicServer.Controllers;

public class HomeController(IWebHostEnvironment env, IConfiguration config) : Controller
{
    public IActionResult Index()
    {
        ViewData["HubUrl"] = "/ws/gamehub";
        ViewData["ApiUrl"] = "/api";
        ViewData["GenericAlbumUrl"] = "/img/generic-album.png";
        ViewData["CharacterPath"] = "/img/characters/";

        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    public IActionResult Debug()
    {
        if (!env.IsDevelopment()) return RedirectToAction(nameof(Error));

        ViewData["EnvironmentName"] = env.EnvironmentName;
        ViewData["IsProduction"] = env.IsProduction();
        ViewData["IsDevelopment"] = env.IsDevelopment();

        return View();
    }
}