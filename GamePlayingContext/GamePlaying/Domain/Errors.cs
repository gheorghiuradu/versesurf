namespace GamePlaying.Domain
{
    public static class Errors
    {
        public static class Player
        {
            public static Error InvalidConnectionId() =>
                new Error("player.connectionid.invalid", "The player's connection id is invalid. The player might be disconnected or someone tried to impersonate.");
        }

        public static class Game
        {
            public static Error DuplicateNicks() =>
                new Error("game.nicks.duplicate", "The game does not allow duplicated nicks.");

            public static Error NotFound()
                => new Error("game.not.found", "The game with the specified id could not be found.");

            public static Error PlayerNotFound(string id) =>
                new Error("game.player.not.found", $"Player with id {id} not found in the game.");
        }

        public static class Action
        {
            public static Error NullOrEmptyAction() =>
                new Error("action.name.null.or.empty", "The action name is null or empty.");
        }

        public static class Organizer
        {
            public static Error InvalidConnectionId() =>
                new Error("organizer.connection.invalid.format", "The organizer connection id is not valid.");

            public static Error InvalidPlatform() =>
                new Error("organizer.platform.invalid.format", "The organizer platform is not valid.");

            public static Error InvalidHostVersion() =>
               new Error("organizer.hostVersion.invalid.format", "The organizer version is not valid.");

            public static Error InvalidPlayfabId() =>
                new Error("organizer.playfabId.invalid.format", "The organizer playfabId is not valid.");

            public static Error InvalidItem() =>
                new Error("organizer.item.invalid", "Could not activate the item for Vip");

            public static Error NotVip() =>
                new Error("organizer.not.vip", "The action is forbidden because organizer is not vip.");
        }

        public static class Room
        {
            public static Error InvalidCode(string code) =>
                new Error("room.code.invalid.format", $"The room code '{code}' is not valid.");

            public static Error NullOrEmptyCode() =>
                new Error("room.code.null.or.empty", "The room code is null or empty.");

            public static Error NotFound() =>
                new Error("room.not.found", "The room with the specified code could not be found.");

            public static Error ForbiddenAction() =>
                new Error("room.forbidden", "Only the host can perform this action in this room.");

            public static Error CodeNotAvailable() =>
                new Error("room.code.not.available", "An available code could not be generated.");

            public static Error GuestNotFound(string guestId) =>
                new Error("room.guest.not.found", $"The specified guest with id {guestId} was not found in the room");

            public static Error IsFull() =>
                new Error("room.is.full", "There is no more space in the room.");

            public static Error NickIsTaken() =>
                new Error("room.nick.is.taken", "That nickname has already been taken by someone that previously joined this room.");

            public static Error GameInProgress() =>
                new Error("room.game.in.progress", "A game is already in progress.");
        }

        public static class GameSetup
        {
            public static Error InvalidNumberOfRounds(int roundsCount) =>
                new Error("game.rounds.invalid.number", $"The number of rounds '{roundsCount}' is not valid.");

            public static Error InvalidColor() =>
                new Error("game.color.invalid", $"The color is not valid.");

            public static Error InvalidEmoji() =>
                new Error("game.emoji.invalid", $"The emoji is not valid.");
        }

        public static class Answer
        {
            public static Error NotFake() =>
                new Error("answer.not.fake", "Oops, that's the correct lyric. Now write a fake one.");
        }

        // TODO: tech debt
        public static class General
        {
            public static Error NotFound(string entityName, string id) =>
                new Error("", $"'{entityName}' not found for Id '{id}'");

            public static Error CreateFailed(string entityName) =>
                new Error("", $"'{entityName}' could not be created");

            public static Error UpdateFailed(string entityName, string id) =>
                new Error("", $"'{entityName}' with Id '{id}' could not be updated");

            public static Error DeleteFailed(string entityName, string id) =>
                new Error("", $"'{entityName}' with Id '{id}' could not be deleted");
        }

        public static class Playlist
        {
            public static Error NotFound()
                => new Error("playlist.not.found", "The playlist with the specified id could not be found.");
        }
    }
}