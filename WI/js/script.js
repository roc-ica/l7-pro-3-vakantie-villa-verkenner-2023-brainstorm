const MobileDropDown = document.getElementById("mobileDropDown");
const MobileDropDownButton = document.getElementById("mobileDropDownButton");
const adminButton = document.getElementById("adminIcon");


function toggleDropdown() {
    MobileDropDown.classList.toggle("show");
    MobileDropDownButton.classList.toggle("show");
}

document.onclick = function (event) {
    // Close the dropdown if the user clicks outside of it
    if (!MobileDropDownButton.contains(event.target) && !MobileDropDown.contains(event.target)) {
        MobileDropDown.classList.remove("show");
        MobileDropDownButton.classList.remove("show");
    }
}

adminButton.addEventListener("click", function () {
    window.location.href = "Admin/AdminLogin.html";
});