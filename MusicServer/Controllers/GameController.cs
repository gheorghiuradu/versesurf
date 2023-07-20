using GamePlaying.Application;
using GcloudWebApiExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MusicApi.Serverless.Client;
using MusicServer.CustomAuth;
using MusicServer.Hubs;
using MusicServer.Models;
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
        private readonly GCloudSecretProvider gCloudSecretProvider;
        private readonly MusicEventClient musicEventClient;

        public GameController(
            GoogleStorage googleStorage,
            RoomAppService roomAppService,
            IHubContext<GameHub> gameHub,
            GCloudSecretProvider gCloudSecretProvider,
            MusicEventClient musicEventClient)
        {
            this.googleStorage = googleStorage;
            this.roomAppService = roomAppService;
            this.gameHub = gameHub;
            this.gCloudSecretProvider = gCloudSecretProvider;
            this.musicEventClient = musicEventClient;
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

            var actualKey = await this.gCloudSecretProvider.GetSecretAsync(nameof(NotificationRequest.BoKey));
            if (!string.Equals(notificationRequest.BoKey, actualKey))
            {
                await this.musicEventClient.PostEventAsync(EventType.NotificationRequestUnauthorized, new
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

            await this.musicEventClient.PostEventAsync(EventType.NotificationRequestAuthorized, new
            {
                Request = notificationRequest,
                HostConnectionIds = connectionIdResult.ConnectionIds
            });

            return this.Ok(connectionIdResult.ConnectionIds.Count);
        }
    }
}