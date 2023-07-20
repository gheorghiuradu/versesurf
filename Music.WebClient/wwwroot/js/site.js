// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function BuildConnection(hubUrl) {
    return new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .configureLogging(signalR.LogLevel.None)
        .withAutomaticReconnect()
        .build();
}

async function TryConnectAsync() {
    try {
        await connection.start();
        console.log("connected");
    } catch (e) {
        console.log(e);
    }
}

async function JoinRoom(code, nick) {
    Loading(true);
    if (connection.state === MyConnectionState.Disconnected) {
        await TryConnectAsync();
    }

    var playerId;
    var playerJson = window.localStorage.getItem("player");
    if (playerJson) {
        playerId = JSON.parse(playerJson).Id;
    }

    var joinRequest = new JoinRoomRequest(playerId, code.toUpperCase(), nick);

    var response = await connection.invoke(ServerMethods.JoinRoom, joinRequest);
    if (response.isSuccess) {
        window.localStorage.setItem("player", response.dataJson);
        player = JSON.parse(response.dataJson);
    }
    Loading(false);

    return response;
}

async function LoadPlaylists(playlists) {
    Loading(true);
    ClearSpace();

    $("#playlists").empty();

    playlists.map(p => {
        if (p.featured) {
            p.name = "🌟 " + p.name;
        }

        $("#playlists").append(`
    <li id='${p.id}' class='list-group-item' data-keywords='${p.keyWords}'>
        <div class='cardd'>
            <a href='#' onclick='VotePlaylist("${p.id}");'>
                <span class='card-header'>
                    ${p.name}
                </span>
                <span>
                    <img src='${p.pictureUrl}' class='card-image album-art mx-auto d-block' loading='lazy' alt='${p.name}' onerror="OnImgError(this)">
                </span>
            </a>
        </div>
    </li>
`);
    });

    $("#playlists-container").removeClass("d-none");
    Loading(false);
}

async function VotePlaylist(id) {
    Loading(true);
    var vote = new PlaylistVote(id, $(`#${id}`).text().trim());

    await connection.invoke(ServerMethods.VotePlaylist, vote);

    $("#playlists-container").addClass("d-none");
    Loading(false);
}

async function SendAnswer(e) {
    e.preventDefault();
    Loading(true);

    var response = await connection.invoke(ServerMethods.Answer, new AnswerMessage($("#answer").val()));

    if (response.isSuccess) {
        $("#answer-error").addClass("d-none");
        ClearSpace();
    } else {
        $("#answer-error").removeClass("d-none").text(response.errorMessage);
    }
    Loading(false);
}

async function SendSpeedAnswer(e) {
    e.preventDefault();
    Loading(true);

    var response = await connection.invoke(ServerMethods.AnswerSpeed, new AnswerMessage($("#answer-speed").val()));

    if (response.isSuccess) {
        $("#answer-speed-error").addClass("d-none");
        ClearSpace();
    } else {
        $("#answer-speed-error").removeClass("d-none").text(response.errorMessage);
    }
    Loading(false);
}

function ClearSpace() {
    $("#content").children().addClass("d-none");
    $("#answers").empty();
    $("#playlists").empty();
}

function StartVoting(answersReceived) {
    ClearSpace();

    answersReceived.map(answer => {
        $("#answers").append(`
            <li id='${answer.id}' class='list-group-item'>
                <a href='#' onclick='Vote("${answer.id}")'>
                    <span class='lyric text-white text-center'>
                        ${answer.name}
                    </span>
                </a>
            </li>
        `);
    });

    $("#answers-container").removeClass("d-none");
}

async function Vote(id) {
    Loading(true);
    var vote = new AnswerVote(id, $(`#${id}`).text().trim());
    var response = await connection.invoke(ServerMethods.VoteAnswer, vote);
    if (response.isSuccess) {
        ClearSpace();
    } else {
        ShowError(response.errorMessage);
    }

    Loading(false);
}

function OnImgError(img) {
    img.src = genericAlbumUrl;
}

async function TryRejoinAsync() {
    ClearSpace();

    while (connection.state === MyConnectionState.Reconnecting) {
        await sleep(500);
    }

    var retryCount = 0;
    var isSuccess = false;
    var errorMessage;

    while (retryCount < 3 && !isSuccess && connection.state === MyConnectionState.Connected) {
        try {
            retryCount++;

            var response = await JoinRoom(player.Code, player.Nick);
            isSuccess = response.isSuccess;
            errorMessage = response.errorMessage;
            LoadPlayerProfile();
        } catch (e) {
            errorMessage = e;
            console.log(e);
            await sleep(2500);
        }
    }

    while (!reconnectingModalVisible) {
        await sleep(500);
    }
    $("#reconnecting-modal").modal("hide");

    if (!isSuccess && errorMessage.length > 0) {
        ShowError(errorMessage);
    }
}

function CancelReconnect() {
    connection.stop();
}

function BindToConnection() {
    connection.on("Ask", () => {
        ClearSpace();
        $("#ask").removeClass("d-none");
        $("#answer").val("");
    });

    connection.on("AskSpeed", () => {
        ClearSpace();
        $("#ask-speed").removeClass("d-none");
        $("#answer-speed").val("");
    });

    connection.on("StartVoting", answers => StartVoting(answers));

    connection.onreconnecting(() => {
        $("#reconnecting-modal").modal({
            backdrop: "static",
            keyboard: false
        })
    });
    connection.onreconnected(TryRejoinAsync);
    connection.onclose(() => {
        ClearSpace();
        $("#room-code").removeClass("d-none");
        $("#join-error").addClass("d-none");
        $("#reconnecting-modal").modal("hide");
    });

    connection.on("RemoveRoom", () => {
        localStorage.removeItem("player");
        ClearSpace();
        $("#room-code").removeClass("d-none");
        $("#join-error").addClass("d-none");
        connection.stop();
    });
    connection.on("DisplayPlaylists", playlists => LoadPlaylists(playlists));
    connection.on("NotifyAutogeneratedAnswer", () => {
        if (!localStorage.getItem("hide-autofill-answer-notification"))
            $("#autofill-answer-notification").modal();
    });

    connection.on("Relax", Relax);
    connection.on("EndGame", Relax);
    connection.on("Kick", connection.stop);
    connection.on("HostDisconnected", () => ShowError("The game is experiencing connection issues."));
}

async function CheckIfLeftGame() {
    var json = localStorage.getItem("player");
    if (json) {
        if (connection.state === MyConnectionState.Disconnected) {
            await TryConnectAsync();
        }
        var canRejoin = new CanRejoinResult();
        player = JSON.parse(json);
        var response = await connection.invoke(ServerMethods.CanRejoin, new CanRejoinMessage(player.Code, player.Id));

        if (response.isSuccess) {
            canRejoin = new CanRejoinResult(response.dataJson);
        }

        if (canRejoin.CanRejoin) {
            $("#reconnecting-modal").modal({
                backdrop: "static",
                keyboard: false
            });
            TryRejoinAsync();
        }
    }
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function LoadPlayerProfile() {
    $("#character").attr('src', characterPath + player.CharacterCode + '.png');
    $("#player-name").text(player.Nick);
}

function Relax() {
    ClearSpace();
    $("#relax").removeClass("d-none");
}

function SearchPlaylist() {
    var query = $("#search-playlist").val().toUpperCase();
    var li = $("#playlists > li");

    for (var i = 0; i < li.length; i++) {
        var match = li[i].textContent.trim().toUpperCase().indexOf(query) > -1 ||
            li[i].getAttribute("data-keywords").toUpperCase().indexOf(query) > -1;
        if (match) {
            $(li[i]).removeClass("d-none");
        }
        else {
            $(li[i]).addClass("d-none");
        }
    }
}

function ShowError(text) {
    $("#error-modal-text").text(text);
    $("#error-modal").modal();
}

function Loading(bool) {
    if (bool) {
        $("input, button").prop("disabled", true);
        $("#loading").removeClass("d-none");
    }
    else {
        $("#loading").addClass("d-none");
        $("input, button").prop("disabled", false);
    }
}