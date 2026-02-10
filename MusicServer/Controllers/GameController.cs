using GamePlaying.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using MusicServer.CustomAuth;
using MusicServer.Hubs;
using MusicServer.Models;
using MusicServer.Services;
using MusicStorageClient;
using SharedDomain;
using SharedDomain.InfraEvents;
using SharedDomain.Messages.Commands;
using System;
using System.Threading.Tasks;

namespace MusicServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly RoomAppService roomAppService;
        private readonly IHubContext<GameHub> gameHub;
        private readonly GoogleStorage googleStorage;
        private readonly IConfiguration configuration;
        private readonly MusicEventService musicEventService;

        public GameController(
            GoogleStorage googleStorage,
            RoomAppService roomAppService,
            IHubContext<GameHub> gameHub,
            IConfiguration configuration,
            MusicEventService musicEventService)
        {
            this.googleStorage = googleStorage;
            this.roomAppService = roomAppService;
            this.gameHub = gameHub;
            this.configuration = configuration;
            this.musicEventService = musicEventService;
        }

        [Authorize(Roles = Roles.Host)]
        [HttpGet]
        public async Task<IActionResult> GetSignedSongPreviewUrl([FromQuery] string previewUrl)
        {
            var signedUrl = await this.googleStorage.GetSignedUrlAsync(previewUrl);

            if (string.IsNullOrEmpty(signedUrl)) return this.NotFound();

            return this.Ok(signedUrl);
        }

        [Authorize(Roles = Roles.Host)]
        [HttpGet]
        public async Task<IActionResult> GetFileMd5([FromQuery] string fileUrl)
        {
            try
            {
                return this.Ok(await this.googleStorage.GetFileMd5Async(fileUrl));
            }
            catch (Exception)
            {
                return this.NotFound();
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest notificationRequest)
        {
            if (notificationRequest is null)
            {
                return this.BadRequest();
            }

            var actualKey = this.configuration["BoKey"] ?? "";
            if (!string.Equals(notificationRequest.BoKey, actualKey))
            {
                await this.musicEventService.PostEventAsync(EventType.NotificationRequestUnauthorized, new
                {
                    Ip = this.HttpContext.Connection.RemoteIpAddress,
                    HttpRequest = this.HttpContext.Request
                });
                return this.Unauthorized();
            }

            var connectionIdResult = this.roomAppService.GetAllConnectedHostConnectionIds();
            if (connectionIdResult.ConnectionIds.Count > 0)
            {
                await this.gameHub.Clients.Clients(connectionIdResult.ConnectionIds).SendAsync(
                    HostMethods.Message,
                    new NotificationMessage
                    {
                        Title = notificationRequest.Title,
                        Message = notificationRequest.Message
                    });
            }

            await this.musicEventService.PostEventAsync(EventType.NotificationRequestAuthorized, new
            {
                Request = notificationRequest,
                HostConnectionIds = connectionIdResult.ConnectionIds
            });

            return this.Ok(connectionIdResult.ConnectionIds.Count);
        }
    }
}