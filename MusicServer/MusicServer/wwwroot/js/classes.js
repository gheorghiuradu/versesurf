const ServerMethods = {
    CanRejoin: "CanRejoin",
    VotePlaylist: "VotePlaylist",
    JoinRoom: "JoinRoom",
    VoteAnswer: "VoteAnswer",
    Answer: "Answer",
    AnswerSpeed: "AnswerSpeed"
}

class JoinRoomRequest {
    constructor(guestId, roomCode, guestNick) {
        this.guestId = guestId;
        this.roomCode = roomCode;
        this.guestNick = guestNick;
    }
}

class CanRejoinMessage {
    constructor(roomCode, guestId) {
        this.guestId = guestId;
        this.roomCode = roomCode;
    }
}

const MyConnectionState = {
    Disconnected: "Disconnected",
    Connecting: "Connecting",
    Connected: "Connected",
    Disconnecting: "Disconnecting",
    Reconnecting: "Reconnecting"
}

class VoteItem {
    constructor(id, name) {
        this.Id = id;
        this.Name = name;
    }
}

class PlaylistVote {
    constructor(playlistId, playlistName) {
        this.By = player;
        this.Code = player.Code;
        this.Item = new VoteItem(playlistId, playlistName)
    }
}

class AnswerVote {
    constructor(answerId, answerName) {
        this.By = player;
        this.Code = player.Code;
        this.Item = new VoteItem(answerId, answerName);
    }
}

class Answer {
    constructor(answerText) {
        this.Player = player;
        this.Name = answerText;
    }
}

class AnswerMessage {
    constructor(answerText) {
        this.RoomCode = player.Code;
        this.Answer = new Answer(answerText);
    }
}

class CanRejoinResult {
    constructor(json) {
        if (json !== undefined) {
            var canReconnect = JSON.parse(json);
            this.GameInProgress = canReconnect.GameInProgress;
            this.CanRejoin = canReconnect.CanRejoin;
        } else {
            this.GameInProgress = false;
            this.CanRejoin = false;
        }
    }
}