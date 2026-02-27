using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace GamePlaying.Domain.GameAggregate
{
    public class Action : ValueObject
    {
        public string Name { get; }
        public object Param { get; }
        public DateTime TimeStamp { get; }
        public ActionStatus Status { get; private set; }

        private Action(string name, object param)
        {
            this.Name = name;
            this.Param = param;
            this.TimeStamp = DateTime.Now;
            this.Status = ActionStatus.Active;
        }

        public static Result<Action, Error> Create(string name, object param = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<Action, Error>(Errors.Action.NullOrEmptyAction());
            }

            return Result.Ok<Action, Error>(new Action(name, param));
        }

        public static Action CreateDefault()
        {
            return new Action(ActionName.Relax, null);
        }

        public void Supersede()
        {
            this.Status = ActionStatus.Superseded;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Name;
            yield return this.Status;
            yield return this.TimeStamp;
        }
    }

    public enum ActionStatus
    {
        Active,
        Superseded
    }

    public static class ActionName
    {
        public const string DisplayPlaylists = nameof(DisplayPlaylists);
        public const string Ask = nameof(Ask);
        public const string StartVoting = nameof(StartVoting);
        public const string Relax = nameof(Relax);
        public const string AskSpeed = nameof(AskSpeed);
    }
}