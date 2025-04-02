const form = document.getElementById('login-form');
const emailField = document.getElementById('email');
const passwordField = document.getElementById('password');
const Login_Popup = document.getElementById('login-message');

form.onsubmit = async function (event) {
    event.preventDefault();
    hideErrorMessage();
    form.querySelector('button').disabled = true;

    const email = emailField.value;
    const password = passwordField.value;

    const IsLoggedIn = await AdminRequest.login(email, password);

    if (IsLoggedIn.success === true) {
        sessionStorage.setItem('SessionKey', IsLoggedIn.data.SessionKey);
        window.location.href = 'VillaList.html';
    } else {
        showErrorMessage(IsLoggedIn.data.Reason);
    }
};

function hideErrorMessage() {
    form.querySelector('button').disabled = true;
    Login_Popup.style.display = 'none';
    Login_Popup.textContent = '';
}

function showErrorMessage(message) {
    emailField.disabled = false;
    passwordField.disabled = false;
    form.querySelector('button').disabled = false;

    Login_Popup.textContent = message;
    Login_Popup.style.display = 'block';
}

hideErrorMessage();


async function checkLogin() {
    const IsLoggedIn = await AdminRequest.IsLoggedIn();
    if (IsLoggedIn.success === true) {
        window.location.href = 'VillaList.html';
    }
}
checkLogin();