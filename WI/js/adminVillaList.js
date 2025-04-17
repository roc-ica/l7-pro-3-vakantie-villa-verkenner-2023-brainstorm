async function checkLogin() {
  const isLoggedIn = await AdminRequest.IsLoggedIn();

  if (isLoggedIn.success === false) { // kan het niet (!isLoggedIn.success) ?
    window.location.href = 'AdminLogin.html';
  }
}

checkLogin();

let data = [];
class VillaHandler {
  constructor() {
    this.Villas = [];
  }

  getRequests(villaID) {
    const villa = this.Villas.find(v => v.VillaID === villaID);

    if (!villa) return [];

    return villa.Requests;
  }

  deleteRequest(villaID, requestID) {
    const villa = this.Villas.find(v => v.VillaID === villaID);

    if (!villa) return [];

    let success = villa.deleteRequest(requestID);
    console.log("success", success);
    console.log("refreshing request modal");
    openRequestModal(villaID);// open the request modal to show the requests
    loadVillas(false);// reload the villa list without fetching data again

    return villa.Requests;
  }

  deleteVilla(villaID) {
    console.log("deleting villa", villaID);
    const villaIndex = this.Villas.findIndex(v => parseInt(v.VillaID) === parseInt(villaID));

    console.log("villaIndex", villaIndex);

    if (villaIndex === -1) return false;

    this.Villas.splice(villaIndex, 1);
    console.log("deleted villa", villaID);
    loadVillas(false); // reload the villa list without fetching data again

    return true;
  }

  addVilla(villa) {
    this.Villas.push(villa);
  }

  clear() {
    this.Villas = [];
  }
}

class Villa {
  constructor(villaID, name, location, price, VillaImagePath, requests) {
    this.VillaID = villaID;
    this.Name = name;
    this.Location = location;
    this.Price = price;
    this.VillaImagePath = VillaImagePath;
    this.Requests = requests;
  }

  deleteRequest(requestID) {
    console.log("deleting request", requestID);
    console.log("requests", this.Requests);

    const request = this.Requests.find(r => r.RequestID === parseInt(requestID));

    if (!request) return false;

    this.Requests = this.Requests.filter(r => r.RequestID !== parseInt(requestID));

    return true;
  }
}

class Request {
  constructor(requestID, email, requestMessage) {
    this.RequestID = requestID;
    this.Email = email;
    this.RequestMessage = requestMessage;
  }
}

const Data = new VillaHandler();

async function loadVillas(getData = true) {
  if (getData) {
    Data.clear();
    data = await VillaRequests.getAdminVillas();
    const villas = JSON.parse(data.data.Villas);

    villas.forEach(villa => {
      let item = new Villa(villa.VillaID, villa.Name, villa.Location, villa.Price, villa.VillaImagePath, []);
      item.Requests = villa.Requests.map(request => {
        return {
          RequestID: request.RequestID,
          Email: request.Email,
          RequestMessage: request.RequestMessage
        };
      });

      Data.addVilla(item);
    });
  }

  const container = document.getElementById("villaContainer");

  //remove all children divs with class card
  const cards = container.querySelectorAll(".card");
  cards.forEach(card => {
    card.remove();
  });

  let tempCSS = document.createElement("style");
  Data.Villas.forEach(villa => {
    container.innerHTML += 
    `<div class="card">
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
          <button class="btn btn-delete" data-id="${villa.VillaID}">Verwijderen</button>
        </div>`;

    tempCSS.innerHTML += 
    `#count_${villa.VillaID}::after {
        content: "${villa.Requests.length}";
    }`;
  });

  document.head.appendChild(tempCSS);

  document.querySelectorAll('.btn-delete').forEach(button => {
    button.addEventListener('click', () => {
      document.getElementById('deleteModal').style.display = 'flex';
      document.getElementById('deleteConfirm').setAttribute('data-id', button.getAttribute('data-id'));
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
  const requests = Data.getRequests(villaId);

  try {
    requestList.innerHTML = "";
  
    if (requests.length === 0) {
      requestList.innerHTML = "<p>Geen verzoeken gevonden.</p>";
    } else {
      requests.forEach(email => {
        console.log(email);
        requestList.innerHTML += 
        `<div class="request-item">
            <span style="color: var(--text-color-dark);">${email.Email}</span>          
            <i class="fas fa-copy" style="cursor:pointer;" onclick="closeRequest('${email.RequestID}', '${villaId}')" title="Verwijder verzoek">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20">
                <path d="M1 10 6 16 18 5" fill="none" opacity="1" stroke-width="3" stroke="lime" stroke-linejoin="round" stroke-linecap="round" />
              </svg>
            </i>
          </div>
          <div class="request-item">
            <p style="color: var(--text-color-dark);">${email.RequestMessage}</p>
          </div>`;
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

async function confirmDelete() {
  const villaId = parseInt(document.getElementById('deleteConfirm').getAttribute('data-id'));
  
  Data.deleteVilla(villaId);
  await VillaRequests.deleteVilla(villaId);
  closeDeleteModal();
}

function closeModal() {
  document.getElementById("requestModal").style.display = "none";
}

async function closeRequest(id, villaId) {
  villaId = parseInt(villaId);
  Data.deleteRequest(villaId, id);
  await VillaRequests.deleteRequest(villaId, id);
}

loadVillas();