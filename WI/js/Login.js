const form = document.getElementById('login-form');
const emailField = document.getElementById('email');
const passwordField = document.getElementById('password');
form.onsubmit = async function(event) {
    event.preventDefault();

    const email = emailField.value;
    const password = passwordField.value;

    // if (!validateEmail(email)){
    //     alert('Voer een geldig e-mailadres in')
    //     return;
    // }

    if (password.length < 6){
        alert('Wachtwoord moet minimaal 8 tekens bevatten')
        return;
    }

    const IsLoggedInSuccess = await LoginRequest.login(email, password)
    if (IsLoggedInSuccess){
        alert('Inloggen gelukt')
    }else{
        alert('Inloggen mislukt')
    }
 
}