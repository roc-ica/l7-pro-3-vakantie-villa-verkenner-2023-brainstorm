let modal = document.getElementById("requestInfoModal");

function contactForMoreInfo() {
    modal.style.display = "flex";
}

let closeIcon = document.getElementsByClassName("close")[0];
closeIcon.onclick = function () {
    modal.style.display = "none";
}

let errorMessage = document.getElementById("errorMessage");
let emailInput = document.getElementById("email");
let message = document.getElementById("message");

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

message.onblur = function () {
    let emailInputValue = emailInput.value;
    const isValidEmail = /\S+@\S+\.\S+/.test(emailInputValue);

    if (!isValidEmail) {
        errorMessage.innerHTML = "Ongeldige email.";
    } else {
        errorMessage.innerHTML = "";
    }
}

async function requestMoreInfo() {
    await MoreInfoRequest.requestMoreInfo(id, emailInput.value, message.value);
    modal.style.display = "none";
    emailInput.value = "";
    message.value = "";
}