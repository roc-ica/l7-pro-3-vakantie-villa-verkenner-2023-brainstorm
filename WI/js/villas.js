async function getVillas() {
    const villas = await VillaRequests.getVillas();
    if (villas.success === false) {
        console.log(villas.error);
        return;
    }
    let villaData = JSON.parse(villas.data.Villas);
    document.getElementsByClassName('villaList')[0].innerHTML = '';
    for (let x = 0; x < 5; x++) {
        for (let i = 0; i < villaData.length; i++) {
            const villa = new SmallVilla(villaData[i]);
            document.getElementsByClassName('villaList')[0].innerHTML += villa.html;
        }
    }
}
getVillas();