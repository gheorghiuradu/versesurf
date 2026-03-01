using CSharpFunctionalExtensions;
using GamePlaying.Application.Commands;
using GamePlaying.Application.Dto;
using GamePlaying.Domain;
using GamePlaying.Domain.GameAggregate;
using GamePlaying.Domain.RoomAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Action = GamePlaying.Domain.GameAggregate.Action;

namespace GamePlaying.Application
{
    public class GameAppService
    {
        private readonly IRoomRepository roomRepository;
        private readonly IGameRepository gameRepository;

        public GameAppService(
            IRoomRepository roomRepository,
            IGameRepository gameRepository)
        {
            this.roomRepository = roomRepository;
            this.gameRepository = gameRepository;
        }

        public async Task<Result<NewGameStartedDto, Error>> StartNewGameAsync(StartNewGameCommand startNewGameCommand)
        {
            // TODO: implement logic
            var codeResult = Code.Create(startNewGameCommand.RoomCode);
            if (codeResult.IsFailure)
            {
                // TODO: log
                return Result.Failure<NewGameStartedDto, Error>(codeResult.Error);
            }

            var room = this.roomRepository.GetRoom(codeResult.Value);
            if (room == null)
            {
                //TODO: log
                return Result.Failure<NewGameStartedDto, Error>(Errors.Room.NotFound());
            }

            if (!room.Organizer.ConnectionId.Equals(startNewGameCommand.HostConnectionId))
            {
                // TODO: log
                return Result.Failure<NewGameStartedDto, Error>(Errors.Room.ForbiddenAction());
            }

            var players = new HashSet<Player>();
            foreach (var guest in room.Guests)
            {
                var playerResult = Player.Create(guest.Id, guest.ConnectionId);
                if (playerResult.IsFailure)
                {
                    return Result.Failure<NewGameStartedDto, Error>(playerResult.Error);
                }

                players.Add(playerResult.Value);
            }

            var gameResult = Game.Create(room.Id, players);
            if (gameResult.IsFailure)
            {
                // TODO: log
                return Result.Failure<NewGameStartedDto, Error>(gameResult.Error);
            }

            var playlists = await startNewGameCommand.GetPlaylistsAsyncTask();

            var game = gameResult.Value;
            room.SetCurrentGame(game.Id, game.GuestUpdatedObserver);

            this.roomRepository.UpdateRoom(room);
            this.gameRepository.AddGame(game);

            return new NewGameStartedDto
            {
                GameId = game.Id,
                Playlists = playlists
            };
        }

        public Result<RejoinPlayerResultDto, Error> RejoinPlayer(RejoinPlayerCommand rejoinPlayerCommand)
        {
            var codeResult = Code.Create(rejoinPlayerCommand.RoomCode);
            if (codeResult.IsFailure)
            {
                // TODO: log
                return Result.Failure<RejoinPlayerResultDto, Error>(codeResult.Error);
            }

            var room = this.roomRepository.GetRoom(codeResult.Value);
            if (room == null)
            {
                //TODO: log
                return Result.Failure<RejoinPlayerResultDto, Error>(Errors.Room.NotFound());
            }

            if (string.IsNullOrWhiteSpace(room.GameId))
            {
                return Result.Failure<RejoinPlayerResultDto, Error>(Errors.Game.NotFound());
            }

            var game = this.gameRepository.GetGame(room.GameId);
            if (game == null)
            {
                var defaultAction = Action.CreateDefault();
                return new RejoinPlayerResultDto
                {
                    LatestActionName = defaultAction.Name,
                    LatestActionParam = defaultAction.Param,
                    Rejoined = false
                };
            }

            var player = game.Players.First(p => p.Id.Equals(rejoinPlayerCommand.PlayerId));

            player.Connect();
            var latestActiveAction = player.GetLatestActiveAction();

            // Get latest active player action

            return new RejoinPlayerResultDto
            {
                LatestActionName = latestActiveAction.Name,
                LatestActionParam = latestActiveAction.Param,
                Rejoined = true
            };
        }

        public Result<VotePlaylistResultDto, Error> RecordPlayersVote(RecordVoteCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                return Result.Failure<VotePlaylistResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<VotePlaylistResultDto, Error>(gameResult.Error);
            }

            var game = gameResult.Value;

            var playerResult = this.ValidateAndGetPlayer(command.PlayerId, command.ConnectionId, game);
            if (playerResult.IsFailure)
            {
                return Result.Failure<VotePlaylistResultDto, Error>(playerResult.Error);
            }

            playerResult.Value.SupersedeActions();
            this.gameRepository.UpdateGame(game);

            var result = new VotePlaylistResultDto { HostConnectionId = roomResult.Value.Organizer.ConnectionId };

            return result;
        }

        public async Task<Result<GetFullPlaylistResultDto, Error>> GetFullPlaylistAsync(GetFullPlaylistCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode, command.HostConnectionId);
            if (roomResult.IsFailure)
            {
                return Result.Failure<GetFullPlaylistResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<GetFullPlaylistResultDto, Error>(gameResult.Error);
            }

            // If not vip or requested random, get random playlist
            var getRandomPlaylist = command.RandomRequested || !roomResult.Value.Organizer.HasPerk(VipPerk.SelectPlaylist);
            var fullPlaylist = getRandomPlaylist ? await command.GetRandomPlaylistAsyncTask() : await command.GetPlaylistAsyncTask();

            var playlistResult = Playlist.Create(
                fullPlaylist.Id,
                fullPlaylist.Name,
                fullPlaylist.PictureUrl,
                fullPlaylist.Featured);

            if (playlistResult.IsFailure)
            {
                return Result.Failure<GetFullPlaylistResultDto, Error>(playlistResult.Error);
            }

            var playlist = playlistResult.Value;
            gameResult.Value.SetPlaylist(playlist);

            this.gameRepository.UpdateGame(gameResult.Value);

            var result = new GetFullPlaylistResultDto
            {
                FullPlaylist = fullPlaylist
            };

            return result;
        }

        public Result<AskResultDto, Error> Ask(AskCommand command)
        {
            // TODO: store songId in memory - somewhere
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                return Result.Failure<AskResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<AskResultDto, Error>(gameResult.Error);
            }

            foreach (var player in gameResult.Value.Players)
            {
                player.SupersedeActions();
                var actionResult = Action.Create(ActionName.Ask);
                if (actionResult.IsFailure)
                {
                    return Result.Failure<AskResultDto, Error>(actionResult.Error);
                }

                player.Actions.Add(actionResult.Value);
            }
            gameResult.Value.SetCorrectAnswerText(command.CorrectAnswer);

            this.gameRepository.UpdateGame(gameResult.Value);
            var result = new AskResultDto
            {
                PlayerConnectionIds = gameResult.Value.Players.Where(p => p.IsConnected).Select(p => p.ConnectionId).ToList()
            };

            return result;
        }

        public Result<AnswerResultDto, Error> Answer(AnswerCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                return Result.Failure<AnswerResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<AnswerResultDto, Error>(gameResult.Error);
            }

            var playerResult = this.ValidateAndGetPlayer(command.PlayerId, command.ConnectionId, gameResult.Value);
            if (playerResult.IsFailure)
            {
                return Result.Failure<AnswerResultDto, Error>(playerResult.Error);
            }

            var processedAnswerText = command.AnswerText.Trim();
            if (string.Equals(processedAnswerText, gameResult.Value.CorrectAnswerText, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<AnswerResultDto, Error>(Errors.Answer.NotFake());
            }

            var guest = roomResult.Value.Guests.FirstOrDefault(g => g.Id.Equals(command.PlayerId));

            playerResult.Value.SupersedeActions();
            this.gameRepository.UpdateGame(gameResult.Value);

            var result = new AnswerResultDto
            {
                HostConnectionId = roomResult.Value.Organizer.ConnectionId,
                Answer = new AnswerDto
                {
                    Id = Guid.NewGuid().ToString(),
                    IsAutoGenerated = false,
                    Name = processedAnswerText,
                    Player = this.GetPlayerDtoFromGuest(guest, command.RoomCode)
                }
            };

            return result;
        }

        public async Task<Result<GetMissingAnswersResultDto, Error>> GetMissingAnswersAsync(GetMissingAnswersCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                return Result.Failure<GetMissingAnswersResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<GetMissingAnswersResultDto, Error>(gameResult.Error);
            }

            var answers = new List<AnswerDto>();
            var playerConnectionIds = new List<string>();
            var answerTexts = await command.GetAnswerTextsTask();

            for (int i = 0; i < command.PlayerIds.Count(); i++)
            {
                var playerId = command.PlayerIds.ElementAt(i);
                var playerResult = this.ValidateAndGetPlayer(playerId, gameResult.Value);
                if (playerResult.IsFailure)
                {
                    return Result.Failure<GetMissingAnswersResultDto, Error>(playerResult.Error);
                }
                var guest = roomResult.Value.Guests.FirstOrDefault(g => g.Id.Equals(playerId));

                answers.Add(new AnswerDto
                {
                    Id = playerId,
                    IsAutoGenerated = true,
                    Name = answerTexts.ElementAt(i),
                    Player = this.GetPlayerDtoFromGuest(guest, command.RoomCode)
                });
                playerResult.Value.SupersedeActions();

                if (guest.IsConnected)
                {
                    playerConnectionIds.Add(guest.ConnectionId);
                }
            }

            this.gameRepository.UpdateGame(gameResult.Value);

            var result = new GetMissingAnswersResultDto
            {
                Answers = answers,
                PlayerConnectionIds = playerConnectionIds
            };

            return result;
        }

        public Result<StartVotingResultDto, Error> StartVoting(StartVotingCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                return Result.Failure<StartVotingResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<StartVotingResultDto, Error>(gameResult.Error);
            }

            var result = new StartVotingResultDto
            {
                PlayerAnswerPairs = new Dictionary<string, IEnumerable<AnswerDto>>()
            };

            foreach (var player in gameResult.Value.Players)
            {
                var answers = this.GetAnswersForVote(command.Answers, player.Id);
                var actionResult = Action.Create(ActionName.StartVoting, answers);
                if (actionResult.IsFailure)
                {
                    return Result.Failure<StartVotingResultDto, Error>(actionResult.Error);
                }

                player.SupersedeActions();
                player.Actions.Add(actionResult.Value);

                if (player.IsConnected)
                {
                    result.PlayerAnswerPairs.Add(player.ConnectionId, answers);
                }
            }

            this.gameRepository.UpdateGame(gameResult.Value);

            return result;
        }

        public Result<RelaxResultDto, Error> Relax(RelaxCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                return Result.Failure<RelaxResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<RelaxResultDto, Error>(gameResult.Error);
            }

            var connectionIds = new List<string>();
            foreach (var playerId in command.PlayerOrGuestIds)
            {
                var playerResult = this.ValidateAndGetPlayer(playerId, gameResult.Value);
                if (playerResult.IsFailure)
                {
                    return Result.Failure<RelaxResultDto, Error>(playerResult.Error);
                }

                var actionResult = Action.Create(ActionName.Relax);
                if (actionResult.IsFailure)
                {
                    return Result.Failure<RelaxResultDto, Error>(actionResult.Error);
                }

                playerResult.Value.SupersedeActions();
                playerResult.Value.Actions.Add(actionResult.Value);

                if (playerResult.Value.IsConnected)
                {
                    connectionIds.Add(playerResult.Value.ConnectionId);
                }
            }

            this.gameRepository.UpdateGame(gameResult.Value);

            return new RelaxResultDto
            {
                PlayerOrGuestConnectionIds = connectionIds
            };
        }

        public Result<AnswerSpeedResultDto, Error> AnswerSpeed(AnswerSpeedCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                return Result.Failure<AnswerSpeedResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<AnswerSpeedResultDto, Error>(gameResult.Error);
            }

            var playerResult = this.ValidateAndGetPlayer(command.PlayerId, command.ConnectionId, gameResult.Value);
            if (playerResult.IsFailure)
            {
                return Result.Failure<AnswerSpeedResultDto, Error>(playerResult.Error);
            }

            var processedAnswerText = command.AnswerText.Trim();
            if (string.Equals(processedAnswerText, gameResult.Value.CorrectAnswerText, StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<AnswerSpeedResultDto, Error>(Errors.Answer.NotFake());
            }

            var guest = roomResult.Value.Guests.FirstOrDefault(g => g.Id.Equals(command.PlayerId));

            playerResult.Value.SupersedeActions();
            this.gameRepository.UpdateGame(gameResult.Value);

            return new AnswerSpeedResultDto
            {
                HostConnectionId = roomResult.Value.Organizer.ConnectionId,
                Answer = new AnswerSpeedDto
                {
                    Id = Guid.NewGuid().ToString(),
                    ReceivedAtUTC = DateTime.UtcNow,
                    Name = processedAnswerText,
                    Player = this.GetPlayerDtoFromGuest(guest, command.RoomCode)
                }
            };
        }

        public Result<AskSpeedResultDto, Error> AskSpeed(AskSpeedCommand command)
        {
            // TODO: store songId in memory - somewhere
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                return Result.Failure<AskSpeedResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<AskSpeedResultDto, Error>(gameResult.Error);
            }

            foreach (var player in gameResult.Value.Players)
            {
                player.SupersedeActions();
                var actionResult = Action.Create(ActionName.AskSpeed);
                if (actionResult.IsFailure)
                {
                    return Result.Failure<AskSpeedResultDto, Error>(actionResult.Error);
                }

                player.Actions.Add(actionResult.Value);
            }

            this.gameRepository.UpdateGame(gameResult.Value);

            return new AskSpeedResultDto
            {
                PlayerConnectionIds = gameResult.Value.Players.Where(p => p.IsConnected).Select(p => p.ConnectionId).ToList()
            };
        }

        public Result<QuitGameResultDto, Error> QuitGame(QuitGameCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode, command.HostConnectionId);
            if (roomResult.IsFailure)
            {
                return Result.Failure<QuitGameResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<QuitGameResultDto, Error>(gameResult.Error);
            }

            gameResult.Value.SupersedePlayerAction();
            this.gameRepository.RemoveGame(gameResult.Value.Id);

            roomResult.Value.EndCurrentGame();
            this.roomRepository.UpdateRoom(roomResult.Value);

            return new QuitGameResultDto
            {
                GameId = roomResult.Value.GameId,
                PlayerConnectionIds = gameResult.Value.Players.Where(p => p.IsConnected).Select(p => p.ConnectionId)
                .ToList()
            };
        }

        public Result<EndGameResultDto, Error> EndGame(EndGameCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode, command.HostConnectionId);
            if (roomResult.IsFailure)
            {
                return Result.Failure<EndGameResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<EndGameResultDto, Error>(gameResult.Error);
            }

            gameResult.Value.SupersedePlayerAction();
            this.gameRepository.RemoveGame(gameResult.Value.Id);

            roomResult.Value.EndCurrentGame();
            this.roomRepository.UpdateRoom(roomResult.Value);

            var result = new EndGameResultDto
            {
                PlayerOrGuestConnectionIds = gameResult.Value.Players.Where(p => p.IsConnected).Select(p => p.ConnectionId)
                .ToList(),
                InventoryItemId = roomResult.Value.Organizer.CurrentInventoryItemId,
                PlayFabId = roomResult.Value.Organizer.PlayfabId,
                GameId = gameResult.Value.Id
            };

            return result;
        }

        public Result<DisconnectPlayerResultDto, Error> DisconnectPlayer(DisconnectPlayerCommand command)
        {
            var gameResult = this.ValidateAndGetGame(command.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<DisconnectPlayerResultDto, Error>(gameResult.Error);
            }

            var playerResult = this.ValidateAndGetPlayer(command.PlayerId, gameResult.Value);
            if (playerResult.IsFailure)
            {
                return Result.Failure<DisconnectPlayerResultDto, Error>(playerResult.Error);
            }

            playerResult.Value.Disconnect();
            this.gameRepository.UpdateGame(gameResult.Value);

            return new DisconnectPlayerResultDto();
        }

        public Result<PurgeGameResultDto, Error> PurgeGame(PurgeGameCommand command)
        {
            var gameResult = this.ValidateAndGetGame(command.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<PurgeGameResultDto, Error>(gameResult.Error);
            }

            this.gameRepository.RemoveGame(gameResult.Value.Id);

            return new PurgeGameResultDto();
        }

        public Result<KickPlayerResultDto, Error> KickPlayer(KickPlayerCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode, command.HostConnectionId);
            if (roomResult.IsFailure)
            {
                return Result.Failure<KickPlayerResultDto, Error>(roomResult.Error);
            }

            var gameResult = this.ValidateAndGetGame(roomResult.Value.GameId);
            if (gameResult.IsFailure)
            {
                return Result.Failure<KickPlayerResultDto, Error>(gameResult.Error);
            }

            var playerResult = this.ValidateAndGetPlayer(command.PlayerId, gameResult.Value);
            if (playerResult.IsFailure)
            {
                return Result.Failure<KickPlayerResultDto, Error>(playerResult.Error);
            }

            gameResult.Value.RemovePlayer(playerResult.Value.Id);
            roomResult.Value.RemoveGuest(playerResult.Value.Id);

            return new KickPlayerResultDto { PlayerConnectionId = playerResult.Value.ConnectionId };
        }

        private IEnumerable<AnswerDto> GetAnswersForVote(IEnumerable<AnswerDto> allAnswers, string playerId)
        {
            var playersAnswer = allAnswers.FirstOrDefault(a => string.Equals(a.Player?.Id, playerId));

            var sameAnswers = allAnswers
            .Where(a => string.Equals(a.Name, playersAnswer.Name, StringComparison.InvariantCultureIgnoreCase));

            return allAnswers.Except(sameAnswers).Distinct();
        }

        private Result<Room, Error> ValidateAndGetRoom(
            string roomCode,
            string hostConnectionId = null)
        {
            var codeResult = Code.Create(roomCode);
            if (codeResult.IsFailure)
            {
                // TODO: log
                return Result.Failure<Room, Error>(codeResult.Error);
            }

            var room = this.roomRepository.GetRoom(codeResult.Value);
            if (room == null)
            {
                //TODO: log
                return Result.Failure<Room, Error>(Errors.Room.NotFound());
            }

            if (!string.IsNullOrWhiteSpace(hostConnectionId) && !room.Organizer.ConnectionId.Equals(hostConnectionId))
            {
                // TODO: log
                return Result.Failure<Room, Error>(Errors.Room.ForbiddenAction());
            }

            return room;
        }

        private Result<Game, Error> ValidateAndGetGame(string gameId)
        {
            var game = this.gameRepository.GetGame(gameId);
            if (game is null)
            {
                //TODO: log
                return Result.Failure<Game, Error>(Errors.Game.NotFound());
            }

            return game;
        }

        private Result<Player, Error> ValidateAndGetPlayer(string playerId, string connectionId, Game game)
        {
            var player = game.Players.FirstOrDefault(p => p.Id.Equals(playerId));
            if (player is null)
            {
                //TODO: log
                return Result.Failure<Player, Error>(Errors.Game.PlayerNotFound(playerId));
            }
            if (!player.ConnectionId.Equals(connectionId))
            {
                //TODO: log
                return Result.Failure<Player, Error>(Errors.Player.InvalidConnectionId());
            }

            return player;
        }

        private Result<Player, Error> ValidateAndGetPlayer(string playerId, Game game)
        {
            var player = game.Players.FirstOrDefault(p => p.Id.Equals(playerId));
            if (player is null)
            {
                //TODO: log
                return Result.Failure<Player, Error>(Errors.Game.PlayerNotFound(playerId));
            }

            return player;
        }

        private PlayerDto GetPlayerDtoFromGuest(Guest guest, string code)
        {
            return new PlayerDto
            {
                Code = code,
                ColorCode = guest.Color.Value,
                CharacterCode = guest.Character.Value,
                Id = guest.Id,
                IsConnected = guest.IsConnected,
                Nick = guest.Nick
            };
        }
    }
}