using GamePlaying.Application;
using GamePlaying.Application.Commands;
using GamePlaying.Application.Dto;
using Microsoft.AspNetCore.SignalR;
using MusicApi.Serverless.Client;
using MusicServer.Extensions;
using MusicServer.Hubs.Services;
using MusicServer.Models;
using MusicServer.Words;
using MusicStorageClient;
using SharedDomain;
using SharedDomain.Domain;
using SharedDomain.InfraEvents;
using SharedDomain.Messages.Commands;
using SharedDomain.Messages.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MusicServer.Hubs
{
    public class GameHub : Hub
    {
        private readonly MusicEventClient musicEventClient;
        private readonly WordProvider wordProvider;
        private readonly RoomAppService roomAppService;
        private readonly GameAppService gameAppService;
        private readonly MusicDbApiClient musicDbApiClient;
        private readonly EconomyClient economyClient;
        private readonly GoogleStorage googleStorage;
        private readonly MetadataService metadataService;
        private readonly ConnectionMonitoringService connectionMonitoringService;
        private readonly VersioningService versioningService;

        public GameHub(
            MusicEventClient musicEventClient,
            WordProvider wordProvider,
            RoomAppService roomAppService,
            GameAppService gameAppService,
            MusicDbApiClient musicDbApiClient,
            EconomyClient economyClient,
            GoogleStorage googleStorage,
            MetadataService metadataService,
            ConnectionMonitoringService connectionMonitoringService,
            VersioningService versioningService)
        {
            this.musicEventClient = musicEventClient;
            this.wordProvider = wordProvider;
            this.roomAppService = roomAppService;
            this.gameAppService = gameAppService;
            this.musicDbApiClient = musicDbApiClient;
            this.economyClient = economyClient;
            this.googleStorage = googleStorage;
            this.metadataService = metadataService;
            this.connectionMonitoringService = connectionMonitoringService;
            this.versioningService = versioningService;
        }

        public override Task OnConnectedAsync()
        {
            this.connectionMonitoringService.InitializeMonitoring(this.Context);
            return base.OnConnectedAsync();
        }

        public Task<HubResponse> BookRoom(BookRoomRequest request)
        {
            if (!this.versioningService.IsVersionSupported(request.HostVersion))
            {
                return Task.FromResult(
                    HubResponse.Error("This version of the game is no longer supported. Please update to the latest version."));
            }

            var bookRoomCommand = new BookRoomCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                HostPlatform = request.HostPlatform,
                HostVersion = request.HostVersion,
                OrganizerPlayfabId = request.PlayfabId,
                GameSetupAvailableColors = request.AvaialableColors,
                GameSetupAvailableCharacters = request.AvailableCharacters,
                LocaleId = request.LocaleId
            };

            var roomBookingResult = this.roomAppService.BookRoom(bookRoomCommand);
            if (roomBookingResult.IsFailure)
            {
                return Task.FromResult(HubResponse.Error(roomBookingResult.Error.Message));
            }

            var code = roomBookingResult.Value.RoomCode;

            // We no longer give free items
            // this.connectionMonitoringService.ScheduleHubMethod(async () =>
            //{
            //    var inventoryResponse = await this.economyClient.EnsureFreeItemsPolicyAsync(request.PlayfabId);
            //    if (inventoryResponse.Outcome == ProcessingOutcome.Processed)
            //    {
            //        return new NotificationMessage
            //        {
            //            Message = "New free items have been added to your inventory"
            //        };
            //    }
            //    return null;
            //}, this.Clients.Client(this.Context.ConnectionId), HostMethods.Message);

            // TODO: raise event in app service
            this.musicEventClient.PostEventAsync(EventType.CreatedRoom, new CreatedRoomEvent
            {
                PlayFabId = request.PlayfabId,
                RoomCode = code,
                LocaleId = request.LocaleId
            }).Forget();

            return Task.FromResult(HubResponse.Success(code));
        }

        public async Task<HubResponse> JoinRoom(JoinRoomRequest joinRoomRequest)
        {
            var registerOrRejoinCommand = new RegisterOrRejoinGuestCommand
            {
                GuestId = joinRoomRequest.GuestId,
                GuestNick = joinRoomRequest.GuestNick,
                RoomCode = joinRoomRequest.RoomCode,
                ConnectionId = this.Context.ConnectionId
            };

            var result = this.roomAppService.RegisterOrRejoin(registerOrRejoinCommand);
            // TODO: raise player rejoined domain event

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            // TODO: handle player rejoined domain event

            var registration = result.Value;
            if (registration.Rejoined)
            {
                var rejoinPlayerCommand = new RejoinPlayerCommand
                {
                    PlayerId = joinRoomRequest.GuestId,
                    RoomCode = joinRoomRequest.RoomCode
                };

                var rejoinPlayerResult = this.gameAppService.RejoinPlayer(rejoinPlayerCommand);

                await this.Clients.Client(result.Value.HostConnectionId).
                   SendAsync(HostMethods.PlayerRejoined, registration.Player);

                if (!string.IsNullOrWhiteSpace(registration.OldConnectionId))
                {
                    await this.Clients.Client(registration.OldConnectionId).SendAsync(WebClientMethods.Kick);
                }

                if (rejoinPlayerResult.IsSuccess)
                {
                    this.connectionMonitoringService.ScheduleHubMethod(
                        TimeSpan.FromMilliseconds(500),
                        this.Clients.Client(this.Context.ConnectionId),
                        rejoinPlayerResult.Value.LatestActionName,
                        rejoinPlayerResult.Value.LatestActionParam);
                }

                return HubResponse.Success(registration.Player);
            }

            await this.Clients.Client(registration.HostConnectionId)
                .SendAsync(HostMethods.PlayerJoined, registration.Player);

            this.musicEventClient.PostEventAsync(EventType.PlayerJoined, registration.Player).Forget();

            return HubResponse.Success(registration.Player);
        }

        public Task<HubResponse> CanRejoin(CanRejoinMessage message)
        {
            var result = this.roomAppService.CanRejoin(new CanRejoinCommand
            {
                GuestId = message.GuestId,
                RoomCode = message.RoomCode
            });

            if (result.IsFailure)
            {
                return Task.FromResult(HubResponse.Error(result.Error.Message));
            }

            return Task.FromResult(HubResponse.Success(result.Value));
        }

        public async Task<HubResponse> ActivateVip(ActivateVipMessage message)
        {
            var activateVipCommand = new ActivateVipCommand
            {
                RoomCode = message.RoomCode,
                HostConnectionId = this.Context.ConnectionId,
                InventoryItemId = message.InventoryItemId,
                DefaultVipPerks = message.DefaultPerks.Select(p => Enum.Parse<GamePlaying.Domain.RoomAggregate.VipPerk>(p.ToString())),
                // validate/check inventoryId (playfab)
                ActivateItemAsyncTask = async (playFabId, inventoryItemId) =>
                {
                    var activationResult = await this.economyClient.ActivateItemAsync(playFabId, inventoryItemId);
                    return activationResult.Success;
                }
            };
            var result = await this.roomAppService.ActivateVipAsync(activateVipCommand);
            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            return HubResponse.Success();
        }

        public Task<HubResponse> AddVipPerk(AddVipPerkMessage message)
        {
            var activatePerkCommand = new ActivateVipPerkCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode,
                Perk = Enum.Parse<GamePlaying.Domain.RoomAggregate.VipPerk>(message.Perk.ToString())
            };
            var result = this.roomAppService.ActivateVipPerk(activatePerkCommand);
            if (result.IsFailure)
            {
                return Task.FromResult(HubResponse.Error(result.Error.Message));
            }

            return Task.FromResult(HubResponse.Success());
        }

        public Task<HubResponse> RemoveVipPerk(RemovePerkMessage message)
        {
            var deactivatePerkCommand = new DeactivateVipPerkCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode,
                Perk = Enum.Parse<GamePlaying.Domain.RoomAggregate.VipPerk>(message.Perk.ToString())
            };
            var result = this.roomAppService.DeactivateVipPerk(deactivatePerkCommand);
            if (result.IsFailure)
            {
                return Task.FromResult(HubResponse.Error(result.Error.Message));
            }

            return Task.FromResult(HubResponse.Success());
        }

        public async Task<HubResponse> StartNewGame(StartNewGameMessage message)
        {
            var startNewGameCommand = new StartNewGameCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode,
                GetPlaylistsAsyncTask = async () =>
                {
                    var rawPlaylists = await this.musicDbApiClient.GetEnabledPlaylistsAsync(
                                            message.PlaylistOptions.Language,
                                            message.PlaylistOptions.AllowExplicit);

                    foreach (var playlist in rawPlaylists)
                    {
                        playlist.PictureHash = await this.googleStorage.GetFileMd5Async(playlist.PictureUrl);
                    }

                    return rawPlaylists.ConvertTo<IEnumerable<PlaylistDto>>();
                },
            };

            var result = await this.gameAppService.StartNewGameAsync(startNewGameCommand);
            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            this.musicEventClient.PostEventAsync(EventType.GameStarted, message).Forget();

            return HubResponse.Success(result.Value.Playlists);
        }

        public async Task<HubResponse> GetFullPlaylist(GetFullPlaylistMessage message)
        {
            var result = await this.gameAppService.GetFullPlaylistAsync(new GetFullPlaylistCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode,
                RandomRequested = string.IsNullOrWhiteSpace(message.PlaylistId),
                GetPlaylistAsyncTask = async () =>
                {
                    var playlist = await this.musicDbApiClient.GetPlaylistByIdAsync(message.PlaylistId, PlaylistViewModelType.Full);

                    var fullPlaylist = (playlist as FullPlaylistViewModel).ConvertTo<FullPlaylistDto>();
                    fullPlaylist.PictureHash = await this.googleStorage.GetFileMd5Async(fullPlaylist.PictureUrl);
                    foreach (var song in fullPlaylist.Songs)
                    {
                        song.PreviewHash = await this.googleStorage.GetFileMd5Async(song.PreviewUrl);
                    }

                    return fullPlaylist;
                },
                GetRandomPlaylistAsyncTask = async () =>
                {
                    var playlist = await this.musicDbApiClient.GetRandomPlaylistAsync(message.PlaylistOptions.AllowExplicit, message.PlaylistOptions.Language, true, PlaylistViewModelType.Full);

                    var fullPlaylist = (playlist as FullPlaylistViewModel).ConvertTo<FullPlaylistDto>();
                    fullPlaylist.PictureHash = await this.googleStorage.GetFileMd5Async(fullPlaylist.PictureUrl);
                    foreach (var song in fullPlaylist.Songs)
                    {
                        song.PreviewHash = await this.googleStorage.GetFileMd5Async(song.PreviewUrl);
                    }

                    return fullPlaylist;
                }
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            this.metadataService.CountPlayForPlaylist(result.Value.FullPlaylist.Id);

            return HubResponse.Success(result.Value.FullPlaylist.ConvertTo<FullPlaylistViewModel>());
        }

        public async Task<HubResponse> Ask(AskMessage message)
        {
            var result = this.gameAppService.Ask(new AskCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode,
                CorrectAnswer = message.CorrectAnswer
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            await this.Clients.Clients(result.Value.PlayerConnectionIds).SendAsync(WebClientMethods.Ask);
            this.musicEventClient.PostEventAsync(EventType.PlayedSong, message).Forget();

            return HubResponse.Success();
        }

        public async Task<HubResponse> AskSpeed(AskMessage message)
        {
            var result = this.gameAppService.AskSpeed(new AskSpeedCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            await this.Clients.Clients(result.Value.PlayerConnectionIds).SendAsync(WebClientMethods.AskSpeed);
            this.musicEventClient.PostEventAsync(EventType.PlayedSong, message).Forget();

            return HubResponse.Success();
        }

        public async Task<HubResponse> Answer(AnswerMessage message)
        {
            var result = this.gameAppService.Answer(new AnswerCommand
            {
                ConnectionId = this.Context.ConnectionId,
                AnswerText = message.Answer.Name,
                PlayerId = message.Answer.Player.Id,
                RoomCode = message.RoomCode
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            await this.Clients.Client(result.Value.HostConnectionId).SendAsync(HostMethods.GetAnswer, result.Value.Answer);

            return HubResponse.Success();
        }

        public async Task<HubResponse> AnswerSpeed(AnswerMessage message)
        {
            var result = this.gameAppService.AnswerSpeed(new AnswerSpeedCommand
            {
                ConnectionId = this.Context.ConnectionId,
                AnswerText = message.Answer.Name,
                PlayerId = message.Answer.Player.Id,
                RoomCode = message.RoomCode
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            await this.Clients.Client(result.Value.HostConnectionId).SendAsync(HostMethods.GetSpeedAnswer, result.Value.Answer);

            return HubResponse.Success();
        }

        public async Task<HubResponse> GetMissingAnswers(GetMissingAnswersMessage message)
        {
            var result = await this.gameAppService.GetMissingAnswersAsync(new GetMissingAnswersCommand
            {
                PlayerIds = message.PlayerIds,
                RoomCode = message.RoomCode,
                GetAnswerTextsTask = async () =>
                {
                    var words = new List<string>();
                    for (int i = 0; i < message.PlayerIds.Count(); i++)
                    {
                        var word = await this.wordProvider.GetRandomWordAsync();
                        words.Add(word);
                    }

                    return words;
                }
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            await this.Clients.Clients(result.Value.PlayerConnectionIds).SendAsync(WebClientMethods.NotifyAutogeneratedAnswer);

            return HubResponse.Success(result.Value.Answers);
        }

        public async Task<HubResponse> StartVoting(StartVotingMessage message)
        {
            var result = this.gameAppService.StartVoting(new StartVotingCommand
            {
                RoomCode = message.RoomCode,
                Answers = message.Answers.ConvertTo<IEnumerable<AnswerDto>>()
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            foreach (var pair in result.Value.PlayerAnswerPairs)
            {
                await this.Clients.Client(pair.Key).SendAsync(WebClientMethods.StartVoting, pair.Value);
            }

            return HubResponse.Success();
        }

        public async Task<HubResponse> VoteAnswer(Vote<Answer> vote)
        {
            var result = this.gameAppService.RecordPlayersVote(new RecordVoteCommand
            {
                ConnectionId = this.Context.ConnectionId,
                PlayerId = vote.By.Id,
                RoomCode = vote.Code
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            await this.Clients.Client(result.Value.HostConnectionId).SendAsync(HostMethods.VoteAnswer, vote);

            return HubResponse.Success();
        }

        public async Task<HubResponse> Relax(RelaxMessage message)
        {
            var command = new RelaxCommand
            {
                PlayerOrGuestIds = message.PlayerOrGuestIds,
                RoomCode = message.RoomCode
            };
            var gameCommand = new AnyActiveGameCommand { RoomCode = message.RoomCode };
            var gameResult = this.roomAppService.AnyActiveGame(gameCommand);

            if (gameResult.IsFailure)
            {
                return HubResponse.Error(gameResult.Error.Message);
            }

            var result = gameResult.Value.AnyActiveGame ? this.gameAppService.Relax(command) : this.roomAppService.Relax(command);

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            await this.Clients.Clients(result.Value.PlayerOrGuestConnectionIds).SendAsync(WebClientMethods.Relax);

            return HubResponse.Success();
        }

        public Task<HubResponse> RejoinHost(string code)
        {
            //TODO: refactor with Result<Dto, Error> and HubResponse
            var rejoinOrganizerCommand = new RejoinOrganizerCommand
            {
                RoomCode = code,
                ConnectionId = this.Context.ConnectionId
            };

            var result = this.roomAppService.RejoinOrganizer(rejoinOrganizerCommand);
            if (result.IsFailure)
            {
                return Task.FromResult(HubResponse.Error(result.Error.Message));
            }

            return Task.FromResult(HubResponse.Success());
        }

        public Task<HubResponse> QuitGame(QuitGameMessage message)
        {
            var command = new QuitGameCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode
            };

            var result = this.gameAppService.QuitGame(command);
            if (result.IsFailure)
            {
                return Task.FromResult(HubResponse.Error(result.Error.Message));
            }

            this.musicEventClient.PostEventAsync(EventType.GameQuit, new
            {
                Message = message,
                Command = command,
                Result = result.Value
            });

            this.Clients.Clients(result.Value.PlayerConnectionIds).SendAsync(WebClientMethods.EndGame);

            return Task.FromResult(HubResponse.Success());
        }

        public async Task<HubResponse> EndGame(EndGameMessage message)
        {
            var command = new EndGameCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode
            };

            var result = this.gameAppService.EndGame(command);
            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            this.metadataService.PushPlaylistsMetadata();

            await this.Clients.Clients(result.Value.PlayerOrGuestConnectionIds).SendAsync(WebClientMethods.EndGame);
            if (!string.IsNullOrWhiteSpace(result.Value.InventoryItemId))
            {
                await this.economyClient.ConsumeItemAsync(result.Value.PlayFabId, result.Value.InventoryItemId);
            }
            this.musicEventClient.PostEventAsync(EventType.GameEnded, result.Value).Forget();

            return HubResponse.Success();
        }

        public async Task<HubResponse> RemoveRoom(RemoveRoomMessage message)
        {
            var result = this.roomAppService.RemoveRoom(new RemoveRoomCommand
            {
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            if (!string.IsNullOrWhiteSpace(result.Value.ActiveGameId))
            {
                var purgeGameResult = this.gameAppService.PurgeGame(new PurgeGameCommand { GameId = result.Value.ActiveGameId });
                if (purgeGameResult.IsFailure)
                {
                    return HubResponse.Error(purgeGameResult.Error.Message);
                }

                // We do not consume items at end of game anymore
                //if (!string.IsNullOrWhiteSpace(purgeGameResult.Value.InventoryItemId))
                //{
                //    await this.economyClient.ConsumeItemAsync(result.Value.PlayFabId, purgeGameResult.Value.InventoryItemId);
                //}
            }

            await this.Clients.Clients(result.Value.GuestConnectionIds).SendAsync(WebClientMethods.RemoveRoom);
            this.musicEventClient.PostEventAsync(EventType.RemovedRoom, message).Forget();

            return HubResponse.Success();
        }

        public async Task<HubResponse> PushEvents(PushEventsMessage message)
        {
            var result = this.roomAppService.ValidateEventPush(new ValidateEventPushCommand
            {
                RoomCode = message.RoomCode,
                ConnectionId = this.Context.ConnectionId
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            foreach (var @event in message.Events)
            {
                if (@event.PayloadType.Equals(nameof(PurchasedItemPayload)))
                {
                    var payload = @event.GetPayloadAs<PurchasedItemPayload>();
                    payload.PlayFabId = result.Value.PlayFabId;
                    @event.UpdatePayload(payload);
                }
            }

            var pushResult = await this.musicEventClient.PutEventsAsync(message.Events);
            if (!pushResult)
            {
                return HubResponse.Error("Failed to push events to database.");
            }

            return HubResponse.Success();
        }

        public async Task<HubResponse> KickGuests(KickGuestsMessage message)
        {
            var result = this.roomAppService.KickGuests(new KickGuestsCommand
            {
                GuestIds = message.GuestIds,
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            await this.Clients.Clients(result.Value.GuestConnectionIds).SendAsync(WebClientMethods.Kick);

            return HubResponse.Success();
        }

        public async Task<HubResponse> KickPlayer(KickPlayerMessage message)
        {
            var result = this.gameAppService.KickPlayer(new KickPlayerCommand
            {
                PlayerId = message.PlayerId,
                HostConnectionId = this.Context.ConnectionId,
                RoomCode = message.RoomCode
            });

            if (result.IsFailure)
            {
                return HubResponse.Error(result.Error.Message);
            }

            await this.Clients.Client(result.Value.PlayerConnectionId).SendAsync(WebClientMethods.Kick);

            return HubResponse.Success();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var guestResult = this.roomAppService.TryDisconnectGuest(new TryDisconnectGuestCommand
            {
                ConnectionId = this.Context.ConnectionId
            });
            if (guestResult.IsSuccess)
            {
                if (!string.IsNullOrWhiteSpace(guestResult.Value.ActiveGameId))
                {
                    this.gameAppService.DisconnectPlayer(new DisconnectPlayerCommand
                    {
                        GameId = guestResult.Value.ActiveGameId,
                        PlayerId = guestResult.Value.GuestId
                    });
                }

                this.Clients.Client(guestResult.Value.HostConnectionId).SendAsync(HostMethods.PlayerDisconnected, guestResult.Value.GuestId);

                return base.OnDisconnectedAsync(exception);
            }

            var hostResult = this.roomAppService.TryDisconnectHost(new TryDisconnectHostCommand
            {
                ConnectionId = this.Context.ConnectionId
            });
            if (hostResult.IsSuccess)
            {
                this.Clients.Clients(hostResult.Value.GuestConnectionIds).SendAsync(WebClientMethods.HostDisconnected);
                this.musicEventClient.PostEventAsync(EventType.HostDisconnected, hostResult.Value);

                this.connectionMonitoringService.SchedulePurge(
                    TimeSpan.FromMinutes(10),
                    hostResult.Value.RoomCode,
                    this.roomAppService,
                    this.gameAppService);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}