using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Newtonsoft.Json;
using SharedDomain;
using SharedDomain.InfraEvents;
using SharedDomain.Messages.Commands;
using SharedDomain.Messages.Queries;
using SharedDomain.Purchasing;
using SignalrCoreWrapper;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Services
{
    public class MusicClient : UnityEventProvider
    {
        private readonly MusicHubClient hubClient;
        private readonly HttpClient httpClient;

        private string roomCode;

        public UnityEvent<Player> PlayerJoined { get; } = new PlayerEvent();
        public UnityEvent<Answer> Answered { get; } = new AnswerEvent();
        public UnityEvent<SpeedAnswer> SpeedAnswered { get; } = new SpeedAnswerEvent();
        public UnityEvent<Vote<Answer>> VotedAnswer { get; } = new VoteAnswerEvent();
        public UnityEvent<Player> PlayerRejoined { get; } = new PlayerEvent();
        public UnityEvent<string> PlayerDisconnected { get; } = new StringEvent();
        public UnityEvent<Exception> Disconnected { get; } = new ExceptionEvent();
        public UnityEvent<string> Reconnected { get; } = new StringEvent();
        public UnityEvent<Exception> Reconnecting { get; } = new ExceptionEvent();
        public UnityEvent<NotificationMessage> Message { get; } = new NotificationEvent();
        public UnityEvent GameEnded { get; } = new UnityEvent();

        public MusicClient(string hubUrl, string apiUrl)
        {
            this.httpClient = new HttpClient() { BaseAddress = new Uri(apiUrl) };
            this.hubClient = new MusicHubClient(hubUrl);

            this.hubClient.On<Player>(HostMethods.PlayerJoined, p =>
                UnityMainThreadDispatcher.Instance().Enqueue(() => this.PlayerJoined.Invoke(p)));
            this.hubClient.On<Answer>(HostMethods.GetAnswer, a =>
                UnityMainThreadDispatcher.Instance().Enqueue(() => this.Answered.Invoke(a)));
            this.hubClient.On<SpeedAnswer>(HostMethods.GetSpeedAnswer, a =>
                UnityMainThreadDispatcher.Instance().Enqueue(() => this.SpeedAnswered.Invoke(a)));
            this.hubClient.On<Vote<Answer>>(HostMethods.VoteAnswer, a =>
                UnityMainThreadDispatcher.Instance().Enqueue(() => this.VotedAnswer.Invoke(a)));
            this.hubClient.On<Player>(HostMethods.PlayerRejoined, p =>
                UnityMainThreadDispatcher.Instance().Enqueue(() => this.PlayerRejoined.Invoke(p)));
            this.hubClient.On<string>(HostMethods.PlayerDisconnected, id =>
                 UnityMainThreadDispatcher.Instance().Enqueue(() => this.PlayerDisconnected.Invoke(id)));
            this.hubClient.On<NotificationMessage>(HostMethods.Message, message =>
                UnityMainThreadDispatcher.Instance().Enqueue(() => this.Message.Invoke(message)));

            this.hubClient.Disconnected += error =>
                UnityMainThreadDispatcher.Instance().Enqueue(() => this.Disconnected.Invoke(error));
            this.hubClient.Reconnected += connectionId =>
            {
                this.SetHostHeader();
                UnityMainThreadDispatcher.Instance().Enqueue(() => this.Reconnected.Invoke(connectionId));
            };
            this.hubClient.Reconnecting += error =>
            UnityMainThreadDispatcher.Instance().Enqueue(() => this.Reconnecting.Invoke(error));
        }

        public Task ConnectAsync()
        {
            if (this.hubClient.State == MusicHubClient.ConnectionState.Disconnected)
            {
                return this.hubClient.ConnectAsync();
            }
            return Task.CompletedTask;
        }

        public async Task ScheduleActionAsync(TimeSpan timeSpan, string methodName, object message)
        {
            await Task.Delay(timeSpan);
            await this.hubClient.SendAsync(methodName, message);
        }

        public Task<HubResponse> BookRoomAsync(BookRoomRequest request)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.BookRoom, request);
        }

        public void BindEventsToRoom(Room room)
        {
            this.roomCode = room.Code;
            this.SetHostHeader();

            this.PlayerDisconnected.RemoveAllListeners();
            this.PlayerRejoined.RemoveAllListeners();
            this.Reconnecting.RemoveAllListeners();
            this.Reconnected.RemoveAllListeners();

            this.PlayerDisconnected.AddListener(id =>
            {
                room.Players.Find(p => string.Equals(id, p.Id)).IsConnected = false;
            });
            this.PlayerRejoined.AddListener(newPlayer =>
            {
                var existingPlayer = room.Players.Find(p => string.Equals(newPlayer.Id, p.Id));
                existingPlayer.IsConnected = true;
                existingPlayer.Nick = newPlayer.Nick;
            });

            this.Reconnecting.AddListener(e =>
               {
                   Debug.LogWarning("Disconnected from GameHub");
                   Time.timeScale = 0;
                   ReconnectPanelScript.Instantiate();
               });

            this.Reconnected.AddListener(async id =>
                {
                    Debug.Log("Reconnected to GameHub");
                    try
                    {
                        var result = await this.RejoinHost(this.roomCode);
                        if (result.IsSuccess)
                        {
                            ReconnectPanelScript.Destroy();
                            Time.timeScale = 1;
                            return;
                        }
                        ErrorPanelScript.Instantiate("Could not reconnect to server.");
                    }
                    catch (Exception)
                    {
                        ErrorPanelScript.Instantiate("Could not reconnect to server.");
                    }
                });
        }

        public Task<HubResponse> StartNewGameAsync(PlaylistOptions options)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.StartNewGame, new StartNewGameMessage
            {
                RoomCode = this.roomCode,
                PlaylistOptions = options
            });
        }

        public Task<HubResponse> GetFullPlaylistAsync(string playlistId, PlaylistOptions options)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.GetFullPlaylist, new GetFullPlaylistMessage
            {
                PlaylistId = playlistId,
                RoomCode = this.roomCode,
                PlaylistOptions = options
            });
        }

        public Task<HubResponse> AskAsync(string songId, string correctAnswer)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.Ask, new AskMessage
            {
                RoomCode = this.roomCode,
                CorrectAnswer = correctAnswer,
                SongId = songId
            });
        }

        public Task<HubResponse> AskSpeedAsync(string songId)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.AskSpeed, new AskMessage
            {
                RoomCode = this.roomCode,
                SongId = songId
            });
        }

        public Task<HubResponse> GetMissingAnswersAsync(IEnumerable<string> playerIds)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.GetMissingAnswers, new GetMissingAnswersMessage
            {
                RoomCode = this.roomCode,
                PlayerIds = playerIds
            }); ;
        }

        public Task StartVotingAsync(IEnumerable<Answer> answers)
        {
            return this.hubClient.InvokeAsync(ServerMethods.StartVoting, new StartVotingMessage
            {
                RoomCode = this.roomCode,
                Answers = answers
            });
        }

        public Task<HubResponse> QuitGameAsync()
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.QuitGame, new QuitGameMessage { RoomCode = this.roomCode });
        }

        public async Task<HubResponse> EndGameWaitAsync()
        {
            var response = await this.hubClient.InvokeAsync<HubResponse>(ServerMethods.EndGame, new EndGameMessage { RoomCode = this.roomCode });
            if (response.IsSuccess)
            {
                this.GameEnded.Invoke();
            }
            return response;
        }

        public Task EndGameAsync()
        {
            return this.hubClient.SendAsync(ServerMethods.EndGame, new EndGameMessage { RoomCode = this.roomCode });
        }

        public Task<HubResponse> RemoveRoomWaitAsync()
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.RemoveRoom, new RemoveRoomMessage { RoomCode = this.roomCode });
        }

        public Task RemoveRoomAsync()
        {
            return this.hubClient.SendAsync(ServerMethods.RemoveRoom, new RemoveRoomMessage { RoomCode = this.roomCode });
        }

        public Task<HubResponse> RejoinHost(string code)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.RejoinHost, code);
        }

        public Task DisconnectAsync()
        {
            return this.hubClient.DisconnectAsync();
        }

        public Task RelaxAsync(string playerId)
        {
            return this.hubClient.SendAsync(ServerMethods.Relax, new RelaxMessage
            {
                RoomCode = this.roomCode,
                PlayerOrGuestIds = new List<string> { playerId }
            });
        }

        public Task RelaxAsync(IEnumerable<string> playerIds)
        {
            return this.hubClient.SendAsync(ServerMethods.Relax, new RelaxMessage
            {
                RoomCode = this.roomCode,
                PlayerOrGuestIds = playerIds
            });
        }

        public Task<string> GetFileMd5Async(string songUrl)
        {
            return this.httpClient.GetStringAsync($"/api/game/GetFileMd5/?fileUrl={Uri.EscapeDataString(songUrl)}");
        }

        public async Task<byte[]> DownloadSongAsync(string songUrl)
        {
            var signedUrl = await this.httpClient.GetStringAsync
                ($"/api/game/GetSignedSongPreviewUrl/?previewUrl={Uri.EscapeDataString(songUrl)}");

            byte[] result;
            using (var client = new HttpClient())
            {
                result = await client.GetByteArrayAsync(signedUrl);
            }

            return result;
        }

        public async Task<SteamUserInfoResponseDto> GetSteamUserInfoAsync(string steamId)
        {
            var json = await this.httpClient.GetStringAsync($"/api/game/getsteamuserinfo/{steamId}");

            return JsonConvert.DeserializeObject<SteamUserInfoResponseDto>(json);
        }

        public Task<HubResponse> PushEventsAsync(IEnumerable<MusicEvent> events)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.PushEvents, events);
        }

        public Task<HubResponse> KickGuestsAsync(IEnumerable<string> guestIds)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.KickGuests, new KickGuestsMessage
            {
                RoomCode = this.roomCode,
                GuestIds = guestIds
            });
        }

        public Task<HubResponse> KickPlayerAsync(string playerId)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.KickPlayer, new KickPlayerMessage
            {
                PlayerId = playerId,
                RoomCode = this.roomCode
            });
        }

        public Task<HubResponse> ActivateVipAsync(string inventoryItemId, List<VipPerk> perks)
        {
            return this.hubClient.InvokeAsync<HubResponse>(ServerMethods.ActivateVip, new ActivateVipMessage
            {
                RoomCode = this.roomCode,
                InventoryItemId = inventoryItemId,
                DefaultPerks = perks
            });
        }

        private void SetHostHeader()
        {
            this.httpClient.DefaultRequestHeaders.Remove("X-HostConnectionId");
            this.httpClient.DefaultRequestHeaders.Add("X-HostConnectionId", this.hubClient.ConnectionId);
        }

        public class PlayerEvent : UnityEvent<Player> { }

        public class ExceptionEvent : UnityEvent<System.Exception> { }

        public class StringEvent : UnityEvent<string> { }

        public class NotificationEvent : UnityEvent<NotificationMessage> { }

        public class AnswerEvent : UnityEvent<Answer> { }

        public class VoteAnswerEvent : UnityEvent<Vote<Answer>> { }

        public class SpeedAnswerEvent : UnityEvent<SpeedAnswer> { }
    }
}