async function getVilla(id) {
    let data = await VillaRequests.getVillasByIDs([id]);
    if (data.success == false) {
        console.error("Failed to get villa");
        window.location.href = "index.html";
        return;
    }
    console.log(data);
    let villa = JSON.parse(data.data.Villas)[0];
    console.log(villa);
    document.getElementById("villaName").innerText = villa.Name;
    document.getElementById("villaDescription").innerText = villa.Description;
    document.getElementById("villaImage").src = `Assets\\villas\\${villa.VillaImagePath}`;//TODO: change to correct path
    document.querySelector("#villaPrice p").innerText = `${villa.Price} per nacht`;
    document.querySelector("#villaLocation p").innerText = villa.Location;
    document.querySelector("#villaCapacity p").innerText = `${villa.Capacity} personen`;
    document.querySelector("#villaBedrooms p").innerText = villa.Bedrooms;
    document.querySelector("#villaBathrooms p").innerText = villa.Bathrooms;
}


// get id from url
let url = new URL(window.location.href);
let id = url.searchParams.get("villaID");
getVilla(id);