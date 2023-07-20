using GamePlaying.Application;
using GamePlaying.Application.Commands;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MusicApi.Serverless.Client;
using SharedDomain;
using SharedDomain.InfraEvents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicServer.Hubs.Services
{
    public class ConnectionMonitoringService
    {
        private readonly HashSet<string> pendingKickConnectionIds = new HashSet<string>();
        private readonly object pendingKickLock = new object();
        private readonly ILogger logger;
        private readonly IHubContext<GameHub> hubContext;
        private readonly MusicEventClient musicEventClient;

        public ConnectionMonitoringService(
            ILogger<ConnectionMonitoringService> logger,
            IHubContext<GameHub> hubContext,
            MusicEventClient musicEventClient)
        {
            this.logger = logger;
            this.hubContext = hubContext;
            this.musicEventClient = musicEventClient;
        }

        public void InitializeMonitoring(HubCallerContext callerContext)
        {
            var feature = callerContext.Features.Get<IConnectionHeartbeatFeature>();
            feature.OnHeartbeat(_ =>
            {
                if (this.pendingKickConnectionIds.Contains(callerContext.ConnectionId))
                {
                    callerContext.Abort();
                    lock (this.pendingKickLock)
                    {
                        this.pendingKickConnectionIds.Remove(callerContext.ConnectionId);
                    }
                }
            }, callerContext.ConnectionId);
        }

        public void KickClient(string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId) && !this.pendingKickConnectionIds.Contains(connectionId))
            {
                lock (this.pendingKickLock)
                {
                    this.pendingKickConnectionIds.Add(connectionId);
                }
            }
        }

        public async void ScheduleHubMethod(
            Func<Task<object>> task,
            IClientProxy clientProxy,
            string methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                this.logger.LogDebug("Received empty method in ScheduleHubMethod");
                return;
            }

            var param = await task();
            if (!(param is null))
            {
                this.ScheduleHubMethod(TimeSpan.Zero, clientProxy, methodName, param);
            }
        }

        public async void ScheduleHubMethod(TimeSpan waitTime, IClientProxy clientProxy, string methodName, object param = null)
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                this.logger.LogDebug("Received empty method in ScheduleHubMethod");
                return;
            }

            await Task.Delay(waitTime);
            try
            {
                if (param is null)
                {
                    await clientProxy.SendAsync(methodName);
                }
                else
                {
                    await clientProxy.SendAsync(methodName, param);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error trying to send scheduled method", ex);
            }
        }

        public async void SchedulePurge(
            TimeSpan waitTime,
            string roomCode,
            RoomAppService roomAppService,
            GameAppService gameAppService)
        {
            this.logger.LogTrace($"Scheduled room purge in {waitTime}");

            await Task.Delay(waitTime);

            var purgeRoomResult = roomAppService.PurgeRoom(new PurgeRoomCommand
            {
                RoomCode = roomCode
            });

            if (purgeRoomResult.IsFailure)
            {
                this.logger.LogDebug($"Purge room failed with error {purgeRoomResult.Error.Code}: {purgeRoomResult.Error.Message}");
                return;
            }

            await this.hubContext.Clients.Clients(purgeRoomResult.Value.GuestConnectionIds)
                .SendAsync(WebClientMethods.RemoveRoom);
            await this.musicEventClient.PostEventAsync(EventType.PurgedRoom, (Code: roomCode, Result: purgeRoomResult.Value));

            if (!string.IsNullOrWhiteSpace(purgeRoomResult.Value.ActiveGameId))
            {
                gameAppService.PurgeGame(new PurgeGameCommand
                {
                    GameId = purgeRoomResult.Value.ActiveGameId
                });
            }

            this.logger.LogTrace($"Succesfully purged room with code {roomCode}");
        }
    }
}