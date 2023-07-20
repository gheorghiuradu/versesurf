using CSharpFunctionalExtensions;
using GamePlaying.Application.Commands;
using GamePlaying.Application.Dto;
using GamePlaying.Domain;
using GamePlaying.Domain.Events;
using GamePlaying.Domain.RoomAggregate;
using GamePlaying.Domain.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamePlaying.Application
{
    public class RoomAppService : ISecurityService
    {
        private readonly IRoomRepository roomRepository;

        public RoomAppService(IRoomRepository roomRepository)
        {
            this.roomRepository = roomRepository;
        }

        public Result<RoomBookingDto, Error> BookRoom(BookRoomCommand bookRoomCommand)
        {
            var organizerResult = Organizer.Create(
                bookRoomCommand.HostConnectionId,
                bookRoomCommand.HostPlatform,
                bookRoomCommand.HostVersion,
                bookRoomCommand.OrganizerPlayfabId,
                bookRoomCommand.LocaleId);

            if (organizerResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<RoomBookingDto, Error>(organizerResult.Error);
            }

            var organizer = organizerResult.Value;

            var roomBookingService = new RoomBookingService(roomRepository);
            var roomCodeAvailabilityResult = roomBookingService.AskForAvailableRoomCode();

            if (roomCodeAvailabilityResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<RoomBookingDto, Error>(roomCodeAvailabilityResult.Error);
            }

            var gameSetupResult = GameSetup.Create(
                bookRoomCommand.GameSetupAvailableColors,
                bookRoomCommand.GameSetupAvailableCharacters);

            if (gameSetupResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<RoomBookingDto, Error>(gameSetupResult.Error);
            }

            var roomResult = Room.Create(
                code: roomCodeAvailabilityResult.Value,
                gameSetup: gameSetupResult.Value);

            if (roomResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<RoomBookingDto, Error>(roomResult.Error);
            }

            var availableRoom = roomResult.Value;
            organizer.BookRoom(availableRoom);
            this.roomRepository.AddRoom(availableRoom);

            return new RoomBookingDto
            {
                RoomCode = availableRoom.Code.Value
            };
        }

        public Result<CanRejoinResultDto, Error> CanRejoin(CanRejoinCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<CanRejoinResultDto, Error>(roomResult.Error);
            }

            var guestResult = this.ValidateAndGetGuest(command.GuestId, roomResult.Value);
            if (guestResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<CanRejoinResultDto, Error>(guestResult.Error);
            }

            var result = new CanRejoinResultDto
            {
                CanRejoin = !guestResult.Value.IsConnected,
                GameInProgress = roomResult.Value.IsGameInProgress()
            };

            return result;
        }

        public Result<GuestRegistrationOrRejoinDto, Error> RegisterOrRejoin(RegisterOrRejoinGuestCommand command)
        {
            var codeResult = Code.Create(command.RoomCode);
            if (codeResult.IsFailure)
            {
                // TODO: log
                return Result.Failure<GuestRegistrationOrRejoinDto, Error>(codeResult.Error);
            }

            var room = this.roomRepository.GetRoom(codeResult.Value);
            if (room == null)
            {
                // TODO: log
                return Result.Failure<GuestRegistrationOrRejoinDto, Error>(Errors.Room.NotFound());
            }

            Result<GuestRegistrationOrRejoinDto, Error> result;
            if (string.IsNullOrEmpty(command.GuestId) || !room.ContainsGuest(command.GuestId))
            {
                result = this.RegisterGuest(room, command.GuestNick, command.ConnectionId);
            }
            else
            {
                result = this.RejoinGuest(room, command.GuestId, command.GuestNick, command.ConnectionId);
            }

            return result;
        }

        private Result<GuestRegistrationOrRejoinDto, Error> RegisterGuest(Room room, string nick, string connectionId)
        {
            if (room.IsGameInProgress())
            {
                // TODO: log
                return Result.Failure<GuestRegistrationOrRejoinDto, Error>(Errors.Room.GameInProgress());
            }
            if (room.NickIsTaken(guestNick: nick))
            {
                // TODO log
                return Result.Failure<GuestRegistrationOrRejoinDto, Error>(Errors.Room.NickIsTaken());
            }
            if (room.IsFull())
            {
                // TODO: log
                return Result.Failure<GuestRegistrationOrRejoinDto, Error>(Errors.Room.IsFull());
            }

            var guest = room.RegisterGuest(nick, connectionId);
            this.roomRepository.UpdateRoom(room);

            return new GuestRegistrationOrRejoinDto
            {
                Player = this.GetPlayerDtoFromGuest(guest, room.Code.Value),
                HostConnectionId = room.Organizer.ConnectionId
            };
        }

        private Result<GuestRegistrationOrRejoinDto, Error> RejoinGuest(Room room, string guestId, string nick, string connectionId)
        {
            if (room.NickIsTaken(guestNick: nick, guestId: guestId))
            {
                // TODO log
                return Result.Failure<GuestRegistrationOrRejoinDto, Error>(Errors.Room.NickIsTaken());
            }

            var guest = room.Guests.FirstOrDefault(g => g.Id.Equals(guestId));
            if (guest == null)
            {
                // TODO: log
                return Result.Failure<GuestRegistrationOrRejoinDto, Error>(Errors.Room.GuestNotFound(guestId));
            }

            var oldConnectionId = guest.ConnectionId;

            guest.Nick = nick;
            guest.ConnectionId = connectionId;
            guest.Connect();

            room.NotifyGuestUpdated(new GuestUpdated
            {
                Id = guest.Id,
                ConnectionId = guest.ConnectionId
            });

            this.roomRepository.UpdateRoom(room);

            return new GuestRegistrationOrRejoinDto
            {
                HostConnectionId = room.Organizer.ConnectionId,
                Rejoined = true,
                Player = this.GetPlayerDtoFromGuest(guest, room.Code.Value),
                OldConnectionId = oldConnectionId
            };
        }

        public Result<object, Error> RejoinOrganizer(RejoinOrganizerCommand rejoinOrganizerCommand)
        {
            var codeResult = Code.Create(rejoinOrganizerCommand.RoomCode);
            if (codeResult.IsFailure)
            {
                // TODO: log
                return Result.Failure<object, Error>(codeResult.Error);
            }

            var room = this.roomRepository.GetRoom(codeResult.Value);
            if (room == null)
            {
                // TODO: log
                return Result.Failure<object, Error>(Errors.Room.NotFound());
            }

            room.Organizer.Rejoin(rejoinOrganizerCommand.ConnectionId);
            this.roomRepository.UpdateRoom(room);

            return Result.Ok();
        }

        public Result<RelaxResultDto, Error> Relax(RelaxCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<RelaxResultDto, Error>(roomResult.Error);
            }

            var connectionIds = new List<string>();

            foreach (var guestId in command.PlayerOrGuestIds)
            {
                var guestResult = this.ValidateAndGetGuest(guestId, roomResult.Value);
                if (guestResult.IsFailure)
                {
                    //TODO: log
                    return Result.Failure<RelaxResultDto, Error>(guestResult.Error);
                }
                if (guestResult.Value.IsConnected)
                {
                    connectionIds.Add(guestResult.Value.ConnectionId);
                }
            }

            var result = new RelaxResultDto
            {
                PlayerOrGuestConnectionIds = connectionIds
            };

            return result;
        }

        public Result<AnyActiveGameResultDto, Error> AnyActiveGame(AnyActiveGameCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<AnyActiveGameResultDto, Error>(roomResult.Error);
            }

            var result = new AnyActiveGameResultDto
            {
                AnyActiveGame = !string.IsNullOrWhiteSpace(roomResult.Value.GameId)
            };

            return result;
        }

        public Result<DisconnectGuestResultDto, Error> TryDisconnectGuest(TryDisconnectGuestCommand command)
        {
            var room = this.roomRepository.GetRoomByGuestConnectionId(command.ConnectionId);
            if (room is null)
            {
                return Result.Failure<DisconnectGuestResultDto, Error>(Errors.Room.NotFound());
            }

            var guest = room.Guests.FirstOrDefault(g => g.ConnectionId.Equals(command.ConnectionId));
            guest.Disconnect();
            this.roomRepository.UpdateRoom(room);

            var result = new DisconnectGuestResultDto
            {
                ActiveGameId = room.GameId,
                GuestId = guest.Id,
                HostConnectionId = room.Organizer.ConnectionId,
                RoomCode = room.Code.Value
            };

            return result;
        }

        public Result<DisconnectHostResultDto, Error> TryDisconnectHost(TryDisconnectHostCommand command)
        {
            var room = this.roomRepository.GetRoomByHostConnectionId(command.ConnectionId);
            if (room is null)
            {
                return Result.Failure<DisconnectHostResultDto, Error>(Errors.Room.NotFound());
            }

            room.Organizer.Disconnect();
            this.roomRepository.UpdateRoom(room);

            var result = new DisconnectHostResultDto
            {
                GuestConnectionIds = room.Guests.Where(g => g.IsConnected).Select(g => g.ConnectionId).ToList(),
                RoomCode = room.Code.Value
            };

            return result;
        }

        public Result<RemoveRoomResultDto, Error> RemoveRoom(RemoveRoomCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode, command.HostConnectionId);
            if (roomResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<RemoveRoomResultDto, Error>(roomResult.Error);
            }

            this.roomRepository.RemoveRoom(roomResult.Value.Code);

            var result = new RemoveRoomResultDto
            {
                GuestConnectionIds = roomResult.Value.Guests.Where(g => g.IsConnected).Select(g => g.ConnectionId).ToList(),
                ActiveGameId = roomResult.Value.GameId,
                PlayFabId = roomResult.Value.Organizer.PlayfabId
            };

            return result;
        }

        public Result<PurgeRoomResultDto, Error> PurgeRoom(PurgeRoomCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode);
            if (roomResult.IsFailure)
            {
                //TODO: log
                return Result.Failure<PurgeRoomResultDto, Error>(roomResult.Error);
            }

            if (roomResult.Value.Organizer.IsConnected)
            {
                return Result.Failure<PurgeRoomResultDto, Error>(new Error("room.host.connected", "Cannot purge room, host has reconnected"));
            }

            this.roomRepository.RemoveRoom(roomResult.Value.Code);

            var result = new PurgeRoomResultDto
            {
                ActiveGameId = roomResult.Value.GameId,
                GuestConnectionIds = roomResult.Value.Guests.Where(g => g.IsConnected).Select(g => g.ConnectionId).ToList()
            };

            return result;
        }

        public GetAllConnectedHostConnectionIdsResultDto GetAllConnectedHostConnectionIds()
            => new GetAllConnectedHostConnectionIdsResultDto
            {
                ConnectionIds = this.roomRepository.GetAllRooms()
                .Where(r => r.Organizer.IsConnected)
                .Select(r => r.Organizer.ConnectionId)
                .ToList()
            };

        private Result<Room, Error> ValidateAndGetRoom(string roomCode, string hostConnectionId = null)
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

        private Result<Guest, Error> ValidateAndGetGuest(string playerId, string connectionId, Room room)
        {
            var guest = room.Guests.FirstOrDefault(p => p.Id.Equals(playerId));
            if (guest is null)
            {
                //TODO: log
                return Result.Failure<Guest, Error>(Errors.Room.GuestNotFound(playerId));
            }
            if (!guest.ConnectionId.Equals(connectionId))
            {
                //TODO: log
                return Result.Failure<Guest, Error>(Errors.Player.InvalidConnectionId());
            }

            return guest;
        }

        private Result<Guest, Error> ValidateAndGetGuest(string playerId, Room room)
        {
            var guest = room.Guests.FirstOrDefault(p => p.Id.Equals(playerId));
            if (guest is null)
            {
                //TODO: log
                return Result.Failure<Guest, Error>(Errors.Room.GuestNotFound(playerId));
            }

            return guest;
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

        public bool AuthenticatesHost(string connectionId)
        {
            var room = this.roomRepository.GetRoomByHostConnectionId(connectionId);
            return room != null;
        }

        public bool AuthenticatesGuest(string connectionId)
        {
            var room = this.roomRepository.GetRoomByGuestConnectionId(connectionId);
            return room != null;
        }

        public Result<ValidateEventPushResultDto, Error> ValidateEventPush(ValidateEventPushCommand validateEventPushCommand)
        {
            var roomResult = this.ValidateAndGetRoom(validateEventPushCommand.RoomCode, validateEventPushCommand.ConnectionId);
            if (roomResult.IsFailure)
            {
                return Result.Failure<ValidateEventPushResultDto, Error>(roomResult.Error);
            }

            return new ValidateEventPushResultDto
            {
                PlayFabId = roomResult.Value.Organizer.PlayfabId
            };
        }

        public Result<KickGuestsResultDto, Error> KickGuests(KickGuestsCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode, command.HostConnectionId);
            if (roomResult.IsFailure)
            {
                return Result.Failure<KickGuestsResultDto, Error>(roomResult.Error);
            }

            try
            {
                var connectionIds = new List<string>();
                foreach (var guestId in command.GuestIds)
                {
                    var guestResult = this.ValidateAndGetGuest(guestId, roomResult.Value);
                    if (guestResult.IsFailure)
                    {
                        return Result.Failure<KickGuestsResultDto, Error>(guestResult.Error);
                    }
                    connectionIds.Add(guestResult.Value.ConnectionId);
                    roomResult.Value.TryRemoveGuest(guestId);
                }

                return new KickGuestsResultDto
                {
                    GuestConnectionIds = connectionIds
                };
            }
            catch (System.InvalidOperationException)
            {
                return Result.Failure<KickGuestsResultDto, Error>(Errors.Room.GameInProgress());
            }
        }

        public async ValueTask<Result<ActivateVipResultDto, Error>> ActivateVipAsync(ActivateVipCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode, command.HostConnectionId);
            if (roomResult.IsFailure)
            {
                return Result.Failure<ActivateVipResultDto, Error>(roomResult.Error);
            }

            var organizer = roomResult.Value.Organizer;
            var canActivate = await command.ActivateItemAsyncTask(organizer.PlayfabId, command.InventoryItemId);
            if (!canActivate)
            {
                return Result.Failure<ActivateVipResultDto, Error>(Errors.Organizer.InvalidItem());
            }

            organizer.SetInventoryItemId(command.InventoryItemId);
            organizer.AddVipPerks(command.DefaultVipPerks);
            this.roomRepository.UpdateRoom(roomResult.Value);

            return new ActivateVipResultDto();
        }

        public Result<ActivateVipPerkResultDto, Error> ActivateVipPerk(ActivateVipPerkCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode, command.HostConnectionId);
            if (roomResult.IsFailure)
            {
                return Result.Failure<ActivateVipPerkResultDto, Error>(roomResult.Error);
            }

            var organizer = roomResult.Value.Organizer;
            if (!organizer.IsVip)
            {
                return Result.Failure<ActivateVipPerkResultDto, Error>(Errors.Organizer.NotVip());
            }

            organizer.AddVipPerk(command.Perk);
            this.roomRepository.UpdateRoom(roomResult.Value);

            return new ActivateVipPerkResultDto();
        }

        public Result<DeactivateVipPerkResultDto, Error> DeactivateVipPerk(DeactivateVipPerkCommand command)
        {
            var roomResult = this.ValidateAndGetRoom(command.RoomCode, command.HostConnectionId);
            if (roomResult.IsFailure)
            {
                return Result.Failure<DeactivateVipPerkResultDto, Error>(roomResult.Error);
            }

            var organizer = roomResult.Value.Organizer;
            organizer.RemoveVipPerk(command.Perk);
            this.roomRepository.UpdateRoom(roomResult.Value);

            return new DeactivateVipPerkResultDto();
        }
    }
}