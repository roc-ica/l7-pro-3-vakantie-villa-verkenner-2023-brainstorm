async function checkLogin() {
    const IsLoggedIn = await AdminRequest.IsLoggedIn();
    if (IsLoggedIn.success === false) {
        window.location.href = 'AdminLogin.html';
    }
}
checkLogin();