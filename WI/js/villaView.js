async function getVilla(id) {
    let data = await VillaRequests.getVillaByID(id);
    if (data.success == false) {
        console.error("Failed to get villa");
        window.location.href = "index.html";
        return;
    }

    let villa = JSON.parse(data.data.Villa);

    document.getElementById("villaName").innerText = villa.Name;
    document.getElementById("villaDescription").innerText = villa.Description;
    document.getElementById("villaImage").src = villa.VillaMainImagePath;
    document.querySelector("#villaPrice p").innerText = `${villa.Price} per nacht`;
    document.querySelector("#villaLocation p").innerText = villa.Location;
    document.querySelector("#villaCapacity p").innerText = `${villa.Capacity} personen`;
    document.querySelector("#villaBedrooms p").innerText = villa.Bedrooms;
    document.querySelector("#villaBathrooms p").innerText = villa.Bathrooms;

    villa.VillaImagePaths.forEach(villaImage => {
        let img = document.createElement("img");
        img.src = villaImage;
        document.getElementById("fotoList").appendChild(img);
    });
}

let path = ""

function downloadPDF() {
    if (downloadButton.classList.contains("disabled")) {
        return;
    }
    window.open(path, "_blank");
}


async function setPdfPath(id) {
    let pdfPath = await PDFRequests.generatePDF(id);
    console.log(pdfPath);
    if (pdfPath.success == false) {
        console.error("Failed to get pdf path");
        return;
    }
    console.log(pdfPath.data);
    path = pdfPath.data.PDF;
    downloadButton.classList.remove("disabled");
}
const downloadButton = document.getElementById("flyerButton");
downloadButton.classList.add("disabled");

// get id from url
let url = new URL(window.location.href);
let id = url.searchParams.get("villaID");
// convert id to number
getVilla(Number(id));
setPdfPath(Number(id));