using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamePlaying.Application;
using GamePlaying.Application.Commands;
using GamePlaying.Application.Dto;
using Microsoft.AspNetCore.SignalR;
using MusicDbApi;
using MusicServer.Extensions;
using MusicServer.Hubs.Services;
using MusicServer.Models;
using MusicServer.Words;
using SharedDomain;
using SharedDomain.Domain;
using SharedDomain.InfraEvents;
using SharedDomain.Messages.Commands;
using SharedDomain.Messages.Queries;

namespace MusicServer.Hubs;

public class GameHub : Hub
{
    private readonly RoomAppService roomAppService;
    private readonly GameAppService gameAppService;
    private readonly MusicDbClient musicDbClient;
    private readonly MetadataService metadataService;
    private readonly ConnectionMonitoringService connectionMonitoringService;
    private readonly VersioningService versioningService;
    private readonly WordProvider wordProvider;

    public GameHub(
        RoomAppService roomAppService,
        GameAppService gameAppService,
        MusicDbClient musicDbClient,
        MetadataService metadataService,
        ConnectionMonitoringService connectionMonitoringService,
        VersioningService versioningService,
        WordProvider wordProvider)
    {
        this.roomAppService = roomAppService;
        this.gameAppService = gameAppService;
        this.musicDbClient = musicDbClient;
        this.metadataService = metadataService;
        this.connectionMonitoringService = connectionMonitoringService;
        this.versioningService = versioningService;
        this.wordProvider = wordProvider;
    }

    public override Task OnConnectedAsync()
    {
        connectionMonitoringService.InitializeMonitoring(Context);
        return base.OnConnectedAsync();
    }

    public Task<HubResponse> BookRoom(BookRoomRequest request)
    {
        if (!versioningService.IsVersionSupported(request.HostVersion))
            return Task.FromResult(
                HubResponse.Error("This version of the game is no longer supported. Please update to the latest version."));

        var bookRoomCommand = new BookRoomCommand
        {
            HostConnectionId = Context.ConnectionId,
            HostPlatform = request.HostPlatform,
            HostVersion = request.HostVersion,
            OrganizerPlayfabId = request.PlayfabId,
            GameSetupAvailableColors = request.AvaialableColors,
            GameSetupAvailableCharacters = request.AvailableCharacters,
            LocaleId = request.LocaleId
        };

        var roomBookingResult = roomAppService.BookRoom(bookRoomCommand);
        if (roomBookingResult.IsFailure) return Task.FromResult(HubResponse.Error(roomBookingResult.Error.Message));

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

        return Task.FromResult(HubResponse.Success(code));
    }

    public async Task<HubResponse> JoinRoom(JoinRoomRequest joinRoomRequest)
    {
        var registerOrRejoinCommand = new RegisterOrRejoinGuestCommand
        {
            GuestId = joinRoomRequest.GuestId,
            GuestNick = joinRoomRequest.GuestNick,
            RoomCode = joinRoomRequest.RoomCode,
            ConnectionId = Context.ConnectionId
        };

        var result = roomAppService.RegisterOrRejoin(registerOrRejoinCommand);
        // TODO: raise player rejoined domain event

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        // TODO: handle player rejoined domain event

        var registration = result.Value;
        if (registration.Rejoined)
        {
            var rejoinPlayerCommand = new RejoinPlayerCommand
            {
                PlayerId = joinRoomRequest.GuestId,
                RoomCode = joinRoomRequest.RoomCode
            };

            var rejoinPlayerResult = gameAppService.RejoinPlayer(rejoinPlayerCommand);

            await Clients.Client(result.Value.HostConnectionId).SendAsync(HostMethods.PlayerRejoined, registration.Player);

            if (!string.IsNullOrWhiteSpace(registration.OldConnectionId)) await Clients.Client(registration.OldConnectionId).SendAsync(WebClientMethods.Kick);

            if (rejoinPlayerResult.IsSuccess)
                connectionMonitoringService.ScheduleHubMethod(
                    TimeSpan.FromMilliseconds(500),
                    Clients.Client(Context.ConnectionId),
                    rejoinPlayerResult.Value.LatestActionName,
                    rejoinPlayerResult.Value.LatestActionParam);

            return HubResponse.Success(registration.Player);
        }

        await Clients.Client(registration.HostConnectionId)
            .SendAsync(HostMethods.PlayerJoined, registration.Player);

        return HubResponse.Success(registration.Player);
    }

    public Task<HubResponse> CanRejoin(CanRejoinMessage message)
    {
        var result = roomAppService.CanRejoin(new CanRejoinCommand
        {
            GuestId = message.GuestId,
            RoomCode = message.RoomCode
        });

        if (result.IsFailure) return Task.FromResult(HubResponse.Error(result.Error.Message));

        return Task.FromResult(HubResponse.Success(result.Value));
    }

    public async Task<HubResponse> ActivateVip(ActivateVipMessage message)
    {
        var activateVipCommand = new ActivateVipCommand
        {
            RoomCode = message.RoomCode,
            HostConnectionId = Context.ConnectionId,
            InventoryItemId = message.InventoryItemId,
            DefaultVipPerks = message.DefaultPerks.Select(p => Enum.Parse<GamePlaying.Domain.RoomAggregate.VipPerk>(p.ToString())),
            // In self-hosted mode, VIP activation is always allowed
            ActivateItemAsyncTask = async (playFabId, inventoryItemId) => { return await Task.FromResult(true); }
        };
        var result = await roomAppService.ActivateVipAsync(activateVipCommand);
        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        return HubResponse.Success();
    }

    public Task<HubResponse> AddVipPerk(AddVipPerkMessage message)
    {
        var activatePerkCommand = new ActivateVipPerkCommand
        {
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode,
            Perk = Enum.Parse<GamePlaying.Domain.RoomAggregate.VipPerk>(message.Perk.ToString())
        };
        var result = roomAppService.ActivateVipPerk(activatePerkCommand);
        if (result.IsFailure) return Task.FromResult(HubResponse.Error(result.Error.Message));

        return Task.FromResult(HubResponse.Success());
    }

    public Task<HubResponse> RemoveVipPerk(RemovePerkMessage message)
    {
        var deactivatePerkCommand = new DeactivateVipPerkCommand
        {
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode,
            Perk = Enum.Parse<GamePlaying.Domain.RoomAggregate.VipPerk>(message.Perk.ToString())
        };
        var result = roomAppService.DeactivateVipPerk(deactivatePerkCommand);
        if (result.IsFailure) return Task.FromResult(HubResponse.Error(result.Error.Message));

        return Task.FromResult(HubResponse.Success());
    }

    public async Task<HubResponse> StartNewGame(StartNewGameMessage message)
    {
        var startNewGameCommand = new StartNewGameCommand
        {
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode,
            GetPlaylistsAsyncTask = async () =>
            {
                var rawPlaylists = await musicDbClient.GetEnabledPlaylistsAsync(
                    message.PlaylistOptions.Language,
                    message.PlaylistOptions.AllowExplicit,
                    message.PlaylistOptions.NumberOfSongs);
                var playlistViewModels = rawPlaylists.ConvertTo<List<PlaylistViewModel>>();
                foreach (var playlistViewModel in playlistViewModels)
                {
                    var rawPlaylist = rawPlaylists.First(p => p.Id == playlistViewModel.Id);
                    playlistViewModel.KeyWords = string.Join(", ", rawPlaylist.Songs.SelectMany(s => new[] { s.Title, s.Artist }).Concat([rawPlaylist.Name]));
                }

                return playlistViewModels.ConvertTo<IEnumerable<PlaylistDto>>();
            }
        };

        var result = await gameAppService.StartNewGameAsync(startNewGameCommand);
        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        return HubResponse.Success(result.Value.Playlists);
    }

    public async Task<HubResponse> GetFullPlaylist(GetFullPlaylistMessage message)
    {
        var result = await gameAppService.GetFullPlaylistAsync(new GetFullPlaylistCommand
        {
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode,
            RandomRequested = string.IsNullOrWhiteSpace(message.PlaylistId),
            GetPlaylistAsyncTask = async () =>
            {
                var playlist = await musicDbClient.GetPlaylistByIdAsync(message.PlaylistId);

                var fullPlaylist = playlist.ConvertTo<FullPlaylistDto>();

                return fullPlaylist;
            },
            GetRandomPlaylistAsyncTask = async () =>
            {
                var playlist = await musicDbClient.GetRandomPlaylistAsync(message.PlaylistOptions.Language, message.PlaylistOptions.AllowExplicit);
                var fullPlaylist = playlist.ConvertTo<FullPlaylistDto>();

                return fullPlaylist;
            }
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        metadataService.CountPlayForPlaylist(result.Value.FullPlaylist.Id);

        return HubResponse.Success(result.Value.FullPlaylist.ConvertTo<FullPlaylistViewModel>());
    }

    public async Task<HubResponse> Ask(AskMessage message)
    {
        var result = gameAppService.Ask(new AskCommand
        {
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode,
            CorrectAnswer = message.CorrectAnswer
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        await Clients.Clients(result.Value.PlayerConnectionIds).SendAsync(WebClientMethods.Ask);

        return HubResponse.Success();
    }

    public async Task<HubResponse> AskSpeed(AskMessage message)
    {
        var result = gameAppService.AskSpeed(new AskSpeedCommand
        {
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        await Clients.Clients(result.Value.PlayerConnectionIds).SendAsync(WebClientMethods.AskSpeed);

        return HubResponse.Success();
    }

    public async Task<HubResponse> Answer(AnswerMessage message)
    {
        var result = gameAppService.Answer(new AnswerCommand
        {
            ConnectionId = Context.ConnectionId,
            AnswerText = message.Answer.Name,
            PlayerId = message.Answer.Player.Id,
            RoomCode = message.RoomCode
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        await Clients.Client(result.Value.HostConnectionId).SendAsync(HostMethods.GetAnswer, result.Value.Answer);

        return HubResponse.Success();
    }

    public async Task<HubResponse> AnswerSpeed(AnswerMessage message)
    {
        var result = gameAppService.AnswerSpeed(new AnswerSpeedCommand
        {
            ConnectionId = Context.ConnectionId,
            AnswerText = message.Answer.Name,
            PlayerId = message.Answer.Player.Id,
            RoomCode = message.RoomCode
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        await Clients.Client(result.Value.HostConnectionId).SendAsync(HostMethods.GetSpeedAnswer, result.Value.Answer);

        return HubResponse.Success();
    }

    public async Task<HubResponse> GetMissingAnswers(GetMissingAnswersMessage message)
    {
        var result = await gameAppService.GetMissingAnswersAsync(new GetMissingAnswersCommand
        {
            PlayerIds = message.PlayerIds,
            RoomCode = message.RoomCode,
            GetAnswerTextsTask = async () =>
            {
                var words = new List<string>();
                for (var i = 0; i < message.PlayerIds.Count(); i++)
                {
                    var word = await wordProvider.GetRandomWordAsync();
                    words.Add(word);
                }

                return words;
            }
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        await Clients.Clients(result.Value.PlayerConnectionIds).SendAsync(WebClientMethods.NotifyAutogeneratedAnswer);

        return HubResponse.Success(result.Value.Answers);
    }

    public async Task<HubResponse> StartVoting(StartVotingMessage message)
    {
        var result = gameAppService.StartVoting(new StartVotingCommand
        {
            RoomCode = message.RoomCode,
            Answers = message.Answers.ConvertTo<IEnumerable<AnswerDto>>()
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        foreach (var pair in result.Value.PlayerAnswerPairs) await Clients.Client(pair.Key).SendAsync(WebClientMethods.StartVoting, pair.Value);

        return HubResponse.Success();
    }

    public async Task<HubResponse> VoteAnswer(Vote<Answer> vote)
    {
        var result = gameAppService.RecordPlayersVote(new RecordVoteCommand
        {
            ConnectionId = Context.ConnectionId,
            PlayerId = vote.By.Id,
            RoomCode = vote.Code
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        await Clients.Client(result.Value.HostConnectionId).SendAsync(HostMethods.VoteAnswer, vote);

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
        var gameResult = roomAppService.AnyActiveGame(gameCommand);

        if (gameResult.IsFailure) return HubResponse.Error(gameResult.Error.Message);

        var result = gameResult.Value.AnyActiveGame ? gameAppService.Relax(command) : roomAppService.Relax(command);

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        await Clients.Clients(result.Value.PlayerOrGuestConnectionIds).SendAsync(WebClientMethods.Relax);

        return HubResponse.Success();
    }

    public Task<HubResponse> RejoinHost(string code)
    {
        //TODO: refactor with Result<Dto, Error> and HubResponse
        var rejoinOrganizerCommand = new RejoinOrganizerCommand
        {
            RoomCode = code,
            ConnectionId = Context.ConnectionId
        };

        var result = roomAppService.RejoinOrganizer(rejoinOrganizerCommand);
        if (result.IsFailure) return Task.FromResult(HubResponse.Error(result.Error.Message));

        return Task.FromResult(HubResponse.Success());
    }

    public Task<HubResponse> QuitGame(QuitGameMessage message)
    {
        var command = new QuitGameCommand
        {
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode
        };

        var result = gameAppService.QuitGame(command);
        if (result.IsFailure) return Task.FromResult(HubResponse.Error(result.Error.Message));

        Clients.Clients(result.Value.PlayerConnectionIds).SendAsync(WebClientMethods.EndGame);

        return Task.FromResult(HubResponse.Success());
    }

    public async Task<HubResponse> EndGame(EndGameMessage message)
    {
        var command = new EndGameCommand
        {
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode
        };

        var result = gameAppService.EndGame(command);
        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        metadataService.PushPlaylistsMetadata();

        await Clients.Clients(result.Value.PlayerOrGuestConnectionIds).SendAsync(WebClientMethods.EndGame);
        // In self-hosted mode, PlayFab economy operations are not needed

        return HubResponse.Success();
    }

    public async Task<HubResponse> RemoveRoom(RemoveRoomMessage message)
    {
        var result = roomAppService.RemoveRoom(new RemoveRoomCommand
        {
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        if (!string.IsNullOrWhiteSpace(result.Value.ActiveGameId))
        {
            var purgeGameResult = gameAppService.PurgeGame(new PurgeGameCommand { GameId = result.Value.ActiveGameId });
            if (purgeGameResult.IsFailure) return HubResponse.Error(purgeGameResult.Error.Message);
        }

        await Clients.Clients(result.Value.GuestConnectionIds).SendAsync(WebClientMethods.RemoveRoom);

        return HubResponse.Success();
    }

    public async Task<HubResponse> PushEvents(PushEventsMessage message)
    {
        var result = roomAppService.ValidateEventPush(new ValidateEventPushCommand
        {
            RoomCode = message.RoomCode,
            ConnectionId = Context.ConnectionId
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        foreach (var @event in message.Events)
            if (@event.PayloadType.Equals(nameof(PurchasedItemPayload)))
            {
                var payload = @event.GetPayloadAs<PurchasedItemPayload>();
                payload.PlayFabId = result.Value.PlayFabId;
                @event.UpdatePayload(payload);
            }

        return HubResponse.Success();
    }

    public async Task<HubResponse> KickGuests(KickGuestsMessage message)
    {
        var result = roomAppService.KickGuests(new KickGuestsCommand
        {
            GuestIds = message.GuestIds,
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        await Clients.Clients(result.Value.GuestConnectionIds).SendAsync(WebClientMethods.Kick);

        return HubResponse.Success();
    }

    public async Task<HubResponse> KickPlayer(KickPlayerMessage message)
    {
        var result = gameAppService.KickPlayer(new KickPlayerCommand
        {
            PlayerId = message.PlayerId,
            HostConnectionId = Context.ConnectionId,
            RoomCode = message.RoomCode
        });

        if (result.IsFailure) return HubResponse.Error(result.Error.Message);

        await Clients.Client(result.Value.PlayerConnectionId).SendAsync(WebClientMethods.Kick);

        return HubResponse.Success();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var guestResult = roomAppService.TryDisconnectGuest(new TryDisconnectGuestCommand
        {
            ConnectionId = Context.ConnectionId
        });
        if (guestResult.IsSuccess)
        {
            if (!string.IsNullOrWhiteSpace(guestResult.Value.ActiveGameId))
                gameAppService.DisconnectPlayer(new DisconnectPlayerCommand
                {
                    GameId = guestResult.Value.ActiveGameId,
                    PlayerId = guestResult.Value.GuestId
                });

            Clients.Client(guestResult.Value.HostConnectionId).SendAsync(HostMethods.PlayerDisconnected, guestResult.Value.GuestId);

            return base.OnDisconnectedAsync(exception);
        }

        var hostResult = roomAppService.TryDisconnectHost(new TryDisconnectHostCommand
        {
            ConnectionId = Context.ConnectionId
        });
        if (hostResult.IsSuccess)
        {
            Clients.Clients(hostResult.Value.GuestConnectionIds).SendAsync(WebClientMethods.HostDisconnected);

            connectionMonitoringService.SchedulePurge(
                TimeSpan.FromMinutes(10),
                hostResult.Value.RoomCode,
                roomAppService,
                gameAppService);
        }

        return base.OnDisconnectedAsync(exception);
    }
}