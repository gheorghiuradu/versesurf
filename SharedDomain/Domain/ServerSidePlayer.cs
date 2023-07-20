using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedDomain
{
    public class ServerSidePlayer : Player
    {
        public string ConnectionId { get; set; }
        public List<PlayerAction> Actions { get; protected set; } = new List<PlayerAction>();

        public ServerSidePlayer Duplicate(string connectionId)
        {
            return new ServerSidePlayer
            {
                Id = this.Id,
                ConnectionId = connectionId,
                Nick = this.Nick,
                Code = this.Code,
                CharacterCode = this.CharacterCode,
                ColorCode = this.ColorCode,
                Actions = this.Actions,
                IsConnected = this.IsConnected
            };
        }

        public void AddAction(string name, object param = null)
        {
            this.Actions.Add(new PlayerAction(name, param));
        }

        public void Supersede()
        {
            foreach (var action in this.Actions.Where(a => a.Status != ActionStatus.Superseded))
            {
                action.Status = ActionStatus.Superseded;
            }
        }

        public PlayerAction GetRejoinAction()
        {
            return this.Actions
                .OrderByDescending(a => a.TimeStamp)
                .FirstOrDefault(a => a.Status == ActionStatus.Active);
        }

        public class PlayerAction
        {
            public string Name { get; }
            public object Param { get; }
            public DateTime TimeStamp { get; }
            public ActionStatus Status { get; set; }

            public PlayerAction(string name, object param = null)
            {
                this.Name = name;
                if (!(param is null))
                    this.Param = param;
                this.TimeStamp = DateTime.Now;
                this.Status = ActionStatus.Active;
            }
        }
    }

    public enum ActionStatus
    {
        Active,
        Superseded
    }
}