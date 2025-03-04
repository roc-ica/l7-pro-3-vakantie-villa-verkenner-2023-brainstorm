function toggleDropdown() {
    document.getElementById("mobileDropDown").classList.toggle("show");
    document.getElementById("mobileDropDownButton").classList.toggle("show");
}
document.onclick = function (event) {
    // Close the dropdown if the user clicks outside of it
    if (!event.target.matches("#mobileDropDownButton") && !event.target.matches("#mobileDropwDownButtonText") && !event.target.matches(".mobileDropDownContent")) {
        document.getElementById("mobileDropDown").classList.remove("show");
        document.getElementById("mobileDropDownButton").classList.remove("show");
    }
}