async function checkLogin() {
  const IsLoggedIn = await AdminRequest.IsLoggedIn();
  if (IsLoggedIn.success === false) {
    window.location.href = 'AdminLogin.html';
  }
}
checkLogin();

let data = [];

async function loadVillas() {
  data = await VillaRequests.getAdminVillas();
  const villas = JSON.parse(data.data.Villas);
  console.log(villas);
  const container = document.getElementById("villaContainer");
  let tempCSS = document.createElement("style");
  villas.forEach(villa => {
    container.innerHTML += `
        <div class="card">
          <img src="${villa.VillaImagePath || ''}" alt="${villa.Name}">
          <p class="description">${villa.description}</p>

          <div class="info">
            <h3>${villa.Name}</h3>
            <p>${villa.Location}</p>
            <div class="price">€${villa.Price},-</div>
          </div>
          <div class="actions">
            <small>ACTIES</small>
            <button class="btn btn-edit">Bewerken</button>
<button class="btn btn-request" id="count_${villa.VillaID}" onclick="openRequestModal(${villa.VillaID})">Request</button>
          </div>
          <button class="btn btn-delete">Verwijderen</button>
        </div>
      `;
    tempCSS.innerHTML += `
    #count_${villa.VillaID}::after {
      content: "${villa.Requests.length}";
    }`
  });
  document.head.appendChild(tempCSS);

}


async function openRequestModal(villaId) {
  const modal = document.getElementById("requestModal");
  const requestList = document.getElementById("requestList");
  requestList.innerHTML = "<p>Laden...</p>";

  try {
    const requests = JSON.parse(data.data.Villas).filter(villa => villa.VillaID === villaId)[0].Requests;
    requestList.innerHTML = "";
    console.log(requests);
    if (requests.length === 0) {
      requestList.innerHTML = "<p>Geen verzoeken gevonden.</p>";
    } else {
      requests.forEach(email => {
        console.log(email);
        requestList.innerHTML += `
            <div class="request-item">
              <span>${email.Email}</span>
              <span>
                <i class="fas fa-copy" style="cursor:pointer;" onclick="copyToClipboard('${email}')"></i>
                <i class="fas fa-check" style="color:green;"></i>
              </span>
            </div>
            <div class="request-item">
            <p>${email.RequestMessage}</p>
            </div>
          `;
      });
    }

    modal.style.display = "flex";

  } catch (err) {
    console.error(err);
    requestList.innerHTML = "<p>Fout bij laden van verzoeken.</p>";
    modal.style.display = "flex";
  }
}

function closeModal() {
  document.getElementById("requestModal").style.display = "none";
}

function copyToClipboard(text) {
  navigator.clipboard.writeText(text).then(() => {
    alert(`Gekopieerd: ${text}`);
  });
}

loadVillas();