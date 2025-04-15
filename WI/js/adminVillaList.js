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
          <div class="info">
            <h3>${villa.Name}</h3>
            <p>${villa.Location}</p>
            <div class="price">€${villa.Price},-</div>
          </div>
          <div class="actions">
            <small>ACTIES</small>
<button class="btn btn-edit" onclick="editVilla(${villa.VillaID})">Bewerken</button>
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

  document.querySelectorAll('.btn-delete').forEach(button => {
    button.addEventListener('click', () => {
      document.getElementById('deleteModal').style.display = 'flex';
    });
  });

}

function editVilla(villaID) {
  window.location.href = `adminEditVilla.html?villaID=${villaID}`;
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
                <span style="color: var(--text-color-dark);">${email.Email}</span>          
                <i class="fas fa-copy" style="cursor:pointer;" onclick="closeRequest('${email.RequestID}', '${villaId}')" title="Verwijder verzoek">
                
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20">
	<path d="M1 10 6 16 18 5" fill="none" opacity="1" stroke-width="3" stroke="lime" stroke-linejoin="round" stroke-linecap="round" />
</svg>
                </i>
              </span>
            </div>
            <div class="request-item">
    <p style="color: var(--text-color-dark);">${email.RequestMessage}</p>
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


function closeDeleteModal() {
  document.getElementById('deleteModal').style.display = 'none';
}

function confirmDelete() {
  console.log('Verwijder-actie');
  closeDeleteModal();
}

function closeModal() {
  document.getElementById("requestModal").style.display = "none";
}

function closeRequest(id, villaId) {
  console.log(id);
  // remove request from local storage to prevent from having to reload the page
  villaId = parseInt(villaId);


  // closeModal();
  // openRequestModal(id);
}

loadVillas();