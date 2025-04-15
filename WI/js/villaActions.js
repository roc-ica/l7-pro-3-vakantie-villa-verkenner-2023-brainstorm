const modal = document.getElementById("requestInfoModal");
const emailInput = document.getElementById("email");
const messageInput = document.getElementById("message");
const errorMessage = document.getElementById("errorMessage");

//Error handler
let isEmailValid = true;
let isMessageValid = true;

function emailValidation() {
    let emailInputValue = emailInput.value;
    const isValidEmail = /\S+@\S+\.\S+/.test(emailInputValue);

    if (!isValidEmail) {
        errorMessage.innerHTML = "Ongeldige email.";
        isEmailValid = false;
    } else {
        errorMessage.innerHTML = "";
        isEmailValid = true;
    }
}

function messageValidation() {
    let messageInputValue = messageInput.value;

    if (messageInputValue === "") {
        errorMessage.innerHTML = "Vul in alle velden.";
        isMessageValid = false;
    } else {
        errorMessage.innerHTML = "";
        isMessageValid = true;
    }
}

function openMoreInfoRequestModal() {
    modal.style.display = "flex";
}

async function confirmRequest() {
    emailValidation();
    messageValidation();
    
    if (isEmailValid && isMessageValid) {
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