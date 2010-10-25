function addMessage(message, type) {
    $("#messagesSection > td").append("<div class='" + (type || "") + "'>" + message + "</div>")
}

function showError(error) {
    addMessage(error.toString(), "error");
}

function onFailed(XMLHttpRequest, textStatus, errorThrown) {
    showError("An unanticipated error occured while calling the server: " + textStatus + "; " + errorThrown);
}

function onSay(result) {
    if (result.error) {
        showError("An error occurred while trying to say your message: " + result.error);
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