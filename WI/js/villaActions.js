const modal = document.getElementById("requestInfoModal");
const emailInput = document.getElementById("email");
const messageInput = document.getElementById("message");
const errorMessage = document.getElementById("errorMessage");

//Error handler
let emailIsValid = true;
let messageIsValid = true;

function emailValidation() {
    let emailInputValue = emailInput.value;
    const isValidEmail = /\S+@\S+\.\S+/.test(emailInputValue);

    if (!isValidEmail) {
        errorMessage.innerHTML = "Ongeldige email.";
        emailIsValid = false;
    } else {
        errorMessage.innerHTML = "";
        emailIsValid = true;
    }
}

function messageValidation() {
    let messageInputValue = messageInput.value;

    if (messageInputValue === "") {
        errorMessage.innerHTML = "Vul in alle velden.";
        messageIsValid = false;
    } else {
        errorMessage.innerHTML = "";
        messageIsValid = true;
    }
}

function openMoreInfoRequestModal() {
    modal.style.display = "flex";
}

async function confirmRequest() {
    emailValidation();
    messageValidation();
    if (emailIsValid && messageIsValid) {
        await MoreInfoRequest.requestMoreInfo(id, emailInput.value, message.value);
        closeModal();
    }
}

function closeModal() {
    modal.style.display = "none";
    emailInput.value = "";
    message.value = "";
    errorMessage.innerHTML = "";
}