let modal = document.getElementById("requestInfoModal");

function contactForMoreInfo() {
    modal.style.display = "flex";
}

let closeIcon = document.getElementsByClassName("close")[0];
closeIcon.onclick = function () {
    modal.style.display = "none";
}