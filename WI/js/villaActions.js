let modal = document.getElementById("requestInfoModal");
let emailInput = document.getElementById("email");
let messageInput = document.getElementById("message");
let errorMessage = document.getElementById("errorMessage");

//Error handler
emailInput.onblur = function () {
    let emailInputValue = emailInput.value;
    const isValidEmail = /\S+@\S+\.\S+/.test(emailInputValue);

    if (!isValidEmail) {
        errorMessage.innerHTML = "Ongeldige email.";
    } else {
        errorMessage.innerHTML = "";
    }
}

messageInput.onblur = function () {
    let messageInputValue = messageInput.value;

    if (messageInputValue === "") {
        errorMessage.innerHTML = "Vul in alle velden.";
    } else {
        errorMessage.innerHTML = "";
    }
}

function openMoreInfoRequestModal() {
    modal.style.display = "flex";
}

async function confirmRequest() {
    await MoreInfoRequest.requestMoreInfo(id, emailInput.value, message.value);
    closeModal();
}

function closeModal() {
    modal.style.display = "none";
    emailInput.value = "";
    message.value = "";
    errorMessage.innerHTML = "";
}