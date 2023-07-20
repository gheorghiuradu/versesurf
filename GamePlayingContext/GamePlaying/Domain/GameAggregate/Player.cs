using CSharpFunctionalExtensions;
using GamePlaying.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GamePlaying.Domain.GameAggregate
{
    public class Player : Entity
    {
        public string ConnectionId { get; set; }

        public bool IsConnected { get; private set; }

        public IList<Action> Actions { get; private set; } = new List<Action>();

        public Player(string id, string connectionId)
        {
            this.Id = id;
            this.ConnectionId = connectionId;
            this.IsConnected = true;
        }

        public static Result<Player, Error> Create(string id, string connectionId)
        {
            // Validate data
            var player = new Player(id, connectionId);

            // TODO: other code business rules

            return Result.Ok<Player, Error>(player);
        }

        public void SupersedeActions()
        {
            foreach (var action in this.Actions)
            {
                action.Supersede();
            }
        }

        public void Disconnect()
        {
            this.IsConnected = false;
        }

        public void Connect()
        {
            this.IsConnected = true;
        }

        public Action GetLatestActiveAction()
        {
            return this.Actions
                .OrderByDescending(a => a.TimeStamp)
                .FirstOrDefault(a => a.Status == ActionStatus.Active)
                ?? Action.CreateDefault();
        }
    }
}