const MobileDropDown = document.getElementById("mobileDropDown");
const MobileDropDownButton = document.getElementById("mobileDropDownButton");

function toggleDropdown() {
    MobileDropDown.classList.toggle("show");
    MobileDropDownButton.classList.toggle("show");
}

document.onclick = function (event) {
    // Close the dropdown if the user clicks outside of it
    if (!MobileDropDownButton.contains(event.target) && !event.target.matches("#mobileDropwDownButtonText") && !MobileDropDown.contains(event.target)) {
        MobileDropDown.classList.remove("show");
        MobileDropDownButton.classList.remove("show");
    }
}