const form = document.getElementById('login-form');
const emailField = document.getElementById('email');
const passwordField = document.getElementById('password');
const loginPopup = document.getElementById('login-message'); // matchet deze classnaam met de readme?

form.onsubmit = async function (event) {
    event.preventDefault();
    hideErrorMessage();
    form.querySelector('button').disabled = true;

    const email = emailField.value;
    const password = passwordField.value;

    const isLoggedIn = await AdminRequest.login(email, password);

    if (isLoggedIn.success === true) {
        sessionStorage.setItem('SessionKey', isLoggedIn.data.SessionKey);
        window.location.href = 'VillaList.html';
    } else {
        showErrorMessage(isLoggedIn.data.Reason);
    }
};

function hideErrorMessage() {
    form.querySelector('button').disabled = true;
    loginPopup.style.display = 'none';
    loginPopup.textContent = '';
}

function showErrorMessage(message) {
    emailField.disabled = false;
    passwordField.disabled = false;
    form.querySelector('button').disabled = false;

    loginPopup.textContent = message;
    loginPopup.style.display = 'block';
}

hideErrorMessage();

async function checkLogin() {
    const isLoggedIn = await AdminRequest.IsLoggedIn();
    
    if (isLoggedIn.success === true) { // kan (isLoggedIn.success) niet gewoon?
        window.location.href = 'VillaList.html';
    }
}

checkLogin();