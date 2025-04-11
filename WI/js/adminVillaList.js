async function checkLogin() {
    const IsLoggedIn = await AdminRequest.IsLoggedIn();
    if (IsLoggedIn.success === false) {
        window.location.href = 'AdminLogin.html';
    }
        const data = await VillaRequests.getAdminVillas();
        console.log(JSON.parse(data.data.Villas));
}
checkLogin();

async function loadVillas() {
    const data = await VillaRequests.getAdminVillas();
    const villas = JSON.parse(data.data.Villas);
    console.log(villas);
    const container = document.getElementById("villaContainer");

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
            <button class="btn btn-edit">Bewerken</button>
            <button class="btn btn-request">Request</button>
          </div>
          <button class="btn btn-delete">Verwijderen</button>
        </div>
      `;
    });
  }

  loadVillas();