using CSharpFunctionalExtensions;
using GamePlaying.Domain.RoomAggregate;
using System;

namespace GamePlaying.Domain.Services
{
    /// <summary>
    /// Domain service that encapsulates login for generating an available room code
    /// </summary>
    public class RoomBookingService
    {
        private IRoomRepository roomRepository;

        public RoomBookingService(IRoomRepository roomRepository)
        {
            this.roomRepository = roomRepository;
        }

        public Result<Code, Error> AskForAvailableRoomCode()
        {
            var tries = 10;
            while (tries-- > 0)
            {
                var codeCandidate = Guid.NewGuid().ToString().Substring(0, 5);
                var codeResult = Code.Create(codeCandidate);
                if (codeResult.IsFailure)
                {
                    // TODO: log
                    continue;
                }

                var code = codeResult.Value;

                var codeIsNotAvailable = !this.roomRepository.CodeIsAvailable(code);
                if (codeIsNotAvailable)
                {
                    // TODO: log
                    continue;
                }

                return codeResult;
            }

            return Result.Failure<Code, Error>(Errors.Room.CodeNotAvailable());
        }

        public void BookRoom(Organizer organizer, Room availableRoom)
        {
        }
    }
}