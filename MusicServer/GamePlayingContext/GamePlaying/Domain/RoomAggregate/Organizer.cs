using CSharpFunctionalExtensions;
using GamePlaying.Infrastructure;
using System.Collections.Generic;

namespace GamePlaying.Domain.RoomAggregate
{
    public class Organizer : Entity
    {
        internal Organizer(
            string connectionId,
            string hostPlatform,
            string hostVersion,
            string playfabId,
            string localeId)
        {
            this.ConnectionId = connectionId;
            this.HostPlatform = hostPlatform;
            this.HostVersion = hostVersion;
            this.PlayfabId = playfabId;
            this.LocaleId = localeId;
            this.IsConnected = true;
        }

        public virtual string ConnectionId { get; private set; }
        public string HostPlatform { get; }
        public string HostVersion { get; }
        public string PlayfabId { get; }
        public string LocaleId { get; }
        public Room BookedRoom { get; set; }
        public bool IsConnected { get; private set; }
        public HashSet<VipPerk> VipPerks { get; } = new HashSet<VipPerk>();
        public string CurrentInventoryItemId { get; private set; }
        public bool IsVip => this.VipPerks.Count > 0;

        public static Result<Organizer, Error> Create(
            string connectionId,
            string hostPlatform,
            string hostVersion,
            string playfabId,
            string localeId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return Result.Failure<Organizer, Error>(Errors.Organizer.InvalidConnectionId());
            }

            if (string.IsNullOrWhiteSpace(hostPlatform))
            {
                return Result.Failure<Organizer, Error>(Errors.Organizer.InvalidPlatform());
            }

            if (string.IsNullOrWhiteSpace(hostVersion))
            {
                return Result.Failure<Organizer, Error>(Errors.Organizer.InvalidHostVersion());
            }

            if (string.IsNullOrWhiteSpace(playfabId))
            {
                return Result.Failure<Organizer, Error>(Errors.Organizer.InvalidPlayfabId());
            }

            var newOrganizer = new Organizer(connectionId, hostPlatform, hostVersion, playfabId, localeId);

            // TODO: other code business rules

            return Result.Ok<Organizer, Error>(newOrganizer);
        }

        public void BookRoom(Room availableRoom)
        {
            this.BookedRoom = availableRoom;
            availableRoom.Organizer = this;
        }

        internal void Disconnect()
        {
            this.IsConnected = false;
        }

        internal void Rejoin(string connectionId)
        {
            this.ConnectionId = connectionId;
            this.IsConnected = true;
        }

        public void SetInventoryItemId(string inventoryItemId)
        {
            this.CurrentInventoryItemId = inventoryItemId;
        }

        public void AddVipPerk(VipPerk perk)
        {
            this.VipPerks.Add(perk);
        }

        public void AddVipPerks(IEnumerable<VipPerk> perks)
        {
            foreach (var perk in perks)
            {
                this.AddVipPerk(perk);
            }
        }

        public void RemoveVipPerk(VipPerk perk)
        {
            if (this.VipPerks.Contains(perk))
            {
                this.VipPerks.Remove(perk);
            }
        }

        public bool HasPerk(VipPerk perk) => this.VipPerks.Contains(perk);
    }
}