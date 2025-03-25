
const form = document.getElementById('login-form');
const emailField = document.getElementById('email');
const passwordField = document.getElementById('password');
const Login_Popup = document.getElementById('login-message'); // Meldingsdiv

form.onsubmit = async function (event) {
    event.preventDefault();


    const email = emailField.value;
    const password = passwordField.value;

    if (password.length < 8) {
        showErrorMessage('Wachtwoord moet minimaal 8 tekens bevatten');
        return;
    }

    const IsLoggedIn = await LoginRequest.login(email, password);

    if (IsLoggedIn.success === true) {
        sessionStorage.setItem('SessionKey', IsLoggedIn.data.SessionKey);
        window.location.href = 'VillaList.html';
    } else {
        showErrorMessage(IsLoggedIn.data.Reason);
    }
};

function showErrorMessage(message) {
    emailField.disabled = true;
    passwordField.disabled = true;
    form.querySelector('button').disabled = true;

    Login_Popup.textContent = message;
    Login_Popup.style.display = 'block';
    Login_Popup.classList.add('error-message');
}
