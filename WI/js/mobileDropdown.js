const mobileDropDown = document.getElementById("mobileDropDown");
const mobileDropDownButton = document.getElementById("mobileDropDownButton");
const adminButton = document.getElementById("adminIcon");

function toggleDropdown() {
    mobileDropDown.classList.toggle("show");
    mobileDropDownButton.classList.toggle("show");
}

document.onclick = function (event) {
    // Close the dropdown if the user clicks outside of it
    if (!mobileDropDownButton.contains(event.target) && !mobileDropDown.contains(event.target)) {
        mobileDropDown.classList.remove("show");
        mobileDropDownButton.classList.remove("show");
    }
}

adminButton.addEventListener("click", function () {
    window.location.href = "Admin/AdminLogin.html";
});