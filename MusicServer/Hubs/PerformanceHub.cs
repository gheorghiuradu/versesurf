using GamePlaying.Application;
using GamePlaying.Application.Commands;
using Microsoft.AspNetCore.SignalR;
using MusicServer.PerformanceTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MusicServer.Hubs
{
    public class PerformanceHub : Hub
    {
        private readonly ConnectionCounter _counter;
        private readonly RoomAppService roomAppService;

        public PerformanceHub(ConnectionCounter counter, RoomAppService roomAppService)
        {
            _counter = counter;
            this.roomAppService = roomAppService;
        }

        public async Task Broadcast(int duration)
        {
            var sent = 0;
            try
            {
                var t = new CancellationTokenSource();
                t.CancelAfter(TimeSpan.FromSeconds(duration));
                while (!t.IsCancellationRequested && !Context.ConnectionAborted.IsCancellationRequested)
                {
                    await Clients.All.SendAsync("send", DateTime.UtcNow);
                    sent++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Broadcast exited: Sent {0} messages", sent);
        }

        public override Task OnConnectedAsync()
        {
            this.roomAppService.BookRoom(new BookRoomCommand
            {
                GameSetupAvailableColors = new List<string>(),
                GameSetupAvailableCharacters = new List<string>(),
                HostConnectionId = this.Context.ConnectionId,
                HostPlatform = "PerformanceTesting",
                HostVersion = "1",
                OrganizerPlayfabId = Guid.NewGuid().ToString()
            });

            _counter?.Connected();
            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var result = this.roomAppService
                .TryDisconnectHost(new TryDisconnectHostCommand { ConnectionId = this.Context.ConnectionId });
            this.roomAppService.PurgeRoom(new PurgeRoomCommand { RoomCode = result.Value.RoomCode });

            _counter?.Disconnected();
            return Task.CompletedTask;
        }

        public DateTime Echo(DateTime time)
        {
            return time;
        }

        public Task EchoAll(DateTime time)
        {
            return Clients.All.SendAsync("send", time);
        }

        public void SendPayload(string payload)
        {
            _counter?.Receive(payload);
        }

        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}