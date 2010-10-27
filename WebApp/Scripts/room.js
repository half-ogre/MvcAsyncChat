var since = "",
    errorCount = 0,
    MAX_ERRORS = 6;

function addMessage(message, type) {
    $("#messagesSection > td").append("<div class='" + (type || "") + "'>" + message + "</div>")
}

function showError(error) {
    addMessage(error.toString(), "error");
}

function onSayFailed(XMLHttpRequest, textStatus, errorThrown) {
    showError("An unanticipated error occured during the say request: " + textStatus + "; " + errorThrown);
}

function onSay(data) {
    if (data.error) {
        showError("An error occurred while trying to say your message: " + data.error);
        return;
    }
}

function setSayHandler() {
    $("#Text").keypress(function (e) {
        if (e.keyCode == 13) {
            $("#sayForm").submit();
            $("#Text").val("");
            return false;
        }
    });
}

function retryGetMessages() {
    if (++errorCount > MAX_ERRORS) {
        showError("There have been too many errors. Please leave the chat room and re-enter.");
    }
    else {
        setTimeout(function () {
            getMessages();
        }, Math.pow(2, errorCount) * 1000);
    }
}

function onMessagesFailed(XMLHttpRequest, textStatus, errorThrown) {
    showError("An unanticipated error occured during the messages request: " + textStatus + "; " + errorThrown);
    retryGetMessages();
}

function onMessages(data, textStatus, XMLHttpRequest) {
    if (data.error) {
        showError("An error occurred while trying to get messages: " + data.error);
        retryGetMessages();
        return;
    }

    errorCount = 0;
    since = data.since;

    for (var n = 0; n < data.messages.length; n++)
        addMessage(data.messages[n]);

    setTimeout(function () {
        getMessages();
    }, 0);
}

function getMessages() {
    $.ajax({
        cache: false,
        type: "POST",
        dataType: "json",
        url: "/messages",
        data: { since: since },
        error: onMessagesFailed,
        success: onMessages,
        timeout: 100000
    });
}